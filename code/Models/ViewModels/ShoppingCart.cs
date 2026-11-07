namespace Koi.Web.Models;

public class ShoppingCart
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public List<CartItem> Items { get; set; } = [];
}
