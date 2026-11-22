namespace Koi.Web.Models;

public class CartItem
{
    public Guid LineId { get; set; } = Guid.NewGuid();
    public Guid DrinkId { get; set; }
    // Keep old cart selections detectable until customers replace them.
    public Guid? SizeId { get; set; }
    public List<Guid> ToppingIds { get; set; } = [];
    public string Customization { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
