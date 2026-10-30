using System.Text.Json;
using Koi.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<StoreCity> StoreCities => Set<StoreCity>();
    public DbSet<TeaBrand> TeaBrands => Set<TeaBrand>();
    public DbSet<StoreEntry> Stores => Set<StoreEntry>();
    public DbSet<MarketBrand> MarketBrands => Set<MarketBrand>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<MarketBrand>().HasKey(value => new { value.MarketId, value.BrandId });
        builder.Entity<StoreEntry>().HasOne(value => value.Market).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.Entity<StoreEntry>().HasOne(value => value.City).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.Entity<StoreEntry>().HasOne(value => value.Brand).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.Entity<StoreCity>().HasOne(value => value.Market).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.Entity<StoreEntry>().HasIndex(value => new { value.IsPublished, value.MarketId, value.CityId, value.BrandId });
        builder.Entity<Market>().Property(value => value.Id).HasIdentityOptions(startValue: 10000);
        builder.Entity<StoreCity>().Property(value => value.Id).HasIdentityOptions(startValue: 10000);
        builder.Entity<TeaBrand>().Property(value => value.Id).HasIdentityOptions(startValue: 10000);
        builder.Entity<StoreEntry>().Property(value => value.Id).HasIdentityOptions(startValue: 10000);
        var articles = builder.Entity<NewsArticle>();
        articles.Property(article => article.Id).HasIdentityOptions(startValue: 10000);
        articles.HasIndex(article => new { article.IsPublished, article.Date });
        articles.HasIndex(article => article.IsFeatured).IsUnique().HasFilter("\"IsFeatured\" = TRUE");
        using var stream = typeof(ApplicationDbContext).Assembly.GetManifestResourceStream("Koi.Web.Data.NewsSeed.json")
            ?? throw new InvalidOperationException("News seed data is missing.");
        articles.HasData(JsonSerializer.Deserialize<NewsArticle[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))!);
    }
}
