using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Koi.Web.Services;

public record ImageUpload(byte[] Bytes, string Extension);

public class ImageUploads(IWebHostEnvironment environment)
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
        var directory = Path.Combine(environment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(directory);
        var name = Guid.NewGuid().ToString("N") + image.Extension;
        var path = Path.Combine(directory, name);
        try
        {
            await File.WriteAllBytesAsync(path, image.Bytes);
        }
        catch
        {
            if (File.Exists(path))
                File.Delete(path);
            throw;
        }
        return $"/uploads/{folder}/{name}";
    }

    // Only roll back a newly created upload after a failed save. Existing media may be shared.
    public void Rollback(string? url)
    {
        if (url != null && url.StartsWith("/uploads/", StringComparison.Ordinal))
            File.Delete(Path.Combine(environment.WebRootPath, url.TrimStart('/')));
    }
}
