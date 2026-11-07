using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Koi.Web.Services;

public class ImageUploadService(IConfiguration configuration)
{
    public async Task<string?> GetImageUrlAsync(
        string? publicId, ModelStateDictionary errors, string field, string folder)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return null;

        var prefix = $"koi/{folder}/";
        if (!publicId.StartsWith(prefix, StringComparison.Ordinal)
            || !Guid.TryParseExact(publicId[prefix.Length..], "N", out _))
        {
            errors.AddModelError(field, "Ảnh tải lên không hợp lệ.");
            return null;
        }

        var resource = await CreateCloudinary().GetResourceAsync(publicId);
        if (resource.Error != null || resource.SecureUrl == null
            || resource.Bytes <= 0 || resource.Bytes > 5 * 1024 * 1024
            || resource.Format is not ("jpg" or "jpeg" or "png" or "webp"))
        {
            errors.AddModelError(field, "Chọn ảnh JPG, PNG hoặc WebP tối đa 5 MB.");
            return null;
        }
        return resource.SecureUrl.ToString();
    }

    public object CreateUploadSignature(string folder)
    {
        var allowedFolders = new[] { "categories", "drinks", "featured-drinks", "menus", "news", "stores" };
        if (!allowedFolders.Contains(folder, StringComparer.Ordinal))
            throw new ArgumentException("Thư mục tải ảnh không hợp lệ.", nameof(folder));

        var cloudinary = CreateCloudinary();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var publicId = $"koi/{folder}/{Guid.NewGuid():N}";
        var parameters = new SortedDictionary<string, object>(StringComparer.Ordinal)
        {
            ["timestamp"] = timestamp,
            ["public_id"] = publicId,
            ["allowed_formats"] = "jpg,jpeg,png,webp",
            ["overwrite"] = "false"
        };
        return new
        {
            cloudName = configuration["Cloudinary:CloudName"],
            apiKey = configuration["Cloudinary:ApiKey"],
            timestamp,
            publicId,
            allowedFormats = parameters["allowed_formats"],
            overwrite = parameters["overwrite"],
            signature = cloudinary.Api.SignParameters(parameters)
        };
    }

    private Cloudinary CreateCloudinary()
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            throw new InvalidOperationException("Cần cấu hình Cloudinary trước khi tải ảnh lên.");
        return new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }
}
