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

    public async Task<IActionResult> Index(Guid? categoryId, Guid? drinkId)
    {
        var model = await Page(GetCart(), categoryId, drinkId);
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
        var cart = GetCart();
        if (!await database.Stores.AnyAsync(value => value.Id == cart.StoreId && value.IsPublished && value.AcceptsOrders)) { TempData["OrderMessage"] = "Hãy chọn chi nhánh đang nhận đơn."; return RedirectToAction(nameof(Index)); }
        var drink = await database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id && value.IsPublished && value.Category.IsPublished && value.Price > 0);
        toppingIds ??= [];
        if (drink == null || quantity < 1 || quantity > 20 || sugar is < 0 or > 2 || ice is < 0 or > 2 || toppingIds.Length > 5 || toppingIds.Distinct().Count() != toppingIds.Length) return BadRequest("Đồ uống hoặc lựa chọn không hợp lệ.");
        if (cart.Items.Count >= 20) { TempData["OrderMessage"] = "Giỏ hàng chỉ hỗ trợ tối đa 20 lựa chọn."; return RedirectToAction(nameof(Index)); }
        var options = await database.SaleOptions.AsNoTracking().Where(value => value.IsActive).ToArrayAsync();
        var size = sizeId.HasValue ? options.SingleOrDefault(value => value.Id == sizeId && value.Kind == SaleOptionKind.Size) : null;
        var toppings = options.Where(value => toppingIds.Contains(value.Id) && value.Kind == SaleOptionKind.Topping).OrderBy(value => value.Name).ToArray();
        if ((options.Any(value => value.Kind == SaleOptionKind.Size) && size == null) || (sizeId.HasValue && size == null) || toppings.Length != toppingIds.Length) return BadRequest("Kích cỡ hoặc topping không hợp lệ.");
        cart.Items.Add(new CartItem { DrinkId = drink.Id, Quantity = quantity, Price = drink.Price!.Value + (size?.ExtraPrice ?? 0) + toppings.Sum(value => value.ExtraPrice), SizeId = sizeId, ToppingIds = toppingIds.ToList(), Sugar = sugar, Ice = ice, Customization = Describe(size, toppings, sugar, ice) });
        SaveCart(cart);
        TempData["OrderMessage"] = "Đã thêm đồ uống vào giỏ hàng.";
        return RedirectToAction(nameof(Index), null, new { categoryId, drinkId }, "drinks");
    }

    private static string Describe(SaleOption? size, SaleOption[] toppings, int sugar, int ice)
    {
        var levels = new[] { "Bình thường", "Ít", "Không" };
        return string.Join(" · ", new[] { size == null ? null : $"Cỡ: {size.Name}", $"Đường: {levels[sugar]}", $"Đá: {levels[ice]}", toppings.Length == 0 ? null : $"Topping: {string.Join(", ", toppings.Select(value => value.Name))}" }.Where(value => value != null));
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
        var options = await database.SaleOptions.AsNoTracking().Where(value => value.IsActive).ToArrayAsync();
        var changed = false;
        foreach (var line in cart.Items)
        {
            var size = line.SizeId.HasValue ? options.SingleOrDefault(value => value.Id == line.SizeId && value.Kind == SaleOptionKind.Size) : null;
            var toppings = options.Where(value => line.ToppingIds.Contains(value.Id) && value.Kind == SaleOptionKind.Topping).OrderBy(value => value.Name).ToArray();
            if ((options.Any(value => value.Kind == SaleOptionKind.Size) && size == null) || (line.SizeId.HasValue && size == null) || toppings.Length != line.ToppingIds.Count)
            {
                ModelState.AddModelError("", "Kích cỡ hoặc topping không còn được bán. Vui lòng xóa món và chọn lại.");
                continue;
            }
            if (drinks.TryGetValue(line.DrinkId, out var drink))
            {
                var price = drink.Price!.Value + (size?.ExtraPrice ?? 0) + toppings.Sum(value => value.ExtraPrice);
                var description = Describe(size, toppings, line.Sugar, line.Ice);
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
            Items = cart.Items.Select(line => new OrderItem { DrinkName = string.IsNullOrWhiteSpace(drinks[line.DrinkId].NameVi) ? drinks[line.DrinkId].Name : drinks[line.DrinkId].NameVi, Customization = line.Customization, UnitPrice = line.Price, Quantity = line.Quantity }).ToList()
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

    private async Task<OrderingPageViewModel> Page(ShoppingCart cart, Guid? categoryId = null, Guid? drinkId = null)
    {
        var branches = await database.Stores.AsNoTracking().Where(value => value.IsPublished && value.AcceptsOrders).OrderBy(value => value.Name).ToArrayAsync();
        var query = database.Drinks.AsNoTracking().Include(value => value.Category).Where(value => value.IsPublished && value.Category.IsPublished);
        if (drinkId.HasValue) query = query.Where(value => value.Id == drinkId);
        else if (categoryId.HasValue) query = query.Where(value => value.CategoryId == categoryId);
        var drinks = await query.OrderBy(value => value.Category.SortOrder).ThenBy(value => value.SortOrder).ToArrayAsync();
        var categories = await database.DrinkCategories.AsNoTracking().Where(value => value.IsPublished && value.Drinks.Any(drink => drink.IsPublished))
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();
        var filterName = drinkId.HasValue ? null : categoryId.HasValue
            ? await database.DrinkCategories.AsNoTracking().Where(value => value.Id == categoryId && value.IsPublished).Select(value => value.NameVi != "" ? value.NameVi : value.Name).SingleOrDefaultAsync() : null;
        var options = await database.SaleOptions.AsNoTracking().Where(value => value.IsActive).OrderBy(value => value.Name).ToArrayAsync();
        var ids = cart.Items.Select(line => line.DrinkId).ToArray();
        var allCartDrinks = await database.Drinks.AsNoTracking().Where(value => ids.Contains(value.Id)).ToDictionaryAsync(value => value.Id);
        return new OrderingPageViewModel
        {
            Branches = branches, Branch = branches.FirstOrDefault(value => value.Id == cart.StoreId), Drinks = drinks, Cart = cart,
            CategoryId = categoryId, DrinkId = drinkId, FilterName = filterName, Categories = categories,
            Sizes = options.Where(value => value.Kind == SaleOptionKind.Size).ToArray(), Toppings = options.Where(value => value.Kind == SaleOptionKind.Topping).ToArray(),
            Lines = cart.Items.Select(line => new CartLine(line.LineId, allCartDrinks.TryGetValue(line.DrinkId, out var d) ? (string.IsNullOrWhiteSpace(d.NameVi) ? d.Name : d.NameVi) : "Món không còn tồn tại", line.Customization, line.Quantity, line.Price)).ToList()
        };
    }
}
