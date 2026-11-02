using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Koi.Web.Models;

public class DrinkEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required, StringLength(250)]
    public string Name { get; set; } = "";
    public Guid CategoryId { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    [Range(0, 100000)]
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    public bool RemoveImage { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentImage { get; set; }

    [BindNever, ValidateNever]
    public SelectList Categories { get; set; } = null!;
}

public class CategoryEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Range(0, 100000)]
    public int SortOrder { get; set; }

    public bool ShowOnHome { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    public IFormFile? IconUpload { get; set; }
    public bool RemoveIcon { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentImage { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentIcon { get; set; }

}

public class FeaturedDrinkEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required, StringLength(250)]
    public string Name { get; set; } = "";

    [Range(0, 100000)]
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }
    public IFormFile? BackgroundUpload { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentImage { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentBackground { get; set; }
}

public record AdminDrinksViewModel(
    Drink[] Drinks,
    SelectList Categories,
    Guid CategoryId,
    string? Search,
    int Page,
    int PageCount
);
