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

    [Range(0, 1000000)]
    public int CountryOrder { get; set; } = 10000;

    [Range(0, 1000000)]
    public int CityOrder { get; set; } = 10000;
    public IFormFile? ImageUpload { get; set; }
    public bool RemoveImage { get; set; }

    [BindNever, ValidateNever]
    public string? CurrentImage { get; set; }

    [BindNever, ValidateNever]
    public StoreCity[] Cities { get; set; } = [];

}

public record AdminStoresViewModel(
    StoreEntry[] Stores,
    string? Search,
    int Page,
    int PageCount
);

public class GeographyEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required, RegularExpression("cities")]
    public string Kind { get; set; } = "cities";

    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    [Range(0, 100000)]
    public int SortOrder { get; set; }

}

public record GeographyRow(Guid Id, string Name, string Detail, int SortOrder);

public record GeographyIndexViewModel(string Kind, string Title, GeographyRow[] Rows);

public record DeleteContentViewModel(Guid Id, string Name, string? Kind = null);
