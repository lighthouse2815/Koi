using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class DrinkAdminService(ApplicationDbContext database, ImageUploads uploads, ILogger<DrinkAdminService> logger)
{
    public async Task<AdminDrinksViewModel> GetListAsync(string? search, Guid categoryId = default, int page = 1)
    {
        search = search?.Trim(); if (search?.Length > 200) search = search[..200];
        var query = database.Drinks.AsNoTracking().Where(value => categoryId == Guid.Empty || value.CategoryId == categoryId);
        if (!string.IsNullOrEmpty(search)) query = query.Where(value => (EF.Functions.ILike(value.Name, $"%{search}%")));
        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 20d)); page = Math.Clamp(page, 1, pages);
        return new AdminDrinksViewModel(await query.Include(value => value.Category).OrderBy(value => value.Category.SortOrder)
            .ThenBy(value => value.CategoryId).ThenBy(value => value.SortOrder).ThenBy(value => value.Id).Skip((page - 1) * 20).Take(20).ToArrayAsync(),
            await Categories(), categoryId, search, page, pages);
    }

    public async Task<DrinkEditViewModel> CreateAsync() => new() { Categories = await Categories() };

    public async Task<DrinkEditViewModel?> GetEditAsync(Guid id)
    {
        var value = await database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DrinkEditViewModel { Id = id, Revision = value.Revision, Name = value.Name,
            CategoryId = value.CategoryId, Description = value.Description, SortOrder = value.SortOrder,
            IsPublished = value.IsPublished, CurrentImage = value.Image, Categories = await Categories() };
    }

    public async Task<SaveResult> SaveAsync(DrinkEditViewModel model, Guid? id, ModelStateDictionary modelState)
    {
        var value = id.HasValue ? await database.Drinks.SingleOrDefaultAsync(value => value.Id == id) : new Drink();
        if (value == null) return SaveResult.NotFound; model.CurrentImage = value.Image;
        if (!await database.DrinkCategories.AnyAsync(value => value.Id == model.CategoryId)) modelState.AddModelError(nameof(model.CategoryId), "Chọn danh mục hợp lệ.");
        var image = await uploads.ReadAsync(model.ImageUpload, modelState, nameof(model.ImageUpload));
        if (!modelState.IsValid) { model.Categories = await Categories(); return SaveResult.Invalid; }
        string? uploaded = null;
        var result = SaveResult.Conflict;
        try
        {
            if (image != null) value.Image = uploaded = await uploads.SaveAsync(image, "drinks");
            else if (model.RemoveImage) value.Image = "";
            value.Name = model.Name.Trim();
            value.CategoryId = model.CategoryId;
            value.Description = model.Description?.Trim() ?? ""; value.SortOrder = model.SortOrder; value.IsPublished = model.IsPublished;
            if (id == null)
            {
                var entry = database.Drinks.Add(value);
                entry.Property("NameVi").CurrentValue = "";
                entry.Property("Region").CurrentValue = "";
                entry.Property("RegionVi").CurrentValue = "";
                entry.Property("DescriptionVi").CurrentValue = "";
            }
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync(); return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException) { modelState.AddModelError("", "Đồ uống đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save drink {Id}", id); result = SaveResult.Unavailable;
            modelState.AddModelError("", "Chưa lưu được đồ uống. Vui lòng thử lại.");
        }
        uploads.Rollback(uploaded); model.Categories = await Categories(); return result;
    }

    public async Task<DeleteContentViewModel?> GetDeleteAsync(Guid id)
    {
        var value = await database.Drinks.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DeleteContentViewModel(id, value.Name);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var value = await database.Drinks.FindAsync(id);
        if (value == null) return false;
        database.Drinks.Remove(value);
        await database.SaveChangesAsync();
        return true;
    }

    private async Task<SelectList> Categories() => new(await database.DrinkCategories.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync(), "Id", "Name");
}
