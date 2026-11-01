using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class FeaturedDrink
{
    public Guid Id { get; set; }

    [Required, StringLength(250)]
    public string Name { get; set; } = "";

    [Required, StringLength(500)]
    public string Image { get; set; } = "";

    [Required, StringLength(500)]
    public string BackgroundImage { get; set; } = "";

    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}
