namespace Koi.Web.Models;

public record StoreSocialLink(string Url, string Label);
public record StoreCountry(int Id, string Name);
public record StoreLocation(int Id, string Name);
public record StoreBrand(int Id, string Name);
public record StoreBranch(int Id, string Name, string Phone, string Hours, string Address,
    int BrandId, string Logo, string Photo, string NavigationUrl);
public record StoreViewModel(IReadOnlyList<StoreCountry> Countries, IReadOnlyList<StoreLocation> Locations,
    IReadOnlyList<StoreBrand> Brands, IReadOnlyList<StoreBranch> Stores,
    IReadOnlyList<StoreSocialLink> SocialLinks, int CountryId, int LocationId, int BrandId, int Page, int PageCount);
