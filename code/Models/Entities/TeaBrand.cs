using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class TeaBrand
{
    [StringLength(150)] public string NameVi { get; set; } = "";

    public Guid Id { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [StringLength(500)] public string Logo { get; set; } = "";
    public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}
