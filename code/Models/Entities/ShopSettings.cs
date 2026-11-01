using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class ShopSettings
{
    public int Id { get; set; } = 1;
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [StringLength(150)] public string NameVi { get; set; } = "";
    [StringLength(500)] public string Logo { get; set; } = "";
}
