using Koi.Web.Data;
using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DrinkCategoriesController(ApplicationDbContext database, ImageUploads uploads, ILogger<DrinkCategoriesController> logger) : Controller
{
    public async Task<IActionResult> Index() => View(await database.DrinkCategories.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync());
    public IActionResult Create() => View("Edit", new DrinkCategoryEditViewModel());
    public async Task<IActionResult> Edit(int id)
    {
        var value = await database.DrinkCategories.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View(new DrinkCategoryEditViewModel { Id = id, Revision = value.Revision, NameVi = value.NameVi, Name = value.Name,
            IsPublished = value.IsPublished, ShowOnHome = value.ShowOnHome, SortOrder = value.SortOrder, HomeOrder = value.HomeOrder,
            CurrentImage = value.Image, CurrentIcon = value.Icon, CurrentHomeImage = value.HomeImage, CurrentHomeIcon = value.HomeIcon });
    }
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Create(DrinkCategoryEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Edit(int id, DrinkCategoryEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(DrinkCategoryEditViewModel model, int? id)
    {
        var value = id.HasValue ? await database.DrinkCategories.SingleOrDefaultAsync(value => value.Id == id) : new DrinkCategory();
        if (value == null) return NotFound();
        model.CurrentImage = value.Image; model.CurrentIcon = value.Icon;
        model.CurrentHomeImage = value.HomeImage; model.CurrentHomeIcon = value.HomeIcon;
        var image = await uploads.ReadAsync(model.ImageUpload, ModelState, nameof(model.ImageUpload));
        var icon = await uploads.ReadAsync(model.IconUpload, ModelState, nameof(model.IconUpload));
        var homeImage = await uploads.ReadAsync(model.HomeImageUpload, ModelState, nameof(model.HomeImageUpload));
        var homeIcon = await uploads.ReadAsync(model.HomeIconUpload, ModelState, nameof(model.HomeIconUpload));
        if (image == null && string.IsNullOrEmpty(value.Image)) ModelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh chung cho danh mục.");
        if (model.ShowOnHome && homeImage == null && string.IsNullOrEmpty(value.HomeImage)) ModelState.AddModelError(nameof(model.HomeImageUpload), "Chọn ảnh thẻ trang chủ.");
        if (model.ShowOnHome && homeIcon == null && string.IsNullOrEmpty(value.HomeIcon)) ModelState.AddModelError(nameof(model.HomeIconUpload), "Chọn biểu tượng thẻ trang chủ.");
        if (!ModelState.IsValid) { Response.StatusCode = 400; return View("Edit", model); }
        var uploaded = new List<string>();
        async Task<string> SaveImage(ImageUpload? file, string current)
        {
            if (file == null) return current;
            var path = await uploads.SaveAsync(file, "categories"); uploaded.Add(path); return path;
        }
        try
        {
            value.Image = await SaveImage(image, value.Image); value.Icon = await SaveImage(icon, model.RemoveIcon ? "" : value.Icon);
            value.HomeImage = await SaveImage(homeImage, value.HomeImage); value.HomeIcon = await SaveImage(homeIcon, value.HomeIcon);
            value.Name = model.Name.Trim();
            value.NameVi = model.NameVi?.Trim() ?? ""; value.SortOrder = model.SortOrder; value.HomeOrder = model.HomeOrder;
            value.ShowOnHome = model.ShowOnHome; value.IsPublished = model.IsPublished;
            if (id == null) database.DrinkCategories.Add(value);
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync(); TempData["Status"] = "Đã lưu danh mục đồ uống."; return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException) { Response.StatusCode = 409; ModelState.AddModelError("", "Danh mục đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save drink category {Id}", id); Response.StatusCode = 503;
            ModelState.AddModelError("", "Chưa lưu được danh mục. Vui lòng thử lại.");
        }
        uploaded.ForEach(uploads.Rollback); return View("Edit", model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var value = await database.DrinkCategories.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", new DeleteContentViewModel(id, value.Name));
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var value = await database.DrinkCategories.FindAsync(id); if (value == null) return NotFound();
        try { database.DrinkCategories.Remove(value); await database.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            Response.StatusCode = 409; ModelState.AddModelError("", "Danh mục đang có đồ uống. Hãy chuyển hoặc xóa đồ uống trước, hoặc tắt hiển thị danh mục.");
            return View("~/code/Views/Admin/Shared/Delete.cshtml", new DeleteContentViewModel(id, value.Name));
        }
        TempData["Status"] = "Đã xóa danh mục đồ uống."; return RedirectToAction(nameof(Index));
    }
}
