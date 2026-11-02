using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koi.Web.Models;

public class MenuEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required, StringLength(200)]
    public string Label { get; set; } = "";

    [Range(0, 100000)]
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public IFormFile? ImageUpload { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentImage { get; set; }

}

public record AdminMenusViewModel(Menu[] Menus);
