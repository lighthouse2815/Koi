using Microsoft.EntityFrameworkCore.Migrations;

namespace Koi.Web.Data;

internal static class MenuSeed
{
    public static void Insert(MigrationBuilder migration)
    {
        foreach (var country in MenuCatalog.Countries)
        {
            var marketId = StoreCatalog.Snapshot.Countries.Single(value => value.Name == country.Name).Id;
            for (var order = 0; order < country.Menus.Count; order++)
            {
                var menu = country.Menus[order];
                migration.InsertData("Menus", ["MarketId", "Label", "File", "SortOrder", "IsPublished", "Revision"],
                    [marketId, menu.Label, "/menus/" + menu.File, order, true, 1]);
            }
        }
    }
}
