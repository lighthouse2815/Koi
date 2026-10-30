using Koi.Web.Data;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class StoresController(ApplicationDbContext database, ImageUploads uploads, ILogger<StoresController> logger) : Controller
{
    public async Task<IActionResult> Index(string? search, int marketId = 0, int page = 1)
    {
        search = search?.Trim();
        if (search?.Length > 200) search = search[..200];
        var query = database.Stores.AsNoTracking().Where(value => marketId == 0 || value.MarketId == marketId);
        if (!string.IsNullOrEmpty(search)) query = query.Where(value => EF.Functions.ILike(value.Name, $"%{search}%") || EF.Functions.ILike(value.Address, $"%{search}%"));
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 20d));
        page = Math.Clamp(page, 1, pages);
        var rows = await query.Include(value => value.Market).Include(value => value.City).Include(value => value.Brand)
            .OrderBy(value => value.SortOrder).ThenBy(value => value.Id).Skip((page - 1) * 20).Take(20).ToArrayAsync();
        return View(new AdminStoresViewModel(rows, new SelectList(await database.Markets.OrderBy(value => value.SortOrder).ToArrayAsync(), "Id", "Name"), search, marketId, page, pages));
    }

    public async Task<IActionResult> Create() => View("Edit", await Options(new StoreEditViewModel()));

    public async Task<IActionResult> Edit(int id)
    {
        var value = await database.Stores.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View(await Options(new StoreEditViewModel
        {
            Id = value.Id, Revision = value.Revision, Name = value.Name, Phone = value.Phone, Hours = value.Hours,
            Address = value.Address, MarketId = value.MarketId, CityId = value.CityId, BrandId = value.BrandId,
            NavigationUrl = value.NavigationUrl, IsPublished = value.IsPublished, SortOrder = value.SortOrder,
            CountryOrder = value.CountryOrder, CityOrder = value.CityOrder, CurrentImage = value.Photo
        }));
    }

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(StoreEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(int id, StoreEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(StoreEditViewModel model, int? id)
    {
        var value = id.HasValue ? await database.Stores.SingleOrDefaultAsync(value => value.Id == id) : new StoreEntry();
        if (value == null) return NotFound();
        model.CurrentImage = value.Photo;
        if (!await database.Markets.AnyAsync(value => value.Id == model.MarketId)) ModelState.AddModelError(nameof(model.MarketId), "Chọn quốc gia hợp lệ.");
        if (model.CityId.HasValue && !await database.StoreCities.AnyAsync(value => value.Id == model.CityId && value.MarketId == model.MarketId))
            ModelState.AddModelError(nameof(model.CityId), "Khu vực phải thuộc quốc gia đã chọn.");
        if (!await database.MarketBrands.AnyAsync(value => value.MarketId == model.MarketId && value.BrandId == model.BrandId))
            ModelState.AddModelError(nameof(model.BrandId), "Thương hiệu chưa được gán cho quốc gia này. Cập nhật tại mục Quốc gia.");
        if (!SafeLink(model.NavigationUrl)) ModelState.AddModelError(nameof(model.NavigationUrl), "Nhập liên kết HTTPS hoặc HTTP hợp lệ.");
        var image = await uploads.ReadAsync(model.ImageUpload, ModelState, nameof(model.ImageUpload));
        if (!ModelState.IsValid) { Response.StatusCode = 400; return View("Edit", await Options(model)); }
        string? uploaded = null;
        try
        {
            if (image != null) value.Photo = uploaded = await uploads.SaveAsync(image, "stores");
            else if (model.RemoveImage) value.Photo = "";
            value.Name = model.Name.Trim(); value.Address = model.Address?.Trim() ?? "";
            value.Phone = model.Phone?.Trim() ?? ""; value.Hours = model.Hours?.Trim() ?? "";
            value.MarketId = model.MarketId; value.CityId = model.CityId; value.BrandId = model.BrandId;
            value.NavigationUrl = model.NavigationUrl?.Trim() ?? ""; value.IsPublished = model.IsPublished;
            value.SortOrder = model.SortOrder; value.CountryOrder = model.CountryOrder; value.CityOrder = model.CityOrder;
            if (id == null) database.Stores.Add(value);
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync();
            TempData["Status"] = "Đã lưu cửa hàng.";
            return RedirectToAction(nameof(Index), new { marketId = model.MarketId });
        }
        catch (DbUpdateConcurrencyException) { Response.StatusCode = 409; ModelState.AddModelError("", "Dữ liệu đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save store {StoreId}", id);
            Response.StatusCode = 503; ModelState.AddModelError("", "Chưa lưu được cửa hàng. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded);
        return View("Edit", await Options(model));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var value = await database.Stores.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View("~/Areas/Admin/Views/Shared/Delete.cshtml", new DeleteContentViewModel(id, value.Name));
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var value = await database.Stores.FindAsync(id);
        if (value == null) return NotFound();
        database.Stores.Remove(value); await database.SaveChangesAsync();
        TempData["Status"] = "Đã xóa cửa hàng."; return RedirectToAction(nameof(Index));
    }

    private async Task<StoreEditViewModel> Options(StoreEditViewModel model)
    {
        model.Markets = new SelectList(await database.Markets.AsNoTracking().OrderBy(value => value.SortOrder).ToArrayAsync(), "Id", "Name");
        model.Cities = await database.StoreCities.AsNoTracking().Include(value => value.Market).OrderBy(value => value.SortOrder).ToArrayAsync();
        model.Brands = new SelectList(await database.TeaBrands.AsNoTracking().OrderBy(value => value.SortOrder).ToArrayAsync(), "Id", "Name");
        return model;
    }

    internal static bool SafeLink(string? url) => string.IsNullOrWhiteSpace(url) ||
        (Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" && string.IsNullOrEmpty(uri.UserInfo));
}
