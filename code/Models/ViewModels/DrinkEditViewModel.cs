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
    public string? ImageUploadId { get; set; }
    public bool RemoveImage { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentImage { get; set; }

    [BindNever, ValidateNever]
    public SelectList Categories { get; set; } = null!;
}
