using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class CategoryService(
    ApplicationDbContext database,
    ImageUploadService uploads
)
{
    public Task<Category[]> GetListAsync() =>
        database
            .DrinkCategories.AsNoTracking()
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArrayAsync();

    public Task<Category?> GetByIdAsync(Guid id) =>
        database.DrinkCategories.AsNoTracking().SingleOrDefaultAsync(category => category.Id == id);

    public async Task<SaveResult> SaveAsync(
        DrinkCategoryEditViewModel model,
        Guid? id,
        ModelStateDictionary modelState
    )
    {
        var value = id.HasValue
            ? await database.DrinkCategories.SingleOrDefaultAsync(value => value.Id == id)
            : new Category();
        if (value == null)
            return SaveResult.NotFound;
        model.CurrentImage = value.Image;
        model.CurrentIcon = value.Icon;
        var image = await uploads.ReadAsync(
            model.ImageUpload,
            modelState,
            nameof(model.ImageUpload)
        );
        var icon = await uploads.ReadAsync(model.IconUpload, modelState, nameof(model.IconUpload));
        if (image == null && string.IsNullOrEmpty(value.Image))
            modelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh chung cho danh mục.");
        if (
            model.ShowOnHome
            && icon == null
            && (model.RemoveIcon || string.IsNullOrEmpty(value.Icon))
        )
            modelState.AddModelError(nameof(model.IconUpload), "Chọn biểu tượng thẻ trang chủ.");
        if (!modelState.IsValid)
        {
            return SaveResult.Invalid;
        }
        async Task<string> SaveImage(ImageUpload? file, string current)
        {
            if (file == null)
                return current;
            return await uploads.SaveAsync(file, "categories");
        }
        value.Image = await SaveImage(image, value.Image);
        value.Icon = await SaveImage(icon, model.RemoveIcon ? "" : value.Icon);
        value.Name = model.Name.Trim();
        value.SortOrder = model.SortOrder;
        value.ShowOnHome = model.ShowOnHome;
        value.IsPublished = model.IsPublished;
        if (id == null)
            database.DrinkCategories.Add(value);
        else
        {
            database.Entry(value).Property(value => value.Revision).OriginalValue =
                model.Revision;
            value.Revision++;
        }
        await database.SaveChangesAsync();
        return SaveResult.Success;
    }

    public async Task<SaveResult> DeleteAsync(Guid id)
    {
        var value = await database.DrinkCategories.FindAsync(id);
        if (value == null)
            return SaveResult.NotFound;
        database.DrinkCategories.Remove(value);
        await database.SaveChangesAsync();
        return SaveResult.Success;
    }
}
