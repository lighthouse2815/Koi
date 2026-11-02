using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers;

public class OrderController(OrderService orders) : Controller
{
    public async Task<IActionResult> Index(Guid? categoryId, Guid? drinkId, int page = 1)
    {
        var model = await orders.GetPageAsync(orders.GetCart(HttpContext.Session), categoryId, drinkId, page);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SelectBranch(Guid storeId, Guid? categoryId, Guid? drinkId)
    {
        if (!await orders.BranchAcceptsOrdersAsync(storeId)) return BadRequest("Chi nhánh không nhận đơn.");
        var cart = orders.GetCart(HttpContext.Session);
        if (cart.StoreId != storeId) { cart.StoreId = storeId; cart.OrderId = Guid.NewGuid(); }
        orders.SaveCart(HttpContext.Session, cart);
        return RedirectToAction(nameof(Index), new { categoryId, drinkId });
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid id, int quantity = 1, Guid? sizeId = null, Guid[]? toppingIds = null, int? sugar = null, int? ice = null, Guid? categoryId = null, Guid? drinkId = null)
    {
        if (sizeId.HasValue || toppingIds is { Length: > 0 } || sugar.HasValue || ice.HasValue) return BadRequest("Các lựa chọn cũ không còn được hỗ trợ. Vui lòng tải lại trang.");
        var cart = orders.GetCart(HttpContext.Session);
        if (!await orders.BranchAcceptsOrdersAsync(cart.StoreId)) { TempData["OrderMessage"] = "Hãy chọn chi nhánh đang nhận đơn."; return RedirectToAction(nameof(Index)); }
        var drink = await orders.GetAvailableDrinkAsync(id);
        if (drink == null || quantity < 1 || quantity > 20) return BadRequest("Đồ uống hoặc lựa chọn không hợp lệ.");
        var existing = cart.Items.Find(line => line.DrinkId == id && line.SizeId == null && line.ToppingIds.Count == 0 && string.IsNullOrEmpty(line.Customization));
        if (existing != null)
        {
            if (existing.Quantity + quantity > 20) { TempData["OrderMessage"] = "Mỗi lựa chọn chỉ được đặt tối đa 20 ly."; return RedirectToAction(nameof(Index), new { categoryId, drinkId }); }
            existing.Quantity += quantity;
        }
        else
        {
            if (cart.Items.Count >= 20) { TempData["OrderMessage"] = "Giỏ hàng chỉ hỗ trợ tối đa 20 lựa chọn."; return RedirectToAction(nameof(Index)); }
            cart.Items.Add(new CartItem { DrinkId = drink.Id, Quantity = quantity, Price = drink.Price!.Value });
        }
        orders.SaveCart(HttpContext.Session, cart);
        TempData["OrderMessage"] = "Đã thêm đồ uống vào giỏ hàng.";
        return RedirectToAction(nameof(Index), null, new { categoryId, drinkId }, "drinks");
    }

    [HttpPost]
    public IActionResult Update(Guid id, int quantity)
    {
        var cart = orders.GetCart(HttpContext.Session);
        var line = cart.Items.Find(value => value.LineId == id);
        if (line == null) return NotFound();
        if (quantity < 0 || quantity > 20) return BadRequest("Số lượng không hợp lệ.");
        if (quantity == 0) cart.Items.Remove(line); else line.Quantity = quantity;
        orders.SaveCart(HttpContext.Session, cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutViewModel checkout)
    {
        var cart = orders.GetCart(HttpContext.Session);
        if (!await orders.CheckoutAsync(cart, checkout, ModelState))
        {
            orders.SaveCart(HttpContext.Session, cart);
            Response.StatusCode = 400;
            var model = await orders.GetPageAsync(cart);
            model.Checkout = checkout;
            return View("Index", model);
        }
        HttpContext.Session.Remove("shopping-cart");
        HttpContext.Session.SetString("order-reference", cart.OrderId.ToString());
        return RedirectToAction(nameof(Complete));
    }

    public IActionResult Complete()
    {
        var reference = HttpContext.Session.GetString("order-reference");
        if (reference == null) return RedirectToAction(nameof(Index));
        HttpContext.Session.Remove("order-reference");
        return View((object)reference);
    }

}
