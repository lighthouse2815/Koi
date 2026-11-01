namespace Koi.Web.Models;

public record HomeViewModel(IReadOnlyList<NewsArticle> News, IReadOnlyList<Category> DrinkCategories);
