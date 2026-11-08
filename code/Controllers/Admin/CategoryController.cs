using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoryController(CategoryService categories) : Controller
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
        HandleSaveAsync(categories.CreateAsync(model, ModelState));

    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Edit([FromForm] CategoryEditViewModel model) =>
        HandleSaveAsync(categories.UpdateAsync(model, ModelState));

    private async Task<IActionResult> HandleSaveAsync(Task saveTask)
    {
        await saveTask;
        TempData["Status"] = "Đã lưu danh mục đồ uống.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await categories.GetByIdAsync(id);
        return View("~/code/Views/Admin/Shared/Delete.cshtml", new DeleteContentViewModel(category.Id, category.Name));
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await categories.DeleteAsync(id);
        TempData["Status"] = "Đã xóa danh mục đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
