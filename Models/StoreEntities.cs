using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class Market
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Range(0, 100000)] public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
    public List<MarketBrand> Brands { get; set; } = [];
    public List<MarketSocialLink> SocialLinks { get; set; } = [];
}

public class MarketBrand
{
    public int MarketId { get; set; }
    public Market Market { get; set; } = null!;
    public int BrandId { get; set; }
    public TeaBrand Brand { get; set; } = null!;
}

public class MarketSocialLink
{
    public int Id { get; set; }
    public int MarketId { get; set; }
    public Market Market { get; set; } = null!;
    [StringLength(100)] public string Label { get; set; } = "";
    [StringLength(1000)] public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}

public class StoreCity
{
    public int Id { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    public int MarketId { get; set; }
    public Market Market { get; set; } = null!;
    [Range(0, 100000)] public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}

public class TeaBrand
{
    public int Id { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [StringLength(500)] public string Logo { get; set; } = "";
    [Range(0, 100000)] public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}

public class StoreEntry
{
    public int Id { get; set; }
    [Required, StringLength(300)] public string Name { get; set; } = "";
    [StringLength(100)] public string Phone { get; set; } = "";
    [StringLength(500)] public string Hours { get; set; } = "";
    [StringLength(2000)] public string Address { get; set; } = "";
    public int MarketId { get; set; }
    public Market Market { get; set; } = null!;
    public int? CityId { get; set; }
    public StoreCity? City { get; set; }
    public int BrandId { get; set; }
    public TeaBrand Brand { get; set; } = null!;
    [StringLength(500)] public string Photo { get; set; } = "";
    [StringLength(2000)] public string NavigationUrl { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    [Range(0, 1000000)] public int SortOrder { get; set; }
    [Range(0, 1000000)] public int CountryOrder { get; set; }
    [Range(0, 1000000)] public int CityOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}
