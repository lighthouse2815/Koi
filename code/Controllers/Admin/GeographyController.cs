using Koi.Web.Models;
using Koi.Web.Filters;
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

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Create(GeographyEditViewModel model)
    {
        await geography.CreateAsync(model);
        TempData["Status"] = "Đã lưu dữ liệu.";
        return Json(new { redirectUrl = Url.Action(nameof(Index), new { kind = model.Kind }) });
    }

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Edit([FromRoute] Guid id, GeographyEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        await geography.UpdateAsync(id, model);
        TempData["Status"] = "Đã lưu dữ liệu.";
        return Json(new { redirectUrl = Url.Action(nameof(Index), new { kind = model.Kind }) });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id, string kind)
    {
        if (!await geography.DeleteAsync(id, kind))
            return NotFound();
        TempData["Status"] = "Đã xóa dữ liệu.";
        return RedirectToAction(nameof(Index), new
        {
            kind
        });
    }
}
