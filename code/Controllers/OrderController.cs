using Koi.Web.Models;
using Koi.Web.Exceptions;
using Koi.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers;

public class OrderController(OrderService orders) : Controller
{
    public async Task<IActionResult> Index(Guid? categoryId, Guid? drinkId, int page = 1, string? search = null)
    {
        var model = await orders.GetPageAsync(orders.GetCart(HttpContext.Session), categoryId, drinkId, page, search);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SelectBranch(Guid storeId, Guid? categoryId, Guid? drinkId, string? search = null)
    {
        if (!await orders.SelectBranchAsync(HttpContext.Session, storeId)) return BadRequest("Chi nhánh không nhận đơn.");
        return RedirectToAction(nameof(Index), null, new { categoryId, drinkId, search }, "drinks");
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid id, int quantity = 1, Guid? sizeId = null, Guid[]? toppingIds = null, int? sugar = null, int? ice = null, Guid? categoryId = null, Guid? drinkId = null, string? search = null)
    {
        var result = await orders.AddAsync(HttpContext.Session, id, quantity, sizeId, toppingIds, sugar, ice);
        if (result.Status == "bad") return BadRequest(result.Message);
        TempData["OrderMessage"] = result.Message;
        return RedirectToAction(nameof(Index), null, new { categoryId, drinkId, search }, "drinks");
    }

    [HttpPost]
    public IActionResult Update(Guid id, int quantity, Guid? categoryId = null, Guid? drinkId = null, string? search = null)
    {
        var result = orders.Update(HttpContext.Session, id, quantity);
        if (result == "missing") return NotFound();
        if (result == "invalid") return BadRequest("Số lượng không hợp lệ.");
        return RedirectToAction(nameof(Index), null, new { categoryId, drinkId, search }, "cart");
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutViewModel checkout, Guid? categoryId = null, Guid? drinkId = null, string? search = null)
    {
        var cart = orders.GetCart(HttpContext.Session);
        var completed = false;
        try
        {
            completed = await orders.CheckoutAsync(cart, checkout, ModelState.IsValid);
        }
        catch (CheckoutException exception)
        {
            foreach (var (field, error) in exception.Errors)
                ModelState.AddModelError(field, error.Message);
        }
        if (!completed)
        {
            orders.SaveCart(HttpContext.Session, cart);
            Response.StatusCode = 400;
            var model = await orders.GetPageAsync(cart, categoryId, drinkId, search: search);
            model.Checkout = checkout;
            return View("Index", model);
        }
        orders.CompleteCheckout(HttpContext.Session, cart.OrderId);
        orders.RememberOrder(HttpContext, cart.OrderId);
        return RedirectToAction(nameof(Complete));
    }

    public IActionResult Complete()
    {
        var reference = orders.TakeOrderReference(HttpContext.Session);
        if (reference == null) return RedirectToAction(nameof(Index));
        return View((object)reference);
    }

    public async Task<IActionResult> History() => View(await orders.GetHistoryAsync(Request));

}
