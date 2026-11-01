using System.Text.Json;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers;

public class OrderController(ApplicationDbContext database) : Controller
{
    private const string CartKey = "shopping-cart";
    private ShoppingCart GetCart()
    {
        var value = HttpContext.Session.GetString(CartKey);
        return value == null ? new ShoppingCart() : JsonSerializer.Deserialize<ShoppingCart>(value) ?? new ShoppingCart();
    }
    private void SaveCart(ShoppingCart cart) => HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));

    public async Task<IActionResult> Index(Guid? categoryId, Guid? drinkId, int page = 1)
    {
        var model = await Page(GetCart(), categoryId, drinkId, page);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SelectBranch(Guid storeId, Guid? categoryId, Guid? drinkId)
    {
        if (!await database.Stores.AnyAsync(value => value.Id == storeId && value.IsPublished && value.AcceptsOrders)) return BadRequest("Chi nhánh không nhận đơn.");
        var cart = GetCart();
        if (cart.StoreId != storeId) { cart.StoreId = storeId; cart.OrderId = Guid.NewGuid(); }
        SaveCart(cart);
        return RedirectToAction(nameof(Index), new { categoryId, drinkId });
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid id, int quantity = 1, Guid? sizeId = null, Guid[]? toppingIds = null, int sugar = 0, int ice = 0, Guid? categoryId = null, Guid? drinkId = null)
    {
        if (sizeId.HasValue || toppingIds is { Length: > 0 }) return BadRequest("Kích cỡ và topping không còn được hỗ trợ. Vui lòng tải lại trang.");
        var cart = GetCart();
        if (!await database.Stores.AnyAsync(value => value.Id == cart.StoreId && value.IsPublished && value.AcceptsOrders)) { TempData["OrderMessage"] = "Hãy chọn chi nhánh đang nhận đơn."; return RedirectToAction(nameof(Index)); }
        var drink = await database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id && value.IsPublished && value.Category.IsPublished && value.Price > 0);
        if (drink == null || quantity < 1 || quantity > 20 || sugar is < 0 or > 2 || ice is < 0 or > 2) return BadRequest("Đồ uống hoặc lựa chọn không hợp lệ.");
        var existing = cart.Items.Find(line => line.DrinkId == id && line.SizeId == null && line.ToppingIds.Count == 0 && line.Sugar == sugar && line.Ice == ice);
        if (existing != null)
        {
            if (existing.Quantity + quantity > 20) { TempData["OrderMessage"] = "Mỗi lựa chọn chỉ được đặt tối đa 20 ly."; return RedirectToAction(nameof(Index), new { categoryId, drinkId }); }
            existing.Quantity += quantity;
        }
        else
        {
            if (cart.Items.Count >= 20) { TempData["OrderMessage"] = "Giỏ hàng chỉ hỗ trợ tối đa 20 lựa chọn."; return RedirectToAction(nameof(Index)); }
            cart.Items.Add(new CartItem { DrinkId = drink.Id, Quantity = quantity, Price = drink.Price!.Value, Sugar = sugar, Ice = ice, Customization = Describe(sugar, ice) });
        }
        SaveCart(cart);
        TempData["OrderMessage"] = "Đã thêm đồ uống vào giỏ hàng.";
        return RedirectToAction(nameof(Index), null, new { categoryId, drinkId }, "drinks");
    }

    private static string Describe(int sugar, int ice)
    {
        var levels = new[] { "Bình thường", "Ít", "Không" };
        return $"Đường: {levels[sugar]} · Đá: {levels[ice]}";
    }

    [HttpPost]
    public IActionResult Update(Guid id, int quantity)
    {
        var cart = GetCart();
        var line = cart.Items.Find(value => value.LineId == id);
        if (line == null) return NotFound();
        if (quantity < 0 || quantity > 20) return BadRequest("Số lượng không hợp lệ.");
        if (quantity == 0) cart.Items.Remove(line); else line.Quantity = quantity;
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutViewModel checkout)
    {
        var cart = GetCart();
        if (!Enum.IsDefined(checkout.Fulfillment)) ModelState.AddModelError("Checkout.Fulfillment", "Chọn cách nhận hàng hợp lệ.");
        if (checkout.Fulfillment == FulfillmentMethod.Delivery && string.IsNullOrWhiteSpace(checkout.DeliveryAddress)) ModelState.AddModelError("Checkout.DeliveryAddress", "Nhập địa chỉ giao hàng.");
        var branch = await database.Stores.AsNoTracking().SingleOrDefaultAsync(value => value.Id == cart.StoreId && value.IsPublished && value.AcceptsOrders);
        if (branch == null) ModelState.AddModelError("", "Chi nhánh không còn nhận đơn. Vui lòng chọn chi nhánh khác.");
        if (cart.Items.Count == 0) ModelState.AddModelError("", "Giỏ hàng đang trống.");
        var ids = cart.Items.Select(value => value.DrinkId).ToArray();
        var drinks = await database.Drinks.AsNoTracking().Where(value => ids.Contains(value.Id) && value.IsPublished && value.Category.IsPublished && value.Price > 0).ToDictionaryAsync(value => value.Id);
        if (drinks.Count != ids.Distinct().Count()) ModelState.AddModelError("", "Có món không còn được bán. Vui lòng xóa món đó khỏi giỏ hàng.");
        var changed = false;
        foreach (var line in cart.Items)
        {
            if (line.SizeId.HasValue || line.ToppingIds.Count > 0)
            {
                ModelState.AddModelError("", "Kích cỡ và topping không còn được hỗ trợ. Vui lòng xóa món cũ và chọn lại.");
                continue;
            }
            if (drinks.TryGetValue(line.DrinkId, out var drink))
            {
                var price = drink.Price!.Value;
                var description = Describe(line.Sugar, line.Ice);
                if (price != line.Price || description != line.Customization) { line.Price = price; line.Customization = description; changed = true; }
            }
        }
        if (changed) { SaveCart(cart); ModelState.AddModelError("", "Giá đã thay đổi. Vui lòng kiểm tra lại giỏ hàng và xác nhận đặt hàng lần nữa."); }
        if (!ModelState.IsValid) { Response.StatusCode = 400; var model = await Page(cart); model.Checkout = checkout; return View("Index", model); }
        var order = new Order
        {
            Id = cart.OrderId, StoreId = branch!.Id, StoreName = branch.Name, StoreAddress = branch.Address,
            CustomerName = checkout.CustomerName.Trim(), CustomerPhone = checkout.CustomerPhone.Trim(),
            Fulfillment = checkout.Fulfillment, DeliveryAddress = checkout.Fulfillment == FulfillmentMethod.Delivery ? checkout.DeliveryAddress!.Trim() : "",
            Note = checkout.Note?.Trim() ?? "",
            Items = cart.Items.Select(line => new OrderItem { DrinkId = line.DrinkId, DrinkName = drinks[line.DrinkId].Name, Customization = line.Customization, UnitPrice = line.Price, Quantity = line.Quantity }).ToList()
        };
        if (!await database.Orders.AnyAsync(value => value.Id == order.Id))
        {
            database.Orders.Add(order);
            try { await database.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                database.Entry(order).State = EntityState.Detached;
                if (!await database.Orders.AsNoTracking().AnyAsync(value => value.Id == order.Id)) throw;
            }
        }
        HttpContext.Session.Remove(CartKey);
        HttpContext.Session.SetString("order-reference", order.Id.ToString());
        return RedirectToAction(nameof(Complete));
    }

    public IActionResult Complete()
    {
        var reference = HttpContext.Session.GetString("order-reference");
        if (reference == null) return RedirectToAction(nameof(Index));
        HttpContext.Session.Remove("order-reference");
        return View((object)reference);
    }

    private async Task<OrderingPageViewModel> Page(ShoppingCart cart, Guid? categoryId = null, Guid? drinkId = null, int page = 1)
    {
        var branches = await database.Stores.AsNoTracking().Where(value => value.IsPublished && value.AcceptsOrders).OrderBy(value => value.Name).ToArrayAsync();
        var query = database.Drinks.AsNoTracking().Include(value => value.Category).Where(value => value.IsPublished && value.Category.IsPublished);
        if (drinkId.HasValue) query = query.Where(value => value.Id == drinkId);
        else if (categoryId.HasValue) query = query.Where(value => value.CategoryId == categoryId);
        const int pageSize = 12;
        var drinkCount = await query.CountAsync();
        var pageCount = Math.Max(1, (int)Math.Ceiling(drinkCount / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        var drinks = await query.OrderBy(value => value.Category.SortOrder).ThenBy(value => value.CategoryId)
            .ThenBy(value => value.SortOrder).ThenBy(value => value.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync();
        var categories = await database.DrinkCategories.AsNoTracking().Where(value => value.IsPublished && value.Drinks.Any(drink => drink.IsPublished))
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        var filterName = drinkId.HasValue ? null : categoryId.HasValue
            ? await database.DrinkCategories.AsNoTracking().Where(value => value.Id == categoryId && value.IsPublished).Select(value => value.Name).SingleOrDefaultAsync() : null;
        var ids = cart.Items.Select(line => line.DrinkId).ToArray();
        var allCartDrinks = await database.Drinks.AsNoTracking().Where(value => ids.Contains(value.Id)).ToDictionaryAsync(value => value.Id);
        return new OrderingPageViewModel
        {
            Branches = branches, Branch = branches.FirstOrDefault(value => value.Id == cart.StoreId), Drinks = drinks, Cart = cart,
            CategoryId = categoryId, DrinkId = drinkId, FilterName = filterName, Categories = categories,
            Page = page, DrinkCount = drinkCount, HasMoreDrinks = page < pageCount,
            Lines = cart.Items.Select(line => new CartLine(line.LineId, allCartDrinks.TryGetValue(line.DrinkId, out var d) ? (d.Name) : "Món không còn tồn tại", line.Customization, line.Quantity, line.Price)).ToList()
        };
    }
}
