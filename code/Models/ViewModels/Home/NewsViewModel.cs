namespace Koi.Web.Models;

public record NewsViewModel(
    NewsArticle? Featured,
    IReadOnlyList<NewsArticle> Articles,
    string? Category,
    int Page,
    int PageCount
);
