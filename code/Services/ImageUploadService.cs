using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Koi.Web.Services;

public record ImageUpload(byte[] Bytes, string Extension, string? Url = null);

public class ImageUploadService(IWebHostEnvironment environment, IConfiguration configuration)
{
    public async Task<ImageUpload?> ReadAsync(
        IFormFile? file,
        ModelStateDictionary errors,
        string field,
        string? publicId = null,
        string? folder = null
    )
    {
        if (!string.IsNullOrWhiteSpace(publicId))
        {
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
            return new ImageUpload([], "." + resource.Format, resource.SecureUrl.ToString());
        }

        if (file == null || file.Length == 0)
            return null;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (
            file.Length > 5 * 1024 * 1024
            || extension is not (".jpg" or ".jpeg" or ".png" or ".webp")
        )
        {
            errors.AddModelError(field, "Chọn ảnh JPG, PNG hoặc WebP tối đa 5 MB.");
            return null;
        }
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        var valid = extension switch
        {
            ".png" => bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 }),
            ".webp" => bytes.Length >= 12
                && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false,
        };
        if (!valid)
        {
            errors.AddModelError(field, "Nội dung tệp không đúng định dạng ảnh.");
            return null;
        }
        return new ImageUpload(bytes, extension);
    }

    public async Task<string> SaveAsync(ImageUpload image, string folder)
    {
        if (image.Url != null)
            return image.Url;
        var cloudinary = CreateCloudinary();
        using var stream = new MemoryStream(image.Bytes);
        var result = await cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription("image" + image.Extension, stream),
            Folder = $"koi/{folder}"
        });
        if (result.Error != null || result.SecureUrl == null)
            throw new InvalidOperationException("Không thể tải ảnh lên Cloudinary.");
        return result.SecureUrl.AbsoluteUri;
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
            allowedFormats = "jpg,jpeg,png,webp",
            overwrite = "false",
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

    // Only roll back a newly created upload after a failed save. Existing media may be shared.
    public void Rollback(string? url)
    {
        if (url != null && url.StartsWith("/uploads/", StringComparison.Ordinal))
            File.Delete(Path.Combine(environment.WebRootPath, url.TrimStart('/')));
    }
}
