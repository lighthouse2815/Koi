namespace Koi.Web.Models;

public class MarketBrand
{
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;
    public Guid BrandId { get; set; }
    public TeaBrand Brand { get; set; } = null!;
}
