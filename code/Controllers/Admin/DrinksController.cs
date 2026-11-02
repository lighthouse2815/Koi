using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DrinksController(DrinkService drinks) : Controller
{
    public async Task<IActionResult> Index(string? search, Guid categoryId = default, int page = 1) => View(await drinks.GetListAsync(search, categoryId, page));
    public async Task<IActionResult> Create() => View("Edit", await drinks.CreateAsync());
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await drinks.GetEditAsync(id);
        return model == null ? NotFound() : View(model);
    }
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(DrinkEditViewModel model) =>
        Save(model, drinks.CreateAsync(model, ModelState));
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, DrinkEditViewModel model) =>
        id != model.Id
            ? Task.FromResult<IActionResult>(BadRequest())
            : Save(model, drinks.UpdateAsync(id, model, ModelState));

    private async Task<IActionResult> Save(
        DrinkEditViewModel model,
        Task<SaveResult> saveTask)
    {
        var result = await saveTask;
        if (result == SaveResult.NotFound)
            return NotFound();
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu đồ uống.";
            return RedirectToAction(nameof(Index), new
            {
                categoryId = model.CategoryId
            });
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
        var model = await drinks.GetDeleteAsync(id);
        return model == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", model);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await drinks.DeleteAsync(id))
            return NotFound();
        TempData["Status"] = "Đã xóa đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
