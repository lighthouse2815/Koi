using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Koi.Web.Models;

public class MenuEditViewModel
{
    [StringLength(200)] public string? LabelVi { get; set; }

    public Guid Id { get; set; }
    public int Revision { get; set; }
    [Required, StringLength(200)] public string Label { get; set; } = "";
    public Guid MarketId { get; set; }
    [Range(0, 100000)] public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    [BindNever, ValidateNever] public string? CurrentImage { get; set; }
    [BindNever, ValidateNever] public SelectList Markets { get; set; } = null!;
}

public record AdminMenusViewModel(MenuEntry[] Menus, SelectList Markets, Guid MarketId);
