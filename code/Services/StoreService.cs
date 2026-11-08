using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class StoreService(
    ApplicationDbContext database,
    ImageUploadService uploads
)
{
    public async Task<AdminStoresViewModel> GetListAsync(string? search, int page = 1)
    {
        search = search?.Trim();
        if (search?.Length > 200)
            search = search[..200];
        var query = database.Stores.AsNoTracking();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(value =>
                EF.Functions.ILike(value.Name, $"%{search}%")
                || EF.Functions.ILike(value.Address, $"%{search}%")
            );
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 20d));
        page = Math.Clamp(page, 1, pages);
        var rows = await query
            .Include(value => value.City)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .Skip((page - 1) * 20)
            .Take(20)
            .ToArrayAsync();
        var cities = await database.StoreCities.AsNoTracking()
            .OrderBy(value => value.SortOrder)
            .ToArrayAsync();
        return new AdminStoresViewModel(rows, cities, search, page, pages);
    }

    public async Task CreateAsync(StoreEditViewModel model)
    {
        var store = new Store();
        await ApplyChangesAsync(store, model);
        database.Stores.Add(store);
        await database.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, StoreEditViewModel model)
    {
        var store = await database.Stores.FindAsync(id)
            ?? throw new AppException(ErrorCode.STORE_NOT_FOUND);

        await ApplyChangesAsync(store, model);
        database.Entry(store).Property(value => value.Revision).OriginalValue = model.Revision;
        store.Revision++;
        await database.SaveChangesAsync();
    }

    private async Task ApplyChangesAsync(Store store, StoreEditViewModel model)
    {
        if (model.CityId.HasValue && !await database.StoreCities.AnyAsync(value => value.Id == model.CityId))
            throw new AppException(ErrorCode.STORE_CITY_INVALID);
        if (!SafeLink(model.NavigationUrl))
            throw new AppException(ErrorCode.STORE_LINK_INVALID);

        var image = await uploads.GetImageUrlAsync(model.ImageUploadId, "stores");
        if (image != null)
            store.Photo = image;
        else if (model.RemoveImage)
            store.Photo = "";

        store.Name = model.Name.Trim();
        store.Address = model.Address?.Trim() ?? "";
        store.Phone = model.Phone?.Trim() ?? "";
        store.Hours = model.Hours?.Trim() ?? "";
        store.CityId = model.CityId;
        store.NavigationUrl = model.NavigationUrl?.Trim() ?? "";
        store.IsPublished = model.IsPublished;
        store.SortOrder = model.SortOrder;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var value = await database.Stores.SingleOrDefaultAsync(value => value.Id == id);
        if (value == null)
            return false;
        database.Stores.Remove(value);
        await database.SaveChangesAsync();
        return true;
    }

    internal static bool SafeLink(string? url) =>
        string.IsNullOrWhiteSpace(url)
        || (
            Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https"
            && string.IsNullOrEmpty(uri.UserInfo)
        );
}
