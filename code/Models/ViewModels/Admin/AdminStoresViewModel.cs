namespace Koi.Web.Models;

public record AdminStoresViewModel(
    Store[] Stores,
    StoreCity[] Cities,
    string? Search,
    int Page,
    int PageCount
);
