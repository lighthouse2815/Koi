using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class MenuEntry
{
    [StringLength(200)]
    public string LabelVi { get; set; } = "";

    public Guid Id { get; set; }
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;

    [Required, StringLength(200)]
    public string Label { get; set; } = "";

    [Required, StringLength(500)]
    public string File { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}
