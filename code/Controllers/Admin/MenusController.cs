using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class MenusController(MenuService menus) : Controller
{
    public async Task<IActionResult> Index() => View(await menus.GetListAsync());
    public IActionResult Create() => View("Edit", new MenuEditViewModel());
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await menus.GetEditAsync(id);
        return model == null ? NotFound() : View(model);
    }
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(MenuEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, MenuEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(MenuEditViewModel model, Guid? id)
    {
        var result = await menus.SaveAsync(model, id, ModelState);
        if (result == SaveResult.NotFound)
            return NotFound();
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu thực đơn.";
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
        var model = await menus.GetDeleteAsync(id);
        return model == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", model);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await menus.DeleteAsync(id))
            return NotFound();
        TempData["Status"] = "Đã xóa thực đơn.";
        return RedirectToAction(nameof(Index));
    }
}
