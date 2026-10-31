using System.ComponentModel.DataAnnotations;
using Koi.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Controllers.Admin;

[Area("Admin"), Authorize(Roles = "Admin")]
public class SalesController(ApplicationDbContext database) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        search = search?.Trim();
        if (search?.Length > 100) search = search[..100];
        var shop = await database.ShopSettings.AsNoTracking().SingleAsync(value => value.Id == 1);
        var brandId = shop.BrandId;
        var branches = database.Stores.AsNoTracking().Where(value => value.IsPublished && (brandId == null || value.BrandId == brandId) && (shop.MarketId == null || value.MarketId == shop.MarketId));
        if (!string.IsNullOrEmpty(search))
            branches = branches.Where(value => EF.Functions.ILike(value.Name, $"%{search}%") || EF.Functions.ILike(value.Address, $"%{search}%"));
        var pageCount = Math.Max(1, (int)Math.Ceiling(await branches.CountAsync() / 20d));
        page = Math.Clamp(page, 1, pageCount);
        return View(new SalesSettingsViewModel
        {
            Drinks = await database.Drinks.AsNoTracking().Include(value => value.Category).OrderBy(value => value.Category.SortOrder).ThenBy(value => value.SortOrder).ToArrayAsync(),
            Branches = await branches.Include(value => value.Market).Include(value => value.Brand).Include(value => value.City).OrderBy(value => value.Name).ThenBy(value => value.Id).Skip((page - 1) * 20).Take(20).ToArrayAsync(),
            Brands = await database.TeaBrands.AsNoTracking().OrderBy(value => value.Name).ToArrayAsync(), BrandId = brandId,
            Search = search, Page = page, PageCount = pageCount
        });
    }

    [HttpPost]
    public async Task<IActionResult> Brand(Guid brandId)
    {
        if (!await database.TeaBrands.AnyAsync(value => value.Id == brandId)) return BadRequest("Thương hiệu không hợp lệ.");
        var settings = await database.ShopSettings.SingleAsync(value => value.Id == 1);
        settings.BrandId = brandId;
        await database.SaveChangesAsync();
        TempData["Status"] = "Đã chọn thương hiệu. Chỉ chi nhánh thuộc thương hiệu này nhận đơn; cấu hình chi nhánh cũ vẫn được giữ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Price(Guid id, [Range(1, 999999999)] decimal? price, int revision)
    {
        if (!ModelState.IsValid) { TempData["Status"] = "Giá phải là số nguyên từ 1 đến 999.999.999 đồng, hoặc để trống để ngừng bán."; return RedirectToAction(nameof(Index)); }
        if (price.HasValue && decimal.Truncate(price.Value) != price.Value) { TempData["Status"] = "Giá phải là số nguyên đồng."; return RedirectToAction(nameof(Index)); }
        var drink = await database.Drinks.FindAsync(id);
        if (drink == null) return NotFound();
        if (drink.Revision != revision) { TempData["Status"] = "Đồ uống đã thay đổi. Hãy tải lại và thử lại."; return RedirectToAction(nameof(Index)); }
        database.Entry(drink).Property(value => value.Revision).OriginalValue = revision;
        drink.Price = price;
        drink.Revision++;
        try { await database.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { TempData["Status"] = "Đồ uống đã thay đổi. Hãy tải lại và thử lại."; return RedirectToAction(nameof(Index)); }
        TempData["Status"] = "Đã cập nhật giá bán.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Branch(Guid id, bool acceptsOrders, int revision, string? search, int page = 1)
    {
        var branch = await database.Stores.FindAsync(id);
        if (branch == null) return NotFound();
        var shop = await database.ShopSettings.AsNoTracking().SingleAsync(value => value.Id == 1);
        var brandId = shop.BrandId;
        if (acceptsOrders && (!brandId.HasValue || !shop.MarketId.HasValue)) { TempData["Status"] = "Hãy chọn thương hiệu trước khi mở nhận đơn."; return RedirectToAction(nameof(Index), new { search, page }); }
        if (acceptsOrders && (branch.BrandId != brandId || branch.MarketId != shop.MarketId)) return BadRequest("Chi nhánh không thuộc chuỗi tại Việt Nam.");
        if (branch.Revision != revision) { TempData["Status"] = "Chi nhánh đã thay đổi. Hãy tải lại và thử lại."; return RedirectToAction(nameof(Index), new { search, page }); }
        database.Entry(branch).Property(value => value.Revision).OriginalValue = revision;
        branch.AcceptsOrders = acceptsOrders;
        branch.Revision++;
        try { await database.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { TempData["Status"] = "Chi nhánh đã thay đổi. Hãy tải lại và thử lại."; return RedirectToAction(nameof(Index), new { search, page }); }
        TempData["Status"] = "Đã cập nhật trạng thái nhận đơn của chi nhánh.";
        return RedirectToAction(nameof(Index), new { search, page });
    }
}

public class SalesSettingsViewModel
{
    public Koi.Web.Models.Drink[] Drinks { get; set; } = [];
    public Koi.Web.Models.StoreEntry[] Branches { get; set; } = [];
    public Koi.Web.Models.TeaBrand[] Brands { get; set; } = [];
    public Guid? BrandId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; }
    public int PageCount { get; set; }
}
