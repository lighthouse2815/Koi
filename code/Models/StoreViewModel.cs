namespace Koi.Web.Models;

public record StoreSocialLink(string Url, string Label);
public record StoreCountry(int Id, string Name, int[] BrandIds, StoreSocialLink[] SocialLinks, int[] StoreIds);
public record StoreLocation(int Id, int CountryId, string Name, int[] StoreIds);
public record StoreBrand(int Id, string Name);
public record StoreBranch(int Id, string Name, string Phone, string Hours, string Address,
    int CountryId, int LocationId, int BrandId, string Logo, string Photo, string NavigationUrl);
public record StoreSnapshot(string CapturedOn, StoreCountry[] Countries, StoreLocation[] Locations,
    StoreBrand[] Brands, StoreBranch[] Stores);
public record StoreViewModel(IReadOnlyList<StoreCountry> Countries, IReadOnlyList<StoreLocation> Locations,
    IReadOnlyList<StoreBrand> Brands, IReadOnlyList<StoreBranch> Stores,
    IReadOnlyList<StoreSocialLink> SocialLinks, int CountryId, int LocationId, int BrandId, int Page, int PageCount);
