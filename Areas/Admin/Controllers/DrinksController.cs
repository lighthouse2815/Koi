using Koi.Web.Data;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DrinksController(ApplicationDbContext database, ImageUploads uploads, ILogger<DrinksController> logger) : Controller
{
    public async Task<IActionResult> Index(string? search, int categoryId = 0, int page = 1)
    {
        search = search?.Trim(); if (search?.Length > 200) search = search[..200];
        var query = database.Drinks.AsNoTracking().Where(value => categoryId == 0 || value.CategoryId == categoryId);
        if (!string.IsNullOrEmpty(search)) query = query.Where(value => EF.Functions.ILike(value.Name, $"%{search}%"));
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 20d)); page = Math.Clamp(page, 1, pages);
        return View(new AdminDrinksViewModel(await query.Include(value => value.Category).OrderBy(value => value.Category.SortOrder)
            .ThenBy(value => value.CategoryId).ThenBy(value => value.SortOrder).ThenBy(value => value.Id).Skip((page - 1) * 20).Take(20).ToArrayAsync(),
            await Categories(), categoryId, search, page, pages));
    }

    public async Task<IActionResult> Create() => View("Edit", new DrinkEditViewModel { Categories = await Categories() });
    public async Task<IActionResult> Edit(int id)
    {
        var value = await database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View(new DrinkEditViewModel { Id = id, Revision = value.Revision, Name = value.Name,
            CategoryId = value.CategoryId, Region = value.Region, Description = value.Description, SortOrder = value.SortOrder,
            IsPublished = value.IsPublished, CurrentImage = value.Image, Categories = await Categories() });
    }
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(DrinkEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(int id, DrinkEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(DrinkEditViewModel model, int? id)
    {
        var value = id.HasValue ? await database.Drinks.SingleOrDefaultAsync(value => value.Id == id) : new Drink();
        if (value == null) return NotFound(); model.CurrentImage = value.Image;
        if (!await database.DrinkCategories.AnyAsync(value => value.Id == model.CategoryId)) ModelState.AddModelError(nameof(model.CategoryId), "Chọn danh mục hợp lệ.");
        var image = await uploads.ReadAsync(model.ImageUpload, ModelState, nameof(model.ImageUpload));
        if (!ModelState.IsValid) { Response.StatusCode = 400; model.Categories = await Categories(); return View("Edit", model); }
        string? uploaded = null;
        try
        {
            if (image != null) value.Image = uploaded = await uploads.SaveAsync(image, "drinks");
            else if (model.RemoveImage) value.Image = "";
            value.Name = model.Name.Trim(); value.CategoryId = model.CategoryId; value.Region = model.Region?.Trim() ?? "";
            value.Description = model.Description?.Trim() ?? ""; value.SortOrder = model.SortOrder; value.IsPublished = model.IsPublished;
            if (id == null) database.Drinks.Add(value);
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync(); TempData["Status"] = "Đã lưu đồ uống.";
            return RedirectToAction(nameof(Index), new { categoryId = model.CategoryId });
        }
        catch (DbUpdateConcurrencyException) { Response.StatusCode = 409; ModelState.AddModelError("", "Đồ uống đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save drink {Id}", id); Response.StatusCode = 503;
            ModelState.AddModelError("", "Chưa lưu được đồ uống. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded); model.Categories = await Categories(); return View("Edit", model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var value = await database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View("~/Areas/Admin/Views/Shared/Delete.cshtml", new DeleteContentViewModel(id, value.Name));
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var value = await database.Drinks.FindAsync(id); if (value == null) return NotFound();
        database.Drinks.Remove(value); await database.SaveChangesAsync(); TempData["Status"] = "Đã xóa đồ uống."; return RedirectToAction(nameof(Index));
    }
    private async Task<SelectList> Categories() => new(await database.DrinkCategories.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync(), "Id", "Name");
}
