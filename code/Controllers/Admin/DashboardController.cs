using Koi.Web.Components.Admin;
using Koi.Web.Components.Shared;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DashboardController(ApplicationDbContext database) : Controller
{
    private const string VietnamTimeZoneId = "Asia/Ho_Chi_Minh";

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(VietnamTimeZoneId);
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
        var firstDay = today.AddDays(-6);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(firstDay, timeZone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(today.AddDays(1), timeZone);

        var dailyCounts = await database.Orders
            .Where(order => order.CreatedAt >= startUtc && order.CreatedAt < endUtc)
            .GroupBy(order => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(order.CreatedAt, VietnamTimeZoneId).Date)
            .Select(day => new { Date = day.Key, Count = day.Count() })
            .ToDictionaryAsync(day => day.Date, day => day.Count, cancellationToken);

        var chart = Enumerable.Range(0, 7)
            .Select(offset => firstDay.AddDays(offset))
            .Select(date => new DashboardOrderDay(DateOnly.FromDateTime(date), dailyCounts.GetValueOrDefault(date)))
            .ToArray();

        var recentOrders = await database.Orders.AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ThenBy(order => order.Id)
            .Take(5)
            .Select(order => new DashboardOrder(order.Id, order.StoreName, order.CreatedAt, order.Status))
            .ToListAsync(cancellationToken);

        var model = new DashboardViewModel(
            await database.Orders.CountAsync(cancellationToken),
            await database.Drinks.CountAsync(cancellationToken),
            await database.Stores.CountAsync(cancellationToken),
            await database.NewsArticles.CountAsync(cancellationToken),
            chart,
            recentOrders.Select(order => order with { CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedAt, timeZone) }).ToList());

        return new BlazorViewResult<AdminDashboardPage>(new { Model = model });
    }
}
