using Microsoft.EntityFrameworkCore.Migrations;

namespace Koi.Web.Data;

// Run once by AddStoreCatalog. Keeping the imported snapshot makes fresh installations reproducible.
internal static class StoreSeed
{
    public static void Insert(MigrationBuilder migration)
    {
        var data = StoreCatalog.Snapshot;
        for (var order = 0; order < data.Countries.Length; order++)
        {
            var country = data.Countries[order];
            migration.InsertData("Markets", ["Id", "Name", "SortOrder", "Revision"], [country.Id, country.Name, order, 1]);
        }
        for (var order = 0; order < data.Brands.Length; order++)
        {
            var brand = data.Brands[order];
            migration.InsertData("TeaBrands", ["Id", "Name", "Logo", "SortOrder", "Revision"],
                [brand.Id, brand.Name, data.Stores.First(value => value.BrandId == brand.Id).Logo, order, 1]);
        }
        foreach (var country in data.Countries)
        {
            foreach (var brand in country.BrandIds) migration.InsertData("MarketBrands", ["MarketId", "BrandId"], [country.Id, brand]);
            for (var order = 0; order < country.SocialLinks.Length; order++)
            {
                var link = country.SocialLinks[order];
                migration.InsertData("MarketSocialLink", ["MarketId", "Label", "Url", "SortOrder"], [country.Id, link.Label, link.Url, order]);
            }
        }
        for (var order = 0; order < data.Locations.Length; order++)
        {
            var city = data.Locations[order];
            migration.InsertData("StoreCities", ["Id", "Name", "MarketId", "SortOrder", "Revision"], [city.Id, city.Name, city.CountryId, order, 1]);
        }
        foreach (var store in data.Stores)
        {
            var country = data.Countries.Single(value => value.Id == store.CountryId);
            var city = data.Locations.SingleOrDefault(value => value.Id == store.LocationId);
            migration.InsertData("Stores", ["Id", "Name", "Phone", "Hours", "Address", "MarketId", "CityId", "BrandId", "Photo", "NavigationUrl", "IsPublished", "SortOrder", "CountryOrder", "CityOrder", "Revision"],
                [store.Id, store.Name, store.Phone, store.Hours, store.Address, store.CountryId, store.LocationId == 0 ? null! : store.LocationId,
                    store.BrandId, store.Photo, store.NavigationUrl, true, store.Id, Array.IndexOf(country.StoreIds, store.Id), city == null ? 0 : Array.IndexOf(city.StoreIds, store.Id), 1]);
        }
    }
}
