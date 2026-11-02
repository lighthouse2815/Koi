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
        if (!await orders.SelectBranchAsync(HttpContext.Session, storeId)) return BadRequest("Chi nhánh không nhận đơn.");
        return RedirectToAction(nameof(Index), new { categoryId, drinkId });
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid id, int quantity = 1, Guid? sizeId = null, Guid[]? toppingIds = null, int? sugar = null, int? ice = null, Guid? categoryId = null, Guid? drinkId = null)
    {
        var result = await orders.AddAsync(HttpContext.Session, id, quantity, sizeId, toppingIds, sugar, ice);
        if (result.Status == "bad") return BadRequest(result.Message);
        TempData["OrderMessage"] = result.Message;
        if (result.Status is "branch" or "full") return RedirectToAction(nameof(Index));
        if (result.Status == "quantity") return RedirectToAction(nameof(Index), new { categoryId, drinkId });
        return RedirectToAction(nameof(Index), null, new { categoryId, drinkId }, "drinks");
    }

    [HttpPost]
    public IActionResult Update(Guid id, int quantity)
    {
        var result = orders.Update(HttpContext.Session, id, quantity);
        if (result == "missing") return NotFound();
        if (result == "invalid") return BadRequest("Số lượng không hợp lệ.");
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
        orders.CompleteCheckout(HttpContext.Session, cart.OrderId);
        return RedirectToAction(nameof(Complete));
    }

    public IActionResult Complete()
    {
        var reference = orders.TakeOrderReference(HttpContext.Session);
        if (reference == null) return RedirectToAction(nameof(Index));
        return View((object)reference);
    }

}
