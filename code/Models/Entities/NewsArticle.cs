using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class NewsArticle
{
    [StringLength(300)]
    public string TitleVi { get; set; } = "";

    [StringLength(200000)]
    public string BodyHtmlVi { get; set; } = "";

    public Guid Id { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = "";

    [MaxLength(10)]
    public string Category { get; set; } = "NEWS";
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
