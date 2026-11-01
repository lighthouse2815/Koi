namespace Koi.Web.Models;

public record StoreSocialLink(string Url, string Label);
public record StoreLocation(Guid Id, string Name);
public record StoreBranch(Guid Id, string Name, string Phone, string Hours, string Address, string Logo, string Photo, string NavigationUrl);
public record StoreViewModel(IReadOnlyList<StoreLocation> Locations, IReadOnlyList<StoreBranch> Stores,
    string ShopName, Guid LocationId, int Page, int PageCount);
