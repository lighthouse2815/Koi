using System.Globalization;

namespace Koi.Web.Localization;

public class SiteText
{
    public static bool IsVietnamese => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi";
    public static string Date(DateOnly date) => IsVietnamese ? date.ToString("dd/MM/yyyy") : date.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
}
