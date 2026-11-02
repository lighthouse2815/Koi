using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class GeographyController(StoreCityService geography) : Controller
{
    public async Task<IActionResult> Index(string kind = "cities")
    {
        var model = await geography.GetListAsync(kind);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> Create(string kind = "cities")
    {
        var model = await geography.CreateAsync(kind);
        if (model == null)
            return NotFound();
        ViewData["KindTitle"] = StoreCityService.Title(kind);
        return View("Edit", model);
    }

    public async Task<IActionResult> Edit(Guid id, string kind = "cities")
    {
        var model = await geography.GetEditAsync(id, kind);
        if (model == null)
            return NotFound();
        ViewData["KindTitle"] = StoreCityService.Title(kind);
        return View(model);
    }
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(GeographyEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, GeographyEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(GeographyEditViewModel model, Guid? id)
    {
        var result = await geography.SaveAsync(model, id, ModelState);
        if (result == SaveResult.BadRequest)
            return BadRequest();
        if (result == SaveResult.NotFound)
            return NotFound();
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu dữ liệu.";
            return RedirectToAction(nameof(Index), new
            {
                kind = model.Kind
            });
        }
        Response.StatusCode = result switch
        {
            SaveResult.Invalid => StatusCodes.Status400BadRequest,
            SaveResult.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status503ServiceUnavailable
        };
        ViewData["KindTitle"] = StoreCityService.Title(model.Kind);
        return View("Edit", model);
    }

    public async Task<IActionResult> Delete(Guid id, string kind = "cities")
    {
        var model = await geography.GetDeleteAsync(id, kind);
        return model == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", model);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id, string kind)
    {
        var result = await geography.DeleteAsync(id, kind, ModelState);
        if (result == SaveResult.NotFound)
            return NotFound();
        if (result == SaveResult.Conflict)
        {
            var model = await geography.GetDeleteAsync(id, kind);
            if (model == null)
                return NotFound();
            Response.StatusCode = StatusCodes.Status409Conflict;
            return View("~/code/Views/Admin/Shared/Delete.cshtml", model);
        }
        TempData["Status"] = "Đã xóa dữ liệu.";
        return RedirectToAction(nameof(Index), new
        {
            kind
        });
    }
}
