using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class StoreEntry
{
    public Guid Id { get; set; }

    [Required, StringLength(300)]
    public string Name { get; set; } = "";

    [StringLength(100)]
    public string Phone { get; set; } = "";

    [StringLength(500)]
    public string Hours { get; set; } = "";

    [StringLength(2000)]
    public string Address { get; set; } = "";
    public Guid? CityId { get; set; }
    public StoreCity? City { get; set; }

    [StringLength(500)]
    public string Photo { get; set; } = "";

    [StringLength(2000)]
    public string NavigationUrl { get; set; } = "";
    public bool AcceptsOrders { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    [ConcurrencyCheck]
    public int Revision { get; set; } = 1;
}
