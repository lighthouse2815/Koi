using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class DrinkCategory
{
    [StringLength(200)]
    public string NameVi { get; set; } = "";

    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string Icon { get; set; } = "";

    [Required, StringLength(500)]
    public string Image { get; set; } = "";

    [StringLength(500)]
    public string HomeImage { get; set; } = "";

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

public class Drink
{
    [StringLength(250)]
    public string NameVi { get; set; } = "";

    [StringLength(150)]
    public string RegionVi { get; set; } = "";

    [StringLength(4000)]
    public string DescriptionVi { get; set; } = "";

    public int Id { get; set; }
    public int CategoryId { get; set; }
    public DrinkCategory Category { get; set; } = null!;

    [Required, StringLength(250)]
    public string Name { get; set; } = "";

    [StringLength(150)]
    public string Region { get; set; } = "";

    [StringLength(4000)]
    public string Description { get; set; } = "";

    [StringLength(500)]
    public string Image { get; set; } = "";

    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}

public class FeaturedDrink
{
    [StringLength(250)]
    public string NameVi { get; set; } = "";

    [StringLength(150)]
    public string RegionVi { get; set; } = "";

    public int Id { get; set; }

    [Required, StringLength(250)]
    public string Name { get; set; } = "";

    [StringLength(150)]
    public string Region { get; set; } = "";

    [Required, StringLength(500)]
    public string Image { get; set; } = "";

    [Required, StringLength(500)]
    public string BackgroundImage { get; set; } = "";

    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}
