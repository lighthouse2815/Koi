using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class OrdersController(ApplicationDbContext database) : Controller
{
    public async Task<IActionResult> Index(Guid? storeId, OrderStatus? status, int page = 1)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value)) return BadRequest();
        var query = database.Orders.AsNoTracking().Include(value => value.Items).AsQueryable();
        if (storeId.HasValue) query = query.Where(value => value.StoreId == storeId);
        if (status.HasValue) query = query.Where(value => value.Status == status);
        var pageCount = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 25d));
        page = Math.Clamp(page, 1, pageCount);
        var branches = await database.Stores.AsNoTracking().Where(value => value.AcceptsOrders || database.Orders.Any(order => order.StoreId == value.Id)).OrderBy(value => value.Name).ToArrayAsync();
        return View(new OrderListViewModel
        {
            Orders = await query.OrderByDescending(value => value.CreatedAt).ThenBy(value => value.Id).Skip((page - 1) * 25).Take(25).ToArrayAsync(),
            Branches = branches, StoreId = storeId, Status = status, Page = page, PageCount = pageCount
        });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var order = await database.Orders.AsNoTracking().Include(value => value.Items).SingleOrDefaultAsync(value => value.Id == id);
        return order == null ? NotFound() : View(order);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(Guid id, OrderStatus current, OrderStatus next)
    {
        var valid = current == OrderStatus.New && (next == OrderStatus.Preparing || next == OrderStatus.Cancelled)
            || current == OrderStatus.Preparing && (next == OrderStatus.Completed || next == OrderStatus.Cancelled);
        if (!valid) return BadRequest("Chuyển trạng thái không hợp lệ.");
        var updated = await database.Orders.Where(value => value.Id == id && value.Status == current)
            .ExecuteUpdateAsync(setters => setters.SetProperty(value => value.Status, next));
        TempData["Status"] = updated == 1 ? "Đã cập nhật trạng thái đơn hàng." : "Đơn hàng đã thay đổi hoặc không còn tồn tại.";
        return RedirectToAction(nameof(Details), new { id });
    }
}

public class OrderListViewModel
{
    public Order[] Orders { get; set; } = [];
    public StoreEntry[] Branches { get; set; } = [];
    public Guid? StoreId { get; set; }
    public OrderStatus? Status { get; set; }
    public int Page { get; set; }
    public int PageCount { get; set; }
}
