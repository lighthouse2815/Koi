using Koi.Web.Data;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class FeaturedDrinksController(ApplicationDbContext database, ImageUploads uploads, ILogger<FeaturedDrinksController> logger) : Controller
{
    public async Task<IActionResult> Index() => View(await database.FeaturedDrinks.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync());
    public IActionResult Create() => View("Edit", new FeaturedDrinkEditViewModel());
    public async Task<IActionResult> Edit(int id)
    {
        var value = await database.FeaturedDrinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View(new FeaturedDrinkEditViewModel { Id = id, Revision = value.Revision, NameVi = value.NameVi, RegionVi = value.RegionVi, Name = value.Name,
            Region = value.Region, IsPublished = value.IsPublished, SortOrder = value.SortOrder, CurrentImage = value.Image, CurrentBackground = value.BackgroundImage });
    }
    [HttpPost, RequestSizeLimit(12 * 1024 * 1024)]
    public Task<IActionResult> Create(FeaturedDrinkEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(12 * 1024 * 1024)]
    public Task<IActionResult> Edit(int id, FeaturedDrinkEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(FeaturedDrinkEditViewModel model, int? id)
    {
        var value = id.HasValue ? await database.FeaturedDrinks.SingleOrDefaultAsync(value => value.Id == id) : new FeaturedDrink();
        if (value == null) return NotFound(); model.CurrentImage = value.Image; model.CurrentBackground = value.BackgroundImage;
        var image = await uploads.ReadAsync(model.ImageUpload, ModelState, nameof(model.ImageUpload));
        var background = await uploads.ReadAsync(model.BackgroundUpload, ModelState, nameof(model.BackgroundUpload));
        if (image == null && string.IsNullOrEmpty(value.Image)) ModelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh đồ uống.");
        if (background == null && string.IsNullOrEmpty(value.BackgroundImage)) ModelState.AddModelError(nameof(model.BackgroundUpload), "Chọn ảnh nền.");
        if (!ModelState.IsValid) { Response.StatusCode = 400; return View("Edit", model); }
        string? uploaded = null, uploadedBackground = null;
        try
        {
            if (image != null) value.Image = uploaded = await uploads.SaveAsync(image, "featured-drinks");
            if (background != null) value.BackgroundImage = uploadedBackground = await uploads.SaveAsync(background, "featured-drinks");
            value.Name = model.Name.Trim();
            value.NameVi = model.NameVi?.Trim() ?? ""; value.RegionVi = model.RegionVi?.Trim() ?? ""; value.Region = model.Region?.Trim() ?? "";
            value.SortOrder = model.SortOrder; value.IsPublished = model.IsPublished;
            if (id == null) database.FeaturedDrinks.Add(value);
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync(); TempData["Status"] = "Đã lưu mục New Drinks."; return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException) { Response.StatusCode = 409; ModelState.AddModelError("", "Mục này đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save featured drink {Id}", id); Response.StatusCode = 503;
            ModelState.AddModelError("", "Chưa lưu được mục New Drinks. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded); uploads.Rollback(uploadedBackground); return View("Edit", model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var value = await database.FeaturedDrinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", new DeleteContentViewModel(id, value.Name));
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var value = await database.FeaturedDrinks.FindAsync(id); if (value == null) return NotFound();
        database.FeaturedDrinks.Remove(value); await database.SaveChangesAsync(); TempData["Status"] = "Đã xóa mục New Drinks."; return RedirectToAction(nameof(Index));
    }
}
