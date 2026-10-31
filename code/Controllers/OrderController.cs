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

    public async Task<IActionResult> Index()
    {
        var cart = GetCart();
        var model = await Page(cart);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SelectBranch(Guid storeId)
    {
        if (!await database.Stores.AnyAsync(value => value.Id == storeId && value.IsPublished && value.AcceptsOrders)) return BadRequest("Chi nhánh không nhận đơn.");
        var cart = GetCart();
        if (cart.StoreId != storeId) { cart.StoreId = storeId; cart.OrderId = Guid.NewGuid(); }
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid id, int quantity = 1)
    {
        var cart = GetCart();
        if (!await database.Stores.AnyAsync(value => value.Id == cart.StoreId && value.IsPublished && value.AcceptsOrders)) { TempData["OrderMessage"] = "Hãy chọn chi nhánh đang nhận đơn."; return RedirectToAction(nameof(Index)); }
        var drink = await database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id && value.IsPublished && value.Category.IsPublished && value.Price > 0);
        if (drink == null || quantity < 1 || quantity > 20) return BadRequest("Đồ uống hoặc số lượng không hợp lệ.");
        var line = cart.Items.Find(value => value.DrinkId == id);
        if (line == null)
        {
            if (cart.Items.Count >= 20) { TempData["OrderMessage"] = "Giỏ hàng chỉ hỗ trợ tối đa 20 món khác nhau."; return RedirectToAction(nameof(Index)); }
            cart.Items.Add(new CartItem { DrinkId = drink.Id, Quantity = quantity, Price = drink.Price!.Value });
        }
        else
        {
            if (line.Quantity + quantity > 20) { TempData["OrderMessage"] = "Mỗi món chỉ có thể đặt tối đa 20 ly."; return RedirectToAction(nameof(Index)); }
            line.Quantity += quantity;
        }
        SaveCart(cart);
        TempData["OrderMessage"] = "Đã thêm đồ uống vào giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Update(Guid id, int quantity)
    {
        var cart = GetCart();
        var line = cart.Items.Find(value => value.DrinkId == id);
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
        if (drinks.Count != cart.Items.Count) ModelState.AddModelError("", "Có món không còn được bán. Vui lòng xóa món đó khỏi giỏ hàng.");
        var changed = false;
        foreach (var line in cart.Items)
            if (drinks.TryGetValue(line.DrinkId, out var drink) && drink.Price != line.Price) { line.Price = drink.Price!.Value; changed = true; }
        if (changed) { SaveCart(cart); ModelState.AddModelError("", "Giá đã thay đổi. Vui lòng kiểm tra lại giỏ hàng và xác nhận đặt hàng lần nữa."); }
        if (!ModelState.IsValid) { Response.StatusCode = 400; var model = await Page(cart); model.Checkout = checkout; return View("Index", model); }
        var order = new Order
        {
            Id = cart.OrderId, StoreId = branch!.Id, StoreName = branch.Name, StoreAddress = branch.Address,
            CustomerName = checkout.CustomerName.Trim(), CustomerPhone = checkout.CustomerPhone.Trim(),
            Fulfillment = checkout.Fulfillment, DeliveryAddress = checkout.Fulfillment == FulfillmentMethod.Delivery ? checkout.DeliveryAddress!.Trim() : "",
            Note = checkout.Note?.Trim() ?? "",
            Items = cart.Items.Select(line => new OrderItem { DrinkName = string.IsNullOrWhiteSpace(drinks[line.DrinkId].NameVi) ? drinks[line.DrinkId].Name : drinks[line.DrinkId].NameVi, UnitPrice = line.Price, Quantity = line.Quantity }).ToList()
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
        TempData["OrderReference"] = order.Id.ToString();
        return RedirectToAction(nameof(Complete));
    }

    public IActionResult Complete() => TempData["OrderReference"] is string reference ? View((object)reference) : RedirectToAction(nameof(Index));

    private async Task<OrderingPageViewModel> Page(ShoppingCart cart)
    {
        var branches = await database.Stores.AsNoTracking().Include(value => value.Market).Where(value => value.IsPublished && value.AcceptsOrders).OrderBy(value => value.Name).ToArrayAsync();
        var drinks = await database.Drinks.AsNoTracking().Include(value => value.Category).Where(value => value.IsPublished && value.Category.IsPublished && value.Price > 0).OrderBy(value => value.Category.SortOrder).ThenBy(value => value.SortOrder).ToArrayAsync();
        var ids = cart.Items.Select(line => line.DrinkId).ToArray();
        var allCartDrinks = await database.Drinks.AsNoTracking().Where(value => ids.Contains(value.Id)).ToDictionaryAsync(value => value.Id);
        return new OrderingPageViewModel
        {
            Branches = branches, Branch = branches.FirstOrDefault(value => value.Id == cart.StoreId), Drinks = drinks, Cart = cart,
            Lines = cart.Items.Select(line => new CartLine(line.DrinkId, allCartDrinks.TryGetValue(line.DrinkId, out var d) ? (string.IsNullOrWhiteSpace(d.NameVi) ? d.Name : d.NameVi) : "Món không còn tồn tại", line.Quantity, line.Price)).ToList()
        };
    }
}
