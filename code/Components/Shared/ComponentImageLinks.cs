using Microsoft.AspNetCore.Http.HttpResults;

namespace Koi.Web.Components.Shared;

internal static class ComponentImageLinks
{
    public static string? ForNavigation(string value)
    {
        if (RedirectHttpResult.IsLocalUrl(value))
            return value.StartsWith("~/", StringComparison.Ordinal) ? value[1..] : value;

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"
            ? value
            : null;
    }
}
