using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class ShoppingCart
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public List<CartItem> Items { get; set; } = [];
}
public class CartItem
{
    public Guid DrinkId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
public record CartLine(Guid DrinkId, string Name, int Quantity, decimal Price);
public class OrderingPageViewModel
{
    public StoreEntry[] Branches { get; set; } = [];
    public Drink[] Drinks { get; set; } = [];
    public ShoppingCart Cart { get; set; } = new();
    public StoreEntry? Branch { get; set; }
    public List<CartLine> Lines { get; set; } = [];
    public CheckoutViewModel Checkout { get; set; } = new();
    public decimal Total => Lines.Sum(value => value.Price * value.Quantity);
}
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Nhập tên người nhận."), StringLength(150)]
    public string CustomerName { get; set; } = "";
    [Required(ErrorMessage = "Nhập số điện thoại."), StringLength(30), Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string CustomerPhone { get; set; } = "";
    public FulfillmentMethod Fulfillment { get; set; }
    [StringLength(2000)] public string? DeliveryAddress { get; set; }
    [StringLength(1000)] public string? Note { get; set; }
}
