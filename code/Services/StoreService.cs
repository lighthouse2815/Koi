using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class StoreService(
    ApplicationDbContext database,
    ImageUploadService uploads,
    ILogger<StoreService> logger
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
        return new AdminStoresViewModel(rows, search, page, pages);
    }

    public Task<StoreEditViewModel> CreateAsync() => Options(new StoreEditViewModel());

    public async Task<StoreEditViewModel?> GetEditAsync(Guid id)
    {
        var value = await database
            .Stores.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null
            ? null
            : await Options(
                new StoreEditViewModel
                {
                    Id = value.Id,
                    Revision = value.Revision,
                    Name = value.Name,
                    Phone = value.Phone,
                    Hours = value.Hours,
                    Address = value.Address,
                    CityId = value.CityId,
                    NavigationUrl = value.NavigationUrl,
                    IsPublished = value.IsPublished,
                    SortOrder = value.SortOrder,
                    CurrentImage = value.Photo,
                }
            );
    }

    public async Task<SaveResult> SaveAsync(
        StoreEditViewModel model,
        Guid? id,
        ModelStateDictionary modelState
    )
    {
        var value = id.HasValue
            ? await database.Stores.SingleOrDefaultAsync(value => value.Id == id)
            : new StoreEntry();
        if (value == null)
            return SaveResult.NotFound;
        model.CurrentImage = value.Photo;
        if (
            model.CityId.HasValue
            && !await database.StoreCities.AnyAsync(value => value.Id == model.CityId)
        )
            modelState.AddModelError(nameof(model.CityId), "Chọn khu vực hợp lệ.");
        if (!SafeLink(model.NavigationUrl))
            modelState.AddModelError(
                nameof(model.NavigationUrl),
                "Nhập liên kết HTTPS hoặc HTTP hợp lệ."
            );
        var image = await uploads.ReadAsync(
            model.ImageUpload,
            modelState,
            nameof(model.ImageUpload)
        );
        if (!modelState.IsValid)
        {
            await Options(model);
            return SaveResult.Invalid;
        }
        string? uploaded = null;
        var result = SaveResult.Conflict;
        try
        {
            if (image != null)
                value.Photo = uploaded = await uploads.SaveAsync(image, "stores");
            else if (model.RemoveImage)
                value.Photo = "";
            value.Name = model.Name.Trim();
            value.Address = model.Address?.Trim() ?? "";
            value.Phone = model.Phone?.Trim() ?? "";
            value.Hours = model.Hours?.Trim() ?? "";
            value.CityId = model.CityId;
            value.NavigationUrl = model.NavigationUrl?.Trim() ?? "";
            value.IsPublished = model.IsPublished;
            value.SortOrder = model.SortOrder;
            if (id == null)
                database.Stores.Add(value);
            else
            {
                database.Entry(value).Property(value => value.Revision).OriginalValue =
                    model.Revision;
                value.Revision++;
            }
            await database.SaveChangesAsync();
            return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            modelState.AddModelError("", "Dữ liệu đã thay đổi. Hãy tải lại trang trước khi lưu.");
        }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save store {StoreId}", id);
            result = SaveResult.Unavailable;
            modelState.AddModelError("", "Chưa lưu được cửa hàng. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded);
        await Options(model);
        return result;
    }

    public async Task<DeleteContentViewModel?> GetDeleteAsync(Guid id)
    {
        var value = await database
            .Stores.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DeleteContentViewModel(id, value.Name);
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

    private async Task<StoreEditViewModel> Options(StoreEditViewModel model)
    {
        model.Cities = await database
            .StoreCities.AsNoTracking()
            .OrderBy(value => value.SortOrder)
            .ToArrayAsync();
        return model;
    }

    internal static bool SafeLink(string? url) =>
        string.IsNullOrWhiteSpace(url)
        || (
            Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https"
            && string.IsNullOrEmpty(uri.UserInfo)
        );
}
