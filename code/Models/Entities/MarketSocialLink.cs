using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class MarketSocialLink
{
    public Guid Id { get; set; }
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;
    [StringLength(100)] public string Label { get; set; } = "";
    [StringLength(1000)] public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}
