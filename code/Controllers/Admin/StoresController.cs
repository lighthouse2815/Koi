using Koi.Web.Models;
using Koi.Web.Filters;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class StoresController(StoreService stores) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1) => View(await stores.GetListAsync(search, page));
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Create(StoreEditViewModel model)
    {
        await stores.CreateAsync(model);
        TempData["Status"] = "Đã lưu cửa hàng.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Edit([FromRoute] Guid id, StoreEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        await stores.UpdateAsync(id, model);
        TempData["Status"] = "Đã lưu cửa hàng.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await stores.DeleteAsync(id))
            return NotFound();
        TempData["Status"] = "Đã xóa cửa hàng.";
        return RedirectToAction(nameof(Index));
    }
}
