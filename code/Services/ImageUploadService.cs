using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Koi.Web.Services;

public record ImageUpload(byte[] Bytes, string Extension);

public class ImageUploadService(IWebHostEnvironment environment, IConfiguration configuration)
{
    public async Task<ImageUpload?> ReadAsync(
        IFormFile? file,
        ModelStateDictionary errors,
        string field
    )
    {
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
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            throw new InvalidOperationException("Cần cấu hình Cloudinary trước khi tải ảnh lên.");

        var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
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

    // Only roll back a newly created upload after a failed save. Existing media may be shared.
    public void Rollback(string? url)
    {
        if (url != null && url.StartsWith("/uploads/", StringComparison.Ordinal))
            File.Delete(Path.Combine(environment.WebRootPath, url.TrimStart('/')));
    }
}
