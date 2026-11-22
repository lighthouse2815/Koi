namespace Koi.Web.Models;

public record StoreViewModel(IReadOnlyList<StoreCity> Locations, IReadOnlyList<Store> Stores,
    string ShopName, string ShopLogo, Guid LocationId, int Page, int PageCount);
