using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class StoreCityService(
    ApplicationDbContext database
)
{
    public async Task<GeographyIndexViewModel?> GetListAsync(string kind = "cities")
    {
        if (kind != "cities")
            return null;
        var rows = await database
            .StoreCities.AsNoTracking()
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .Select(value => new GeographyRow(value.Id, value.Name, "", value.SortOrder, value.Revision))
            .ToArrayAsync();
        return new GeographyIndexViewModel(kind, Title(kind), rows);
    }

    public async Task CreateAsync(GeographyEditViewModel model)
    {
        var city = new StoreCity();
        ApplyChanges(city, model);
        database.StoreCities.Add(city);
        await database.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, GeographyEditViewModel model)
    {
        var city = await database.StoreCities.FindAsync(id)
            ?? throw new AppException(ErrorCode.STORE_CITY_NOT_FOUND);

        ApplyChanges(city, model);
        database.Entry(city).Property(value => value.Revision).OriginalValue = model.Revision;
        city.Revision++;
        await database.SaveChangesAsync();
    }

    private static void ApplyChanges(StoreCity city, GeographyEditViewModel model)
    {
        city.Name = model.Name.Trim();
        city.SortOrder = model.SortOrder;
    }

    public async Task<bool> DeleteAsync(Guid id, string kind)
    {
        if (kind != "cities")
            return false;
        var city = await database.StoreCities.SingleOrDefaultAsync(value => value.Id == id);
        if (city == null)
            return false;
        database.StoreCities.Remove(city);
        await database.SaveChangesAsync();
        return true;
    }

    public static string Title(string kind) => "Khu vực";
}
