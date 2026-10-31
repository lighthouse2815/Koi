using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class Market
{
    [StringLength(100)]
    public string NameVi { get; set; } = "";

    public Guid Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
    public List<MarketBrand> Brands { get; set; } = [];
    public List<MarketSocialLink> SocialLinks { get; set; } = [];
}
