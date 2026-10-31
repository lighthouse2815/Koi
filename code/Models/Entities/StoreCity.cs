using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class StoreCity
{
    [StringLength(150)] public string NameVi { get; set; } = "";

    public Guid Id { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    public Guid MarketId { get; set; }
    public Market Market { get; set; } = null!;
    public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}
