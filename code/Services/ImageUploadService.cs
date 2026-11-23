using CloudinaryDotNet;

namespace Koi.Web.Services;

public class ImageUploadService(IConfiguration configuration)
{
    public object CreateUploadSignature(string folder)
    {
        var allowedFolders = new[] { "categories", "drinks", "featured-drinks", "menus", "news", "stores" };
        if (!allowedFolders.Contains(folder))
            throw new ArgumentException("Thư mục tải ảnh không hợp lệ.", nameof(folder));

        var cloudinary = CreateCloudinary();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var publicId = $"koi/{folder}/{Guid.NewGuid():N}";
        var parameters = new SortedDictionary<string, object>()
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
