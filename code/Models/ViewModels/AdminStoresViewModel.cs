namespace Koi.Web.Models;

public record AdminStoresViewModel(
    Store[] Stores,
    string? Search,
    int Page,
    int PageCount
);
