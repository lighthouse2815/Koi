namespace Koi.Web.Models;

public record StoreViewModel(IReadOnlyList<StoreLocation> Locations, IReadOnlyList<StoreBranch> Stores,
    string ShopName, Guid LocationId, int Page, int PageCount);
