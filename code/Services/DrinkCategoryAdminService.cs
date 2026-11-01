using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class DrinkCategoryAdminService(ApplicationDbContext database, ImageUploads uploads, ILogger<DrinkCategoryAdminService> logger)
{
    public Task<Category[]> GetListAsync() => database.DrinkCategories.AsNoTracking().OrderBy(value => value.SortOrder).ThenBy(value => value.Id).ToArrayAsync();

    public async Task<DrinkCategoryEditViewModel?> GetEditAsync(Guid id)
    {
        var value = await database.DrinkCategories.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DrinkCategoryEditViewModel { Id = id, Revision = value.Revision, Name = value.Name,
            IsPublished = value.IsPublished, ShowOnHome = value.ShowOnHome, SortOrder = value.SortOrder,
            CurrentImage = value.Image, CurrentIcon = value.Icon };
    }

    public async Task<SaveResult> SaveAsync(DrinkCategoryEditViewModel model, Guid? id, ModelStateDictionary modelState)
    {
        var value = id.HasValue ? await database.DrinkCategories.SingleOrDefaultAsync(value => value.Id == id) : new Category();
        if (value == null) return SaveResult.NotFound;
        model.CurrentImage = value.Image; model.CurrentIcon = value.Icon;
        var image = await uploads.ReadAsync(model.ImageUpload, modelState, nameof(model.ImageUpload));
        var icon = await uploads.ReadAsync(model.IconUpload, modelState, nameof(model.IconUpload));
        if (image == null && string.IsNullOrEmpty(value.Image)) modelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh chung cho danh mục.");
        if (model.ShowOnHome && icon == null && (model.RemoveIcon || string.IsNullOrEmpty(value.Icon))) modelState.AddModelError(nameof(model.IconUpload), "Chọn biểu tượng thẻ trang chủ.");
        if (!modelState.IsValid) { return SaveResult.Invalid; }
        var uploaded = new List<string>();
        async Task<string> SaveImage(ImageUpload? file, string current)
        {
            if (file == null) return current;
            var path = await uploads.SaveAsync(file, "categories"); uploaded.Add(path); return path;
        }
        var result = SaveResult.Conflict;
        try
        {
            value.Image = await SaveImage(image, value.Image); value.Icon = await SaveImage(icon, model.RemoveIcon ? "" : value.Icon);
            value.Name = model.Name.Trim();
            value.SortOrder = model.SortOrder;
            value.ShowOnHome = model.ShowOnHome; value.IsPublished = model.IsPublished;
            if (id == null)
                database.DrinkCategories.Add(value);
            else { database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision; value.Revision++; }
            await database.SaveChangesAsync(); return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException) { modelState.AddModelError("", "Danh mục đã thay đổi. Hãy tải lại trang trước khi lưu."); }
        catch (Exception exception) when (exception is DbUpdateException or IOException)
        {
            logger.LogError(exception, "Could not save drink category {Id}", id); result = SaveResult.Unavailable;
            modelState.AddModelError("", "Chưa lưu được danh mục. Vui lòng thử lại.");
        }
        uploaded.ForEach(uploads.Rollback); return result;
    }

    public async Task<DeleteContentViewModel?> GetDeleteAsync(Guid id)
    {
        var value = await database.DrinkCategories.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DeleteContentViewModel(id, value.Name);
    }

    public async Task<SaveResult> DeleteAsync(Guid id, ModelStateDictionary modelState)
    {
        var value = await database.DrinkCategories.FindAsync(id);
        if (value == null) return SaveResult.NotFound;
        try { database.DrinkCategories.Remove(value); await database.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            modelState.AddModelError("", "Danh mục đang có đồ uống. Hãy chuyển hoặc xóa đồ uống trước, hoặc tắt hiển thị danh mục.");
            return SaveResult.Conflict;
        }
        return SaveResult.Success;
    }
}
