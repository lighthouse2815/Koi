using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

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
