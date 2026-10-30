using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Koi.Web.Models;

public class StoreEditViewModel
{
    [StringLength(300)] public string? NameVi { get; set; }
    [StringLength(2000)] public string? AddressVi { get; set; }
    [StringLength(500)] public string? HoursVi { get; set; }

    public int Id { get; set; }
    public int Revision { get; set; }
    [Required, StringLength(300)] public string Name { get; set; } = "";
    [StringLength(100)] public string? Phone { get; set; }
    [StringLength(500)] public string? Hours { get; set; }
    [StringLength(2000)] public string? Address { get; set; }
    [Range(1, int.MaxValue)] public int MarketId { get; set; }
    public int? CityId { get; set; }
    [Range(1, int.MaxValue)] public int BrandId { get; set; }
    [StringLength(2000)] public string? NavigationUrl { get; set; }
    public bool IsPublished { get; set; } = true;
    [Range(0, 1000000)] public int SortOrder { get; set; } = 10000;
    [Range(0, 1000000)] public int CountryOrder { get; set; } = 10000;
    [Range(0, 1000000)] public int CityOrder { get; set; } = 10000;
    public IFormFile? ImageUpload { get; set; }
    public bool RemoveImage { get; set; }
    [BindNever, ValidateNever] public string? CurrentImage { get; set; }
    [BindNever, ValidateNever] public SelectList Markets { get; set; } = null!;
    [BindNever, ValidateNever] public StoreCity[] Cities { get; set; } = [];
    [BindNever, ValidateNever] public SelectList Brands { get; set; } = null!;
}

public record AdminStoresViewModel(StoreEntry[] Stores, SelectList Markets, string? Search, int MarketId, int Page, int PageCount);

public class GeographyEditViewModel
{
    [StringLength(150)] public string? NameVi { get; set; }

    public int Id { get; set; }
    public int Revision { get; set; }
    [Required, RegularExpression("countries|cities|brands")] public string Kind { get; set; } = "countries";
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Range(0, 100000)] public int SortOrder { get; set; }
    public int MarketId { get; set; }
    public int[] BrandIds { get; set; } = [];
    [StringLength(10000)] public string? SocialLinks { get; set; }
    public IFormFile? ImageUpload { get; set; }
    public bool RemoveImage { get; set; }
    [BindNever, ValidateNever] public string? CurrentImage { get; set; }
    [BindNever, ValidateNever] public SelectList Markets { get; set; } = null!;
    [BindNever, ValidateNever] public TeaBrand[] Brands { get; set; } = [];
}

public record GeographyRow(int Id, string Name, string Detail, int SortOrder);
public record GeographyIndexViewModel(string Kind, string Title, GeographyRow[] Rows);
public record DeleteContentViewModel(int Id, string Name, string? Kind = null);
