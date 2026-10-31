using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public enum OrderStatus { New, Preparing, Completed, Cancelled }
public enum FulfillmentMethod { Pickup, Delivery }

public class Order
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; }
    public StoreEntry? Store { get; set; }
    [Required, StringLength(300)] public string StoreName { get; set; } = "";
    [Required, StringLength(2000)] public string StoreAddress { get; set; } = "";
    [Required, StringLength(150)] public string CustomerName { get; set; } = "";
    [Required, StringLength(30)] public string CustomerPhone { get; set; } = "";
    [StringLength(2000)] public string DeliveryAddress { get; set; } = "";
    [StringLength(1000)] public string Note { get; set; } = "";
    public FulfillmentMethod Fulfillment { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    [Required, StringLength(250)] public string DrinkName { get; set; } = "";
    [StringLength(2000)] public string Customization { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
