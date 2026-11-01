using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class MenuEntry
{
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Label { get; set; } = "";

    [Required, StringLength(500)]
    public string File { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}
