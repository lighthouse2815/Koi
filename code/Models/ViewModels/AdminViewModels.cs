using System.ComponentModel.DataAnnotations;

namespace Koi.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu."), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class NewsEditViewModel
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    [Required(ErrorMessage = "Nhập tiêu đề."), StringLength(300)]
    public string Title { get; set; } = "";

    [EnumDataType(typeof(NewsCategory), ErrorMessage = "Chọn NEWS hoặc MEDIA.")]
    public NewsCategory Category { get; set; } = NewsCategory.NEWS;

    [Required(ErrorMessage = "Chọn ngày đăng."), DataType(DataType.Date)]
    public DateOnly? Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Nhập nội dung."), StringLength(200000)]
    public string BodyHtml { get; set; } = "";

    [StringLength(500)]
    public string? VideoUrl { get; set; }
    public IFormFile? ImageUpload { get; set; }
    public string? CurrentImage { get; set; }
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool ShowOnHome { get; set; }

    [Range(0, 100)]
    public int HomeOrder { get; set; }
}

public record AdminNewsViewModel(IReadOnlyList<NewsArticle> Articles, string? Search, int Page, int PageCount);
