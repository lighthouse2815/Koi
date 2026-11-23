using Microsoft.AspNetCore.Mvc.Rendering;

namespace Koi.Web.Models;

public record AdminDrinksViewModel(
    List<Drink> Drinks,
    SelectList Categories,
    Guid CategoryId,
    string? Search,
    int Page,
    int PageCount
);
