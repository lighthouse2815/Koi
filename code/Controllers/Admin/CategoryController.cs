using Koi.Web.Models;
using Koi.Web.Filters;
using Koi.Web.Services;
using Koi.Web.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[CategoryNameConflictFilter]
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
        HandleSaveAsync(categories.CreateAsync(model, ModelState), model);
    [HttpPost, RequestSizeLimit(22 * 1024 * 1024)]
    public Task<IActionResult> Edit([FromForm] CategoryEditViewModel model) =>
        HandleSaveAsync(categories.UpdateAsync(model, ModelState), model);

    private async Task<IActionResult> HandleSaveAsync(
        Task saveTask,
        CategoryEditViewModel model)
    {
        try
        {
            await saveTask;
        }
        catch (DbUpdateConcurrencyException) when (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            throw new AppException(ErrorCode.CATEGORY_CHANGED);
        }
        catch (AppException exception) when (
            Request.Headers["X-Requested-With"] != "XMLHttpRequest"
            && exception.Error.Code == ErrorCode.CATEGORY_INVALID.Code)
        {
            Response.StatusCode = exception.Error.StatusCode;
            return View("Edit", model);
        }
        TempData["Status"] = "Đã lưu danh mục đồ uống.";
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { redirectUrl = Url.Action(nameof(Index)) });
        return RedirectToAction(nameof(Index));
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
