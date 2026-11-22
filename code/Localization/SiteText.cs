using System.Globalization;

namespace Koi.Web.Localization;

public class SiteText
{
    public static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
