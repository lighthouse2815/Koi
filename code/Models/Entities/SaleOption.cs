using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public enum SaleOptionKind { Size, Topping }
public class SaleOption
{
    public Guid Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    public SaleOptionKind Kind { get; set; }
    public decimal ExtraPrice { get; set; }
    public bool IsActive { get; set; } = true;
}
