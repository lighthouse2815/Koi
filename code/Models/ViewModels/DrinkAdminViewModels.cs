using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Koi.Web.Models;

public class DrinkEditViewModel
{
    [StringLength(250)] public string? NameVi { get; set; }
    [StringLength(150)] public string? RegionVi { get; set; }
    [StringLength(4000)] public string? DescriptionVi { get; set; }

    public int Id { get; set; }
    public int Revision { get; set; }
    [Required, StringLength(250)] public string Name { get; set; } = "";
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    [StringLength(150)] public string? Region { get; set; }
    [StringLength(4000)] public string? Description { get; set; }
    [Range(0, 100000)] public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    public bool RemoveImage { get; set; }
    [BindNever, ValidateNever] public string? CurrentImage { get; set; }
    [BindNever, ValidateNever] public SelectList Categories { get; set; } = null!;
}

public class DrinkCategoryEditViewModel
{
    [StringLength(200)] public string? NameVi { get; set; }

    public int Id { get; set; }
    public int Revision { get; set; }
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Range(0, 100000)] public int SortOrder { get; set; }
    [Range(0, 100000)] public int HomeOrder { get; set; }
    public bool ShowOnHome { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    public IFormFile? IconUpload { get; set; }
    public IFormFile? HomeImageUpload { get; set; }
    public IFormFile? HomeIconUpload { get; set; }
    public bool RemoveIcon { get; set; }
    [BindNever, ValidateNever] public string? CurrentImage { get; set; }
    [BindNever, ValidateNever] public string? CurrentIcon { get; set; }
    [BindNever, ValidateNever] public string? CurrentHomeImage { get; set; }
    [BindNever, ValidateNever] public string? CurrentHomeIcon { get; set; }
}

public class FeaturedDrinkEditViewModel
{
    [StringLength(250)] public string? NameVi { get; set; }
    [StringLength(150)] public string? RegionVi { get; set; }

    public int Id { get; set; }
    public int Revision { get; set; }
    [Required, StringLength(250)] public string Name { get; set; } = "";
    [StringLength(150)] public string? Region { get; set; }
    [Range(0, 100000)] public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    public IFormFile? BackgroundUpload { get; set; }
    [BindNever, ValidateNever] public string? CurrentImage { get; set; }
    [BindNever, ValidateNever] public string? CurrentBackground { get; set; }
}

public record AdminDrinksViewModel(Drink[] Drinks, SelectList Categories, int CategoryId, string? Search, int Page, int PageCount);
