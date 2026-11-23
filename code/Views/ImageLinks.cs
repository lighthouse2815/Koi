using Microsoft.AspNetCore.Mvc;

namespace Koi.Web;

internal static class ImageLinks
{
    public static string? ForNavigation(IUrlHelper url, string value)
    {
        if (url.IsLocalUrl(value))
            return url.Content(value);

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"
            ? value
            : null;
    }
}
