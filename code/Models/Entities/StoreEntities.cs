using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class Market
{
    [StringLength(100)] public string NameVi { get; set; } = "";

    public Guid Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
    public List<MarketBrand> Brands { get; set; } = [];
    public List<MarketSocialLink> SocialLinks { get; set; } = [];
}

public class MarketBrand
{
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;
    public Guid BrandId { get; set; }
    public TeaBrand Brand { get; set; } = null!;
}

public class MarketSocialLink
{
    public Guid Id { get; set; }
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;
    [StringLength(100)] public string Label { get; set; } = "";
    [StringLength(1000)] public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}

public class StoreCity
{
    [StringLength(150)] public string NameVi { get; set; } = "";

    public Guid Id { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;
    public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}

public class TeaBrand
{
    [StringLength(150)] public string NameVi { get; set; } = "";

    public Guid Id { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [StringLength(500)] public string Logo { get; set; } = "";
    public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}

public class StoreEntry
{
    [StringLength(300)] public string NameVi { get; set; } = "";
    [StringLength(2000)] public string AddressVi { get; set; } = "";
    [StringLength(500)] public string HoursVi { get; set; } = "";

    public Guid Id { get; set; }
    [Required, StringLength(300)] public string Name { get; set; } = "";
    [StringLength(100)] public string Phone { get; set; } = "";
    [StringLength(500)] public string Hours { get; set; } = "";
    [StringLength(2000)] public string Address { get; set; } = "";
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;
    public Guid? CityId { get; set; }
    public StoreCity? City { get; set; }
    public Guid BrandId { get; set; }
    public TeaBrand Brand { get; set; } = null!;
    [StringLength(500)] public string Photo { get; set; } = "";
    [StringLength(2000)] public string NavigationUrl { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
    public int CountryOrder { get; set; }
    public int CityOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}
