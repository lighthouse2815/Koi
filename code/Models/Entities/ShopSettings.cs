namespace Koi.Web.Models;

public class ShopSettings
{
    public int Id { get; set; } = 1;
    public Guid? BrandId { get; set; }
    public TeaBrand? Brand { get; set; }
}
