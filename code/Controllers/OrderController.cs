using System.Security.Cryptography;
using Koi.Web.Components.Ordering;
using Koi.Web.Components.Shared;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers;

public class OrderController(
    OrderService orders,
    IDataProtectionProvider protection
) : Controller
{
    private const string CustomerPurpose = "Koi.OrderCustomer";
    private const string ReceiptPurpose = "Koi.OrderReceipt";

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index(Guid? categoryId, Guid? drinkId, int page = 1, string? search = null)
    {
        var model = await orders.GetPageAsync(categoryId, drinkId, page, search);
        return new BlazorViewResult<OrderPage>(new { Model = model, CustomerToken = CreateCustomerToken() });
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> CartLines([FromQuery] Guid[] ids)
    {
        if (!ModelState.IsValid || ids.Length > 20)
            return BadRequest();

        return Json(await orders.GetCartLinesAsync(ids));
    }

    [HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Checkout([FromForm] CheckoutViewModel checkout)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        if (!Guid.TryParse(ReadProtectedValue(CustomerPurpose, checkout.CustomerToken), out var customerId))
            return Unauthorized(new { message = "Mã trình duyệt đã hết hiệu lực. Vui lòng thử lại." });

        try
        {
            var orderId = await orders.CheckoutAsync(checkout, customerId);
            var receipt = protection.CreateProtector(ReceiptPurpose).Protect(orderId.ToString("D"));
            return Json(new { orderId, confirmationUrl = Url.Action(nameof(Complete), new { receipt }) });
        }
        catch (CheckoutException exception)
        {
            foreach (var (field, error) in exception.Errors)
                ModelState.AddModelError(field, error.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet]
    public IActionResult Complete(string? receipt)
    {
        if (!Guid.TryParse(ReadProtectedValue(ReceiptPurpose, receipt), out var orderId))
            return RedirectToAction(nameof(Index));

        return new BlazorViewResult<OrderCompletePage>(new { Model = orderId.ToString("D") });
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult History()
        => new BlazorViewResult<OrderHistoryPage>(new { CustomerToken = CreateCustomerToken() });

    [HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> HistoryOrders([FromForm] string? customerToken)
    {
        if (!Guid.TryParse(ReadProtectedValue(CustomerPurpose, customerToken), out var customerId))
            return Unauthorized();

        return new BlazorViewResult<OrderHistoryList>(new { Model = await orders.GetHistoryAsync(customerId) });
    }

    private string CreateCustomerToken() =>
        protection.CreateProtector(CustomerPurpose).Protect(Guid.NewGuid().ToString("D"));

    private string? ReadProtectedValue(string purpose, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        try
        {
            return protection.CreateProtector(purpose).Unprotect(value);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
