using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Koi.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter<NewsCategory>))]
public enum NewsCategory { NEWS, MEDIA }

public class NewsArticle
{
    public Guid Id { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = "";

    public NewsCategory Category { get; set; } = NewsCategory.NEWS;
    public DateOnly Date { get; set; }

    [MaxLength(500)]
    public string Image { get; set; } = "";

    [MaxLength(500)]
    public string BannerImage { get; set; } = "";

    [MaxLength(500)]
    public string HomeImage { get; set; } = "";

    [MaxLength(11)]
    public string? VideoId { get; set; }
    public string BodyHtml { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool ShowOnHome { get; set; }
    public int HomeOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}
