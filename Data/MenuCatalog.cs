using Koi.Web.Models;

namespace Koi.Web.Data;

public static class MenuCatalog
{
    public static IReadOnlyList<MenuCountry> Countries { get; } =
    [
        new("SINGAPORE",
        [
            new("KOI Thé", "menu_803.png"),
            new("KOI Thé Express", "menu_1773625244840236_0.png"),
            new("Signature KOI", "menu_196.png"),
        ]),
        new("CHINA",
        [
            new("KOI PLUS-China", "menu_1770704210330852_0.png"),
            new("KOI Thé-Macau, China", "menu_1770704243540267_0.png"),
            new("KOI Thé-Hong Kong, China", "menu_1770704288979111_0.png"),
        ]),
        new("INDONESIA",
        [
            new("KOI Thé", "menu_1770688296852018_0.jpeg"),
        ]),
        new("CAMBODIA",
        [
            new("KOI Thé", "menu_1770608542155817_0.jpg"),
        ]),
        new("JAPAN",
        [
            new("KOI Thé", "menu_1770088037400963_0.jpg"),
        ]),
        new("VIETNAM",
        [
            new("KOI Thé", "menu_1772420360655077_0.jpg"),
        ]),
        new("THAILAND",
        [
            new("KOI Thé", "menu_1770272104855109_0.jpg"),
            new("KOI Thé Express", "menu_207.png"),
        ]),
        new("MALAYSIA",
        [
            new("KOI Thé", "menu_1770172471045047_0.png"),
        ]),
        new("USA",
        [
            new("FIFTYLAN", "menu_1770602499259791_0.jpg"),
        ]),
        new("DUBAI",
        [
            new("KOI Thé", "menu_670.png"),
        ]),
        new("PHILIPPINES",
        [
            new("KOI Thé", "menu_1778467151696168_0.jpg"),
        ]),
        new("BANGLADESH",
        [
            new("KOI Thé", "menu_1788233321211640_0.jpg"),
        ]),
        new("LAOS",
        [
            new("KOI Thé", "menu_653.jpg"),
        ]),
        new("CANADA",
        [
            new("FIFTYLAN", "menu_1770602532413945_0.jpg"),
        ]),
        new("FRANCE",
        [
            new("KOI Thé", "menu_1789609954060168_0.png"),
        ]),
        new("BRUNEI",
        [
            new("KOI Thé", "menu_1.jpeg"),
        ]),
        new("AUSTRALIA",
        [
            new("FIFTYLAN", "menu_1770607615376631_0.png"),
        ]),
    ];
}
