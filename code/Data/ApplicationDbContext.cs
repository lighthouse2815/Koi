using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Koi.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<StoreCity> StoreCities => Set<StoreCity>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<StoreEntry> Stores => Set<StoreEntry>();
    public DbSet<MenuEntry> Menus => Set<MenuEntry>();
    public DbSet<Category> DrinkCategories => Set<Category>();
    public DbSet<Drink> Drinks => Set<Drink>();
    public DbSet<FeaturedDrink> FeaturedDrinks => Set<FeaturedDrink>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<SaleOption> SaleOptions => Set<SaleOption>();
    public DbSet<ShopSettings> ShopSettings => Set<ShopSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<StoreEntry>().HasOne(value => value.City).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.Entity<StoreEntry>().HasIndex(value => new { value.IsPublished, value.CityId });
        builder.Entity<MenuEntry>().Property<string>("LabelVi").HasMaxLength(200).IsRequired();
        builder.Entity<MenuEntry>().HasIndex(value => new { value.IsPublished, value.SortOrder });
        builder.Entity<Category>().ToTable("Category");
        builder.Entity<Category>().Property<int>("HomeOrder");
        builder.Entity<Category>().Property<string>("HomeIcon").HasMaxLength(500).IsRequired();
        builder.Entity<Category>().Property<string>("HomeImage").HasMaxLength(500).IsRequired();
        builder.Entity<Category>().Property<string>("NameVi").HasMaxLength(200).IsRequired();
        builder.Entity<Drink>().HasOne(value => value.Category).WithMany(value => value.Drinks).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Drink>().HasIndex(value => new { value.CategoryId, value.IsPublished, value.SortOrder });
        // Preserve legacy database columns without exposing them on the entity.
        builder.Entity<Drink>().Property<string>("NameVi").HasMaxLength(250).IsRequired();
        builder.Entity<Drink>().Property<string>("Region").HasMaxLength(150).IsRequired();
        builder.Entity<Drink>().Property<string>("RegionVi").HasMaxLength(150).IsRequired();
        builder.Entity<Drink>().Property<string>("DescriptionVi").HasMaxLength(4000).IsRequired();
        builder.Entity<FeaturedDrink>().Property<string>("NameVi").HasMaxLength(250).IsRequired();
        builder.Entity<FeaturedDrink>().Property<string>("Region").HasMaxLength(150).IsRequired();
        builder.Entity<FeaturedDrink>().Property<string>("RegionVi").HasMaxLength(150).IsRequired();
        builder.Entity<Drink>().Property(value => value.Price).HasPrecision(12, 0);
        builder.Entity<OrderItem>().Property(value => value.UnitPrice).HasPrecision(12, 0);
        builder.Entity<SaleOption>().Property(value => value.ExtraPrice).HasPrecision(12, 0);
        builder.Entity<ShopSettings>().HasData(new ShopSettings { Id = 1 });
        builder.Entity<Order>().HasOne(value => value.Store).WithMany().OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Order>().HasIndex(value => new { value.Status, value.CreatedAt });
        var articles = builder.Entity<NewsArticle>();
        articles.Property<string>("TitleVi").HasMaxLength(300).IsRequired();
        articles.Property<string>("BodyHtmlVi").HasMaxLength(200000).IsRequired();
        articles.HasIndex(article => new { article.IsPublished, article.Date });
        articles.HasIndex(article => article.IsFeatured).IsUnique().HasFilter("\"IsFeatured\" = TRUE");
        using var stream = typeof(ApplicationDbContext).Assembly.GetManifestResourceStream("Koi.Web.Data.NewsSeed.json")
            ?? throw new InvalidOperationException("News seed data is missing.");
        var seed = JsonNode.Parse(stream)!.AsArray();
        var prefix = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes("NewsArticles")))[..24];
        foreach (var item in seed)
        {
            item!["id"] = Guid.ParseExact($"{prefix}{item["id"]!.GetValue<int>():x8}", "N").ToString();
        }
        var seedArticles = JsonSerializer.Deserialize<NewsArticle[]>(seed.ToJsonString(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        articles.HasData(seedArticles.Select(article => new
        {
            article.Id, article.Title, article.Category, article.Date, article.Image, article.BannerImage,
            article.HomeImage, article.VideoId, article.BodyHtml, article.IsPublished, article.IsFeatured,
            article.ShowOnHome, article.HomeOrder, article.Revision, TitleVi = "", BodyHtmlVi = ""
        }));
    }
}
