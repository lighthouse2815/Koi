namespace Koi.Web.Models;

public record DashboardViewModel(
    int OrderCount,
    int DrinkCount,
    int StoreCount,
    int NewsCount,
    IReadOnlyList<DashboardOrderDay> OrderActivity,
    IReadOnlyList<DashboardOrder> RecentOrders);

public record DashboardOrderDay(DateOnly Date, int Count);

public record DashboardOrder(Guid Id, string StoreName, DateTime CreatedAt, OrderStatus Status);
