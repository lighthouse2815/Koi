namespace Koi.Web.Models;

public record AdminNewsViewModel(IReadOnlyList<NewsArticle> Articles, string? Search, int Page, int PageCount);
