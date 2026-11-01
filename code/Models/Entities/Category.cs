using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class Category
{
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string Icon { get; set; } = "";

    [Required, StringLength(500)]
    public string Image { get; set; } = "";

    [StringLength(500)]
    public string HomeIcon { get; set; } = "";

    public bool ShowOnHome { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }
    public int HomeOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;

    public List<Drink> Drinks { get; set; } = [];
}
