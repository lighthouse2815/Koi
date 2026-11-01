using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class StoreAdminService(ApplicationDbContext database, ImageUploads uploads, ILogger<StoreAdminService> logger)
{
    public async Task<AdminStoresViewModel> GetListAsync(string? search, Guid marketId = default, int page = 1)
    {
        search = search?.Trim();
        if (search?.Length > 200) search = search[..200];
        var shop = await database.ShopSettings.AsNoTracking().SingleAsync(value => value.Id == 1);
        var query = database.Stores.AsNoTracking().Where(value => (!shop.MarketId.HasValue || value.MarketId == shop.MarketId)
            && (!shop.BrandId.HasValue || value.BrandId == shop.BrandId) && (marketId == Guid.Empty || value.MarketId == marketId));
        if (!string.IsNullOrEmpty(search)) query = query.Where(value => EF.Functions.ILike(value.Name, $"%{search}%") || EF.Functions.ILike(value.Address, $"%{search}%") || EF.Functions.ILike(value.NameVi, $"%{search}%") || EF.Functions.ILike(value.AddressVi, $"%{search}%"));
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 20d));
        page = Math.Clamp(page, 1, pages);
        var rows = await query.Include(value => value.Market).Include(value => value.City).Include(value => value.Brand)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).Skip((page - 1) * 20).Take(20).ToArrayAsync();
        return new AdminStoresViewModel(rows, new SelectList(await database.Markets.OrderBy(value => value.SortOrder).ToArrayAsync(), "Id", "Name"), search, marketId, page, pages);
    }

    public Task<StoreEditViewModel> CreateAsync() => Options(new StoreEditViewModel());

    public async Task<StoreEditViewModel?> GetEditAsync(Guid id)
    {
        var value = await ShopStores().AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : await Options(new StoreEditViewModel
        {
            Id = value.Id, Revision = value.Revision, NameVi = value.NameVi, AddressVi = value.AddressVi, HoursVi = value.HoursVi, Name = value.Name, Phone = value.Phone, Hours = value.Hours,
            Address = value.Address, MarketId = value.MarketId, CityId = value.CityId, BrandId = value.BrandId,
            NavigationUrl = value.NavigationUrl, IsPublished = value.IsPublished, SortOrder = value.SortOrder,
            CountryOrder = value.CountryOrder, CityOrder = value.CityOrder, CurrentImage = value.Photo
        });
    }

    public async Task<SaveResult> SaveAsync(StoreEditViewModel model, Guid? id, ModelStateDictionary modelState)
    {
        var value = id.HasValue ? await ShopStores().SingleOrDefaultAsync(value => value.Id == id) : new StoreEntry();
        if (value == null) return SaveResult.NotFound;
        model.CurrentImage = value.Photo;
        var shop = await database.ShopSettings.AsNoTracking().SingleAsync(value => value.Id == 1);
        if ((shop.MarketId.HasValue && model.MarketId != shop.MarketId) || (shop.BrandId.HasValue && model.BrandId != shop.BrandId))
            modelState.AddModelError("", "Chi nhánh phải thuộc chuỗi KOI Thé tại Việt Nam.");
        if (!await database.Markets.AnyAsync(value => value.Id == model.MarketId)) modelState.AddModelError(nameof(model.MarketId), "Chọn quốc gia hợp lệ.");
        if (model.CityId.HasValue && !await database.StoreCities.AnyAsync(value => value.Id == model.CityId && value.MarketId == model.MarketId))
            modelState.AddModelError(nameof(model.CityId), "Khu vực phải thuộc quốc gia đã chọn.");
        if (!await database.MarketBrands.AnyAsync(value => value.MarketId == model.MarketId && value.BrandId == model.BrandId))
            modelState.AddModelError(nameof(model.BrandId), "Thương hiệu chưa được gán cho quốc gia này. Cập nhật tại mục Quốc gia.");
        if (!SafeLink(model.NavigationUrl)) modelState.AddModelError(nameof(model.NavigationUrl), "Nhập liên kết HTTPS hoặc HTTP hợp lệ.");
        var image = await uploads.ReadAsync(model.ImageUpload, modelState, nameof(model.ImageUpload));
        if (!modelState.IsValid) { await Options(model); return SaveResult.Invalid; }
        string? uploaded = null;
        var result = SaveResult.Conflict;
        try
        {
            if (image != null) value.Photo = uploaded = await uploads.SaveAsync(image, "stores");
            else if (model.RemoveImage) value.Photo = "";
            value.Name = model.Name.Trim();
            value.NameVi = model.NameVi?.Trim() ?? ""; value.AddressVi = model.AddressVi?.Trim() ?? ""; value.HoursVi = model.HoursVi?.Trim() ?? ""; value.Address = model.Address?.Trim() ?? "";
            value.Phone = model.Phone?.Trim() ?? ""; value.Hours = model.Hours?.Trim() ?? "";
            value.MarketId = model.MarketId; value.CityId = model.CityId; value.BrandId = model.BrandId;
            value.NavigationUrl = model.NavigationUrl?.Trim() ?? ""; value.IsPublished = model.IsPublished;
            value.SortOrder = model.SortOrder; value.CountryOrder = model.CountryOrder; value.CityOrder = model.CityOrder;
            if (id == null) database.Stores.Add(value);
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync();
            return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException) { modelState.AddModelError("", "Dữ liệu đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save store {StoreId}", id);
            result = SaveResult.Unavailable; modelState.AddModelError("", "Chưa lưu được cửa hàng. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded);
        await Options(model); return result;
    }

    public async Task<DeleteContentViewModel?> GetDeleteAsync(Guid id)
    {
        var value = await ShopStores().AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DeleteContentViewModel(id, value.Name);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var value = await ShopStores().SingleOrDefaultAsync(value => value.Id == id);
        if (value == null) return false;
        database.Stores.Remove(value);
        await database.SaveChangesAsync();
        return true;
    }

    private IQueryable<StoreEntry> ShopStores() => database.Stores.Where(store => database.ShopSettings.Any(shop => shop.Id == 1
        && (!shop.MarketId.HasValue || shop.MarketId == store.MarketId) && (!shop.BrandId.HasValue || shop.BrandId == store.BrandId)));

    private async Task<StoreEditViewModel> Options(StoreEditViewModel model)
    {
        var shop = await database.ShopSettings.AsNoTracking().SingleAsync(value => value.Id == 1);
        if (model.MarketId == Guid.Empty && shop.MarketId.HasValue) model.MarketId = shop.MarketId.Value;
        if (model.BrandId == Guid.Empty && shop.BrandId.HasValue) model.BrandId = shop.BrandId.Value;
        model.Markets = new SelectList(await database.Markets.AsNoTracking().Where(value => !shop.MarketId.HasValue || value.Id == shop.MarketId).OrderBy(value => value.SortOrder).ToArrayAsync(), "Id", "Name");
        model.Cities = await database.StoreCities.AsNoTracking().Include(value => value.Market).Where(value => !shop.MarketId.HasValue || value.MarketId == shop.MarketId).OrderBy(value => value.SortOrder).ToArrayAsync();
        model.Brands = new SelectList(await database.TeaBrands.AsNoTracking().OrderBy(value => value.SortOrder).ToArrayAsync(), "Id", "Name");
        return model;
    }

    internal static bool SafeLink(string? url) => string.IsNullOrWhiteSpace(url) ||
        (Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" && string.IsNullOrEmpty(uri.UserInfo));
}
