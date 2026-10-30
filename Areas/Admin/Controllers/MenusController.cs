using Koi.Web.Data;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class MenusController(ApplicationDbContext database, ImageUploads uploads, ILogger<MenusController> logger) : Controller
{
    public async Task<IActionResult> Index(int marketId = 0) => View(new AdminMenusViewModel(
        await database.Menus.AsNoTracking().Include(value => value.Market).Where(value => marketId == 0 || value.MarketId == marketId)
            .OrderBy(value => value.Market.SortOrder).ThenBy(value => value.MarketId).ThenBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync(),
        await Markets(), marketId));

    public async Task<IActionResult> Create() => View("Edit", new MenuEditViewModel { Markets = await Markets() });
    public async Task<IActionResult> Edit(int id)
    {
        var value = await database.Menus.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View(new MenuEditViewModel { Id = id, Revision = value.Revision,
            Label = value.Label, MarketId = value.MarketId, SortOrder = value.SortOrder, IsPublished = value.IsPublished,
            CurrentImage = value.File, Markets = await Markets() });
    }

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(MenuEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(int id, MenuEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(MenuEditViewModel model, int? id)
    {
        var value = id.HasValue ? await database.Menus.SingleOrDefaultAsync(value => value.Id == id) : new MenuEntry();
        if (value == null) return NotFound();
        model.CurrentImage = value.File;
        if (!await database.Markets.AnyAsync(value => value.Id == model.MarketId)) ModelState.AddModelError(nameof(model.MarketId), "Chọn quốc gia hợp lệ.");
        var image = await uploads.ReadAsync(model.ImageUpload, ModelState, nameof(model.ImageUpload));
        if (id == null && image == null) ModelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh thực đơn.");
        if (!ModelState.IsValid) { Response.StatusCode = 400; model.Markets = await Markets(); return View("Edit", model); }
        string? uploaded = null;
        try
        {
            if (image != null) value.File = uploaded = await uploads.SaveAsync(image, "menus");
            value.Label = model.Label.Trim(); value.MarketId = model.MarketId; value.SortOrder = model.SortOrder; value.IsPublished = model.IsPublished;
            if (id == null) database.Menus.Add(value);
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync(); TempData["Status"] = "Đã lưu thực đơn.";
            return RedirectToAction(nameof(Index), new { marketId = model.MarketId });
        }
        catch (DbUpdateConcurrencyException) { Response.StatusCode = 409; ModelState.AddModelError("", "Thực đơn đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save menu {Id}", id);
            Response.StatusCode = 503; ModelState.AddModelError("", "Chưa lưu được thực đơn. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded); model.Markets = await Markets(); return View("Edit", model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var value = await database.Menus.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View("~/Areas/Admin/Views/Shared/Delete.cshtml", new DeleteContentViewModel(id, value.Label));
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var value = await database.Menus.FindAsync(id); if (value == null) return NotFound();
        database.Menus.Remove(value); await database.SaveChangesAsync(); TempData["Status"] = "Đã xóa thực đơn."; return RedirectToAction(nameof(Index));
    }
    private async Task<SelectList> Markets() => new(await database.Markets.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync(), "Id", "Name");
}
