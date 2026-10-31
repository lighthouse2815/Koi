using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class GeographyAdminService(ApplicationDbContext database, ImageUploads uploads, ILogger<GeographyAdminService> logger)
{
    public async Task<GeographyIndexViewModel?> GetListAsync(string kind = "countries")
    {
        if (!ValidKind(kind)) return null;
        var rows = kind switch
        {
            "countries" => await database.Markets.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id)
                .Select(value => new GeographyRow(value.Id, value.Name, "", value.SortOrder)).ToArrayAsync(),
            "cities" => await database.StoreCities.AsNoTracking().OrderBy(value => value.Market.SortOrder).ThenBy(value => value.SortOrder).ThenBy(value => value.Id)
                .Select(value => new GeographyRow(value.Id, value.Name, value.Market.Name, value.SortOrder)).ToArrayAsync(),
            _ => await database.TeaBrands.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id)
                .Select(value => new GeographyRow(value.Id, value.Name, "", value.SortOrder)).ToArrayAsync()
        };
        return new GeographyIndexViewModel(kind, Title(kind), rows);
    }

    public async Task<GeographyEditViewModel?> CreateAsync(string kind) =>
        !ValidKind(kind) ? null : await Options(new GeographyEditViewModel { Kind = kind });

    public async Task<GeographyEditViewModel?> GetEditAsync(int id, string kind = "countries")
    {
        if (!ValidKind(kind)) return null;
        GeographyEditViewModel? model = null;
        if (kind == "countries")
        {
            var value = await database.Markets.AsNoTracking().Include(value => value.Brands).Include(value => value.SocialLinks).SingleOrDefaultAsync(value => value.Id == id);
            if (value != null) model = new() { Id = id, Revision = value.Revision, NameVi = value.NameVi, Name = value.Name, SortOrder = value.SortOrder,
                BrandIds = value.Brands.Select(value => value.BrandId).ToArray(), SocialLinks = string.Join("\n", value.SocialLinks.OrderBy(value => value.SortOrder).Select(value => $"{value.Label} | {value.Url}")) };
        }
        else if (kind == "cities")
        {
            var value = await database.StoreCities.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
            if (value != null) model = new() { Id = id, Revision = value.Revision, NameVi = value.NameVi, Name = value.Name, SortOrder = value.SortOrder, MarketId = value.MarketId };
        }
        else
        {
            var value = await database.TeaBrands.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
            if (value != null) model = new() { Id = id, Revision = value.Revision, NameVi = value.NameVi, Name = value.Name, SortOrder = value.SortOrder, CurrentImage = value.Logo };
        }
        if (model == null) return null;
        model.Kind = kind; return await Options(model);
    }

    public async Task<SaveResult> SaveAsync(GeographyEditViewModel model, int? id, ModelStateDictionary modelState)
    {
        if (!ValidKind(model.Kind)) return SaveResult.BadRequest;
        var country = model.Kind == "countries" ? (id == null ? new Market() : await database.Markets.Include(value => value.Brands).Include(value => value.SocialLinks).SingleOrDefaultAsync(value => value.Id == id)) : null;
        var city = model.Kind == "cities" ? (id == null ? new StoreCity() : await database.StoreCities.SingleOrDefaultAsync(value => value.Id == id)) : null;
        var brand = model.Kind == "brands" ? (id == null ? new TeaBrand() : await database.TeaBrands.SingleOrDefaultAsync(value => value.Id == id)) : null;
        if (country == null && city == null && brand == null) return SaveResult.NotFound;
        model.CurrentImage = brand?.Logo;
        var links = new List<MarketSocialLink>();
        if (country != null)
        {
            if (model.NameVi?.Length > 100) modelState.AddModelError(nameof(model.NameVi), "Tên quốc gia tiếng Việt tối đa 100 ký tự.");
            if (model.Name?.Length > 100) modelState.AddModelError(nameof(model.Name), "Tên quốc gia tối đa 100 ký tự.");
            var ids = model.BrandIds.Distinct().ToArray();
            if (await database.TeaBrands.CountAsync(value => ids.Contains(value.Id)) != ids.Length)
                modelState.AddModelError(nameof(model.BrandIds), "Có thương hiệu không hợp lệ.");
            if (id != null && await database.Stores.AnyAsync(value => value.MarketId == id && !ids.Contains(value.BrandId)))
                modelState.AddModelError(nameof(model.BrandIds), "Không thể bỏ thương hiệu đang được cửa hàng sử dụng.");
            foreach (var line in (model.SocialLinks ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var pair = line.Split('|', 2, StringSplitOptions.TrimEntries);
                if (pair.Length != 2 || string.IsNullOrWhiteSpace(pair[0]) || pair[0].Length > 100 || pair[1].Length > 1000 || string.IsNullOrWhiteSpace(pair[1]) || !StoreAdminService.SafeLink(pair[1]))
                    modelState.AddModelError(nameof(model.SocialLinks), "Mỗi dòng cần có dạng: Tên liên kết | https://dia-chi. Tên tối đa 100 ký tự, địa chỉ tối đa 1000 ký tự.");
                else links.Add(new MarketSocialLink { Label = pair[0], Url = pair[1], SortOrder = links.Count });
            }
            if (links.Count > 50) modelState.AddModelError(nameof(model.SocialLinks), "Tối đa 50 liên kết.");
        }
        if (city != null)
        {
            if (!await database.Markets.AnyAsync(value => value.Id == model.MarketId)) modelState.AddModelError(nameof(model.MarketId), "Chọn quốc gia hợp lệ.");
            if (id != null && city.MarketId != model.MarketId && await database.Stores.AnyAsync(value => value.CityId == id))
                modelState.AddModelError(nameof(model.MarketId), "Khu vực đang có cửa hàng; hãy cập nhật các cửa hàng trước khi đổi quốc gia.");
        }
        var image = brand != null ? await uploads.ReadAsync(model.ImageUpload, modelState, nameof(model.ImageUpload)) : null;
        if (!modelState.IsValid) { await Options(model); return SaveResult.Invalid; }
        string? uploaded = null;
        var result = SaveResult.Conflict;
        try
        {
            if (country != null)
            {
                country.Name = (model.Name ?? "").Trim(); country.NameVi = model.NameVi?.Trim() ?? ""; country.SortOrder = model.SortOrder;
                database.RemoveRange(country.Brands.Where(value => !model.BrandIds.Contains(value.BrandId)));
                foreach (var brandId in model.BrandIds.Distinct().Except(country.Brands.Select(value => value.BrandId))) country.Brands.Add(new MarketBrand { BrandId = brandId });
                database.RemoveRange(country.SocialLinks); country.SocialLinks = links;
                if (id == null) database.Markets.Add(country);
                else { database.Entry(country).Property(value => value.Revision).OriginalValue = model.Revision; country.Revision++; }
            }
            if (city != null)
            {
                city.Name = (model.Name ?? "").Trim(); city.NameVi = model.NameVi?.Trim() ?? ""; city.SortOrder = model.SortOrder; city.MarketId = model.MarketId;
                if (id == null) database.StoreCities.Add(city);
                else { database.Entry(city).Property(value => value.Revision).OriginalValue = model.Revision; city.Revision++; }
            }
            if (brand != null)
            {
                brand.Name = (model.Name ?? "").Trim(); brand.NameVi = model.NameVi?.Trim() ?? ""; brand.SortOrder = model.SortOrder;
                if (image != null) brand.Logo = uploaded = await uploads.SaveAsync(image, "brands");
                else if (model.RemoveImage) brand.Logo = "";
                if (id == null) database.TeaBrands.Add(brand);
                else { database.Entry(brand).Property(value => value.Revision).OriginalValue = model.Revision; brand.Revision++; }
            }
            await database.SaveChangesAsync(); return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException) { modelState.AddModelError("", "Dữ liệu đã thay đổi. Hãy tải lại trang sửa trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save geography {Kind} {Id}", model.Kind, id);
            result = SaveResult.Unavailable; modelState.AddModelError("", "Chưa lưu được dữ liệu. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded); await Options(model); return result;
    }

    public async Task<DeleteContentViewModel?> GetDeleteAsync(int id, string kind)
    {
        var value = await Find(id, kind);
        return value == null ? null : new DeleteContentViewModel(id, Name(value), kind);
    }

    public async Task<SaveResult> DeleteAsync(int id, string kind, ModelStateDictionary modelState)
    {
        var value = await Find(id, kind);
        if (value == null) return SaveResult.NotFound;
        try { database.Remove(value); await database.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            modelState.AddModelError("", "Dữ liệu đang được sử dụng. Hãy chuyển hoặc xóa các mục liên quan trước.");
            return SaveResult.Conflict;
        }
        return SaveResult.Success;
    }

    private async Task<object?> Find(int id, string kind) => kind switch
    {
        "countries" => await database.Markets.FindAsync(id), "cities" => await database.StoreCities.FindAsync(id),
        "brands" => await database.TeaBrands.FindAsync(id), _ => null
    };
    private static string Name(object value) => value switch { Market country => country.Name, StoreCity city => city.Name, TeaBrand brand => brand.Name, _ => "" };
    private static bool ValidKind(string kind) => kind is "countries" or "cities" or "brands";
    public static string Title(string kind) => kind switch { "countries" => "Quốc gia", "cities" => "Khu vực", _ => "Thương hiệu" };
    private async Task<GeographyEditViewModel> Options(GeographyEditViewModel model)
    {
        model.Markets = new SelectList(await database.Markets.AsNoTracking().OrderBy(value => value.SortOrder).ToArrayAsync(), "Id", "Name");
        model.Brands = await database.TeaBrands.AsNoTracking().OrderBy(value => value.SortOrder).ToArrayAsync(); return model;
    }
}
