namespace Koi.Web.Models;

public record NewsArticle(int Id, string Title, string Category, DateOnly Date, string Image);

public record NewsViewModel(NewsArticle Featured, IReadOnlyList<NewsArticle> Articles, string? Category, int Page, int PageCount);
