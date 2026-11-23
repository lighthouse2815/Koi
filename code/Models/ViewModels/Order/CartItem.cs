using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class CartItem
{
    public Guid DrinkId { get; set; }
    [Range(1, 20, ErrorMessage = "Mỗi món chỉ được đặt từ 1 đến 20 ly.")]
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
