using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Koi.Web.Models;

public class MenuEntry
{
    public int Id { get; set; }
    public int MarketId { get; set; }
    public Market Market { get; set; } = null!;
    [Required, StringLength(200)] public string Label { get; set; } = "";
    [Required, StringLength(500)] public string File { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    [Range(0, 100000)] public int SortOrder { get; set; }
    [ConcurrencyCheck] public int Revision { get; set; } = 1;
}

public class MenuEditViewModel
{
    public int Id { get; set; }
    public int Revision { get; set; }
    [Required, StringLength(200)] public string Label { get; set; } = "";
    [Range(1, int.MaxValue)] public int MarketId { get; set; }
    [Range(0, 100000)] public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    [BindNever, ValidateNever] public string? CurrentImage { get; set; }
    [BindNever, ValidateNever] public SelectList Markets { get; set; } = null!;
}

public record AdminMenusViewModel(MenuEntry[] Menus, SelectList Markets, int MarketId);
