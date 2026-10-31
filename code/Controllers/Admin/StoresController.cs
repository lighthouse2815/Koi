using Koi.Web.Models;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class StoresController(StoreAdminService stores) : Controller
{
    public async Task<IActionResult> Index(string? search, Guid marketId = default, int page = 1) => View(await stores.GetListAsync(search, marketId, page));
    public async Task<IActionResult> Create() => View("Edit", await stores.CreateAsync());
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await stores.GetEditAsync(id);
        return model == null ? NotFound() : View(model);
    }
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Create(StoreEditViewModel model) => Save(model, null);
    [HttpPost, RequestSizeLimit(6 * 1024 * 1024)]
    public Task<IActionResult> Edit(Guid id, StoreEditViewModel model) => id != model.Id ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, id);

    private async Task<IActionResult> Save(StoreEditViewModel model, Guid? id)
    {
        var result = await stores.SaveAsync(model, id, ModelState);
        if (result == SaveResult.NotFound) return NotFound();
        if (result == SaveResult.Success)
        {
            TempData["Status"] = "Đã lưu cửa hàng.";
            return RedirectToAction(nameof(Index), new { marketId = model.MarketId });
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
        var model = await stores.GetDeleteAsync(id);
        return model == null ? NotFound() : View("~/code/Views/Admin/Shared/Delete.cshtml", model);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await stores.DeleteAsync(id)) return NotFound();
        TempData["Status"] = "Đã xóa cửa hàng.";
        return RedirectToAction(nameof(Index));
    }
}
