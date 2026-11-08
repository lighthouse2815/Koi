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
        Task<SaveResult> saveTask,
        CategoryEditViewModel model)
    {
        SaveResult result;
        try
        {
            result = await saveTask;
        }
        catch (DbUpdateConcurrencyException) when (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            throw new AppException(ErrorCode.CATEGORY_CHANGED);
        }
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu danh mục đồ uống.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }
        Response.StatusCode = result switch
        {
            SaveResult.Invalid => StatusCodes.Status400BadRequest,
            SaveResult.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status503ServiceUnavailable
        };
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var message = ModelState.Values.SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
                ?? "Không thể lưu danh mục. Vui lòng thử lại.";
            throw new AppException(new ErrorCode("CATEGORY_SAVE_FAILED", message, Response.StatusCode));
        }
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
        await categories.DeleteAsync(id);
        TempData["Status"] = "Đã xóa danh mục đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
