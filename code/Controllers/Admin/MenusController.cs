using Koi.Web.Models;
using Koi.Web.Filters;
using Koi.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class MenusController(MenuService menus) : Controller
{
    public async Task<IActionResult> Index() => View(await menus.GetListAsync());
    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Create(MenuEditViewModel model)
    {
        await menus.CreateAsync(model);
        TempData["Status"] = "Đã lưu thực đơn.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Edit([FromRoute] Guid id, MenuEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        await menus.UpdateAsync(id, model);
        TempData["Status"] = "Đã lưu thực đơn.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
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
