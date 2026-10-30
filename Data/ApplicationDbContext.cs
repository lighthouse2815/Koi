using System.Text.Json;
using Koi.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var articles = builder.Entity<NewsArticle>();
        articles.Property(article => article.Id).HasIdentityOptions(startValue: 10000);
        articles.HasIndex(article => new { article.IsPublished, article.Date });
        articles.HasIndex(article => article.IsFeatured).IsUnique().HasFilter("\"IsFeatured\" = TRUE");
        using var stream = typeof(ApplicationDbContext).Assembly.GetManifestResourceStream("Koi.Web.Data.NewsSeed.json")
            ?? throw new InvalidOperationException("News seed data is missing.");
        articles.HasData(JsonSerializer.Deserialize<NewsArticle[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))!);
    }
}
