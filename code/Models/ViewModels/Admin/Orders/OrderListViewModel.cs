namespace Koi.Web.Models;

public class OrderListViewModel
{
    public List<Order> Orders { get; set; } = [];
    public List<Store> Branches { get; set; } = [];
    public Guid? StoreId { get; set; }
    public OrderStatus? Status { get; set; }
    public int Page { get; set; }
    public int PageCount { get; set; }
}
