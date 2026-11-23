using System.Security.Cryptography;
using System.Text.Json;
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
    private const string LegacyHistoryCookie = "Koi.OrderHistory";

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index(Guid? categoryId, Guid? drinkId, int page = 1, string? search = null)
    {
        IssueCustomerToken();
        return View(await orders.GetPageAsync(categoryId, drinkId, page, search));
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

        return View((object)orderId.ToString("D"));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult History()
    {
        IssueCustomerToken();
        return View();
    }

    [HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> HistoryOrders([FromForm] string? customerToken)
    {
        if (!Guid.TryParse(ReadProtectedValue(CustomerPurpose, customerToken), out var customerId))
            return Unauthorized();

        var legacyCookie = Request.Cookies[LegacyHistoryCookie];
        var history = await orders.GetHistoryAsync(customerId, ReadLegacyOrderIds(legacyCookie));
        if (legacyCookie != null)
            Response.Cookies.Delete(LegacyHistoryCookie);

        return PartialView("_HistoryOrders", history);
    }

    private void IssueCustomerToken() =>
        ViewData["CustomerToken"] = protection.CreateProtector(CustomerPurpose).Protect(Guid.NewGuid().ToString("D"));

    private Guid[] ReadLegacyOrderIds(string? cookie)
    {
        var json = ReadProtectedValue(LegacyHistoryCookie, cookie);
        if (json == null)
            return [];

        try
        {
            return JsonSerializer.Deserialize<Guid[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

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
