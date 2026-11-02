using Koi.Web.Models;

namespace Koi.Web.Data;

public static class MenuCatalog
{
    public static IReadOnlyList<MenuDocument> Documents
    {
        get;
    } =
    [
        new("KOI Thé", "menu_1772420360655077_0.jpg"),
    ];
}
