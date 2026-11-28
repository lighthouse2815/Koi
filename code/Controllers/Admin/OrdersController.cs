using Koi.Web.Components.Admin;
using Koi.Web.Components.Shared;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class OrdersController(OrderService orders) : Controller
{
    public async Task<IActionResult> Index(Guid? storeId, OrderStatus? status, int page = 1)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value)) return BadRequest();
        return new BlazorViewResult<AdminOrdersPage>(new { Model = await orders.GetAdminListAsync(storeId, status, page) });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, bool preview = false)
    {
        var order = await orders.GetDetailsAsync(id);
        if (order == null) return NotFound();
        return preview
            ? new BlazorViewResult<AdminOrderDetails>(new { Model = order })
            : new BlazorViewResult<AdminOrderDetailsPage>(new { Model = order });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(Guid id, OrderStatus current, OrderStatus next)
    {
        var updated = await orders.ChangeStatusAsync(id, current, next);
        TempData["Status"] = updated
            ? "Đã cập nhật trạng thái đơn hàng."
            : "Đơn hàng đã thay đổi hoặc không còn tồn tại.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
