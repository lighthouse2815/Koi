using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class GeographyAdminService(ApplicationDbContext database, ILogger<GeographyAdminService> logger)
{
    public async Task<GeographyIndexViewModel?> GetListAsync(string kind = "cities")
    {
        if (kind != "cities") return null;
        var rows = await database.StoreCities.AsNoTracking()
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id)
            .Select(value => new GeographyRow(value.Id, value.Name, "", value.SortOrder)).ToArrayAsync();
        return new GeographyIndexViewModel(kind, Title(kind), rows);
    }

    public Task<GeographyEditViewModel?> CreateAsync(string kind) => Task.FromResult<GeographyEditViewModel?>(kind == "cities"
        ? new GeographyEditViewModel { Kind = kind } : null);

    public async Task<GeographyEditViewModel?> GetEditAsync(Guid id, string kind = "cities")
    {
        if (kind != "cities") return null;
        var city = await database.StoreCities.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return city == null ? null : new GeographyEditViewModel { Id = city.Id, Revision = city.Revision, Kind = kind,
            Name = city.Name, SortOrder = city.SortOrder };
    }

    public async Task<SaveResult> SaveAsync(GeographyEditViewModel model, Guid? id, ModelStateDictionary modelState)
    {
        if (model.Kind != "cities") return SaveResult.BadRequest;
        var city = id.HasValue ? await database.StoreCities.SingleOrDefaultAsync(value => value.Id == id) : new StoreCity();
        if (city == null) return SaveResult.NotFound;
        if (!modelState.IsValid) return SaveResult.Invalid;
        city.Name = model.Name.Trim();
        city.SortOrder = model.SortOrder;
        if (id == null) database.StoreCities.Add(city);
        else { database.Entry(city).Property(value => value.Revision).OriginalValue = model.Revision; city.Revision++; }
        try { await database.SaveChangesAsync(); return SaveResult.Success; }
        catch (DbUpdateConcurrencyException) { modelState.AddModelError("", "Dữ liệu đã thay đổi. Hãy tải lại trang sửa trước khi lưu."); return SaveResult.Conflict; }
        catch (DbUpdateException exception) { logger.LogError(exception, "Could not save city {Id}", id); modelState.AddModelError("", "Chưa lưu được dữ liệu. Vui lòng thử lại."); return SaveResult.Unavailable; }
    }

    public async Task<DeleteContentViewModel?> GetDeleteAsync(Guid id, string kind)
    {
        if (kind != "cities") return null;
        var city = await database.StoreCities.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return city == null ? null : new DeleteContentViewModel(city.Id, city.Name, kind);
    }

    public async Task<SaveResult> DeleteAsync(Guid id, string kind, ModelStateDictionary modelState)
    {
        if (kind != "cities") return SaveResult.NotFound;
        var city = await database.StoreCities.SingleOrDefaultAsync(value => value.Id == id);
        if (city == null) return SaveResult.NotFound;
        try { database.StoreCities.Remove(city); await database.SaveChangesAsync(); return SaveResult.Success; }
        catch (DbUpdateException) { modelState.AddModelError("", "Khu vực đang được sử dụng bởi chi nhánh."); return SaveResult.Conflict; }
    }

    public static string Title(string kind) => "Khu vực";
}
