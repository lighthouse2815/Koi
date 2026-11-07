using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DrinkCategoriesController(CategoryService categories) : Controller
{
    public async Task<IActionResult> Index() => View(await categories.GetListAsync());
    public IActionResult Create() => View("Edit", new CategoryEditViewModel());
    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await categories.GetByIdAsync(id);
        return View(new CategoryEditViewModel
        {
            Id = category.Id,
            Revision = category.Revision,
            Name = category.Name,
            IsPublished = category.IsPublished,
            ShowOnHome = category.ShowOnHome,
            SortOrder = category.SortOrder,
            CurrentImage = category.Image,
            CurrentIcon = category.Icon
        });
    }
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Create(CategoryEditViewModel model) =>
        HandleSaveAsync(categories.CreateAsync(model, ModelState), model);
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, CategoryEditViewModel model) =>
        id != model.Id
            ? Task.FromResult<IActionResult>(BadRequest())
            : HandleSaveAsync(categories.UpdateAsync(id, model, ModelState), model);

    private async Task<IActionResult> HandleSaveAsync(
        Task<SaveResult> saveTask,
        CategoryEditViewModel model)
    {
        var result = await saveTask;
        if (result == SaveResult.NotFound)
            return NotFound();
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu danh mục đồ uống.";
            return RedirectToAction(nameof(Index));
        }
        Response.StatusCode = result switch
        {
            SaveResult.Invalid => StatusCodes.Status400BadRequest,
            SaveResult.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status503ServiceUnavailable
        };
        return View("Edit", model);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await categories.GetByIdAsync(id);
        return View("~/code/Views/Admin/Shared/Delete.cshtml", new DeleteContentViewModel(category.Id, category.Name));
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var result = await categories.DeleteAsync(id);
        if (result == SaveResult.NotFound)
            return NotFound();
        if (result == SaveResult.Conflict)
        {
            var category = await categories.GetByIdAsync(id);
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View("~/code/Views/Admin/Shared/Delete.cshtml", new DeleteContentViewModel(category.Id, category.Name));
        }
        TempData["Status"] = "Đã xóa danh mục đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
