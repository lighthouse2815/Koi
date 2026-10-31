namespace Koi.Web.Models;

public record StoreSocialLink(string Url, string Label);

public record StoreCountry(Guid Id, string Name);

public record StoreLocation(Guid Id, string Name);

public record StoreBrand(Guid Id, string Name);

public record StoreBranch(
    Guid Id,
    string Name,
    string Phone,
    string Hours,
    string Address,
    Guid BrandId,
    string Logo,
    string Photo,
    string NavigationUrl
);

public record StoreViewModel(
    IReadOnlyList<StoreCountry> Countries,
    IReadOnlyList<StoreLocation> Locations,
    IReadOnlyList<StoreBrand> Brands,
    IReadOnlyList<StoreBranch> Stores,
    IReadOnlyList<StoreSocialLink> SocialLinks,
    Guid CountryId,
    Guid LocationId,
    Guid BrandId,
    int Page,
    int PageCount
);
