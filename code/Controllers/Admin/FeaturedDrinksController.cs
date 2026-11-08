using Koi.Web.Models;
using Koi.Web.Filters;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class FeaturedDrinksController(FeaturedDrinkService drinks) : Controller
{
    public async Task<IActionResult> Index() => View(await drinks.GetListAsync());
    [HttpPost, RequestSizeLimit(12 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Create(FeaturedDrinkEditViewModel model)
    {
        await drinks.CreateAsync(model);
        TempData["Status"] = "Đã lưu mục New Drinks.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost, RequestSizeLimit(12 * 1024 * 1024)]
    [ValidateModel]
    public async Task<IActionResult> Edit([FromRoute] Guid id, FeaturedDrinkEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        await drinks.UpdateAsync(id, model);
        TempData["Status"] = "Đã lưu mục New Drinks.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!await drinks.DeleteAsync(id))
            return NotFound();
        TempData["Status"] = "Đã xóa mục New Drinks.";
        return RedirectToAction(nameof(Index));
    }
}
