using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class SocialLink
{
    public Guid Id { get; set; }
    [StringLength(100)] public string Label { get; set; } = "";
    [StringLength(1000)] public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}
