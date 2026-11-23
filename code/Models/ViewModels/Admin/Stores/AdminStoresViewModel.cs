namespace Koi.Web.Models;

public record AdminStoresViewModel(
    List<Store> Stores,
    List<StoreCity> Cities,
    string? Search,
    int Page,
    int PageCount
);
