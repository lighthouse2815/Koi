using System.ComponentModel.DataAnnotations;
using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class SaleOptionsController(ApplicationDbContext database) : Controller
{
    public async Task<IActionResult> Index() => View(await database.SaleOptions.AsNoTracking().OrderBy(value => value.Kind).ThenBy(value => value.Name).ToArrayAsync());

    [HttpPost]
    public async Task<IActionResult> Save(Guid? id, [Required, StringLength(100)] string name, SaleOptionKind kind, [Range(0, 999999999)] decimal extraPrice, bool isActive)
    {
        if (!Enum.IsDefined(kind) || decimal.Truncate(extraPrice) != extraPrice || !ModelState.IsValid)
        {
            TempData["Status"] = "Vui lòng nhập tên, loại và giá cộng thêm hợp lệ (số nguyên không âm).";
            return RedirectToAction(nameof(Index));
        }
        var option = id.HasValue ? await database.SaleOptions.FindAsync(id) : new SaleOption();
        if (option == null) return NotFound();
        option.Name = name.Trim(); option.Kind = kind; option.ExtraPrice = extraPrice; option.IsActive = isActive;
        if (!id.HasValue) database.SaleOptions.Add(option);
        await database.SaveChangesAsync();
        TempData["Status"] = "Đã lưu lựa chọn đồ uống.";
        return RedirectToAction(nameof(Index));
    }
}
