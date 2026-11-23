using CloudinaryDotNet;
using Koi.Web.Exceptions;

namespace Koi.Web.Services;

public class ImageUploadService(IConfiguration configuration)
{
    public object CreateUploadSignature(string folder)
    {
        var allowedFolders = new[] { "categories", "drinks", "featured-drinks", "menus", "news", "stores" };
        if (!allowedFolders.Contains(folder))
            throw new AppException(ErrorCode.UPLOAD_FOLDER_INVALID);

        var api = CreateCloudinary().Api;
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
            cloudName = api.Account.Cloud,
            apiKey = api.Account.ApiKey,
            timestamp,
            publicId,
            allowedFormats = parameters["allowed_formats"],
            overwrite = parameters["overwrite"],
            signature = api.SignParameters(parameters)
        };
    }

    private Cloudinary CreateCloudinary()
    {
        var settings = configuration.GetSection("Cloudinary");
        var cloudName = settings["CloudName"];
        var apiKey = settings["ApiKey"];
        var apiSecret = settings["ApiSecret"];
        if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            throw new AppException(ErrorCode.CLOUDINARY_NOT_CONFIGURED);
        return new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }
}
