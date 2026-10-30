using System.Globalization;

namespace Koi.Web.Localization;

public class SiteText
{
    public static bool IsVietnamese => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi";
    public static string Select(string english, string? vietnamese) => IsVietnamese && !string.IsNullOrWhiteSpace(vietnamese) ? vietnamese : english;
    public static string Date(DateOnly date) => IsVietnamese ? date.ToString("dd/MM/yyyy") : date.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
}
