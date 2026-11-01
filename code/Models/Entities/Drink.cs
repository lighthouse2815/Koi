using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class Drink
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public DrinkCategory Category { get; set; } = null!;

    [Required, StringLength(250)]
    public string Name { get; set; } = "";

    [StringLength(4000)]
    public string Description { get; set; } = "";

    [StringLength(500)]
    public string Image { get; set; } = "";

    public decimal? Price { get; set; }

    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}
