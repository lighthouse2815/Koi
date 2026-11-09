using Koi.Web.Models;
using Koi.Web.Filters;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class StoreCitiesController(StoreCityService cities) : Controller
{
    public async Task<IActionResult> Index(string kind = "cities")
    {
        var model = await cities.GetListAsync(kind);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Create(GeographyEditViewModel model)
    {
        await cities.CreateAsync(model);
        TempData["Status"] = "Đã lưu dữ liệu.";
        return Json(new { redirectUrl = Url.Action(nameof(Index), new { kind = model.Kind }) });
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Edit([FromRoute] Guid id, GeographyEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        await cities.UpdateAsync(id, model);
        TempData["Status"] = "Đã lưu dữ liệu.";
        return Json(new { redirectUrl = Url.Action(nameof(Index), new { kind = model.Kind }) });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id, string kind)
    {
        if (!await cities.DeleteAsync(id, kind))
            return NotFound();
        TempData["Status"] = "Đã xóa dữ liệu.";
        return RedirectToAction(nameof(Index), new
        {
            kind
        });
    }
}
