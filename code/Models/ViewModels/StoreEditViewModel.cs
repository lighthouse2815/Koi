using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koi.Web.Models;

public class StoreEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required, StringLength(300)]
    public string Name { get; set; } = "";

    [StringLength(100)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Hours { get; set; }

    [StringLength(2000)]
    public string? Address { get; set; }
    public Guid? CityId { get; set; }

    [StringLength(2000)]
    public string? NavigationUrl { get; set; }
    public bool IsPublished { get; set; } = true;

    [Range(0, 1000000)]
    public int SortOrder { get; set; } = 10000;

    public string? ImageUploadId { get; set; }
    public bool RemoveImage { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentImage { get; set; }

    [BindNever, ValidateNever]
    public StoreCity[] Cities { get; set; } = [];

}
