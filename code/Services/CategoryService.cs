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

    public async Task<SaveResult> CreateAsync(
        DrinkCategoryEditViewModel model,
        ModelStateDictionary modelState
    )
    {
        var category = new Category();
        if (!await ApplyChangesAsync(category, model, modelState))
            return SaveResult.Invalid;
        database.DrinkCategories.Add(category);
        await database.SaveChangesAsync();
        return SaveResult.Success;
    }

    public async Task<SaveResult> UpdateAsync(
        Guid id,
        DrinkCategoryEditViewModel model,
        ModelStateDictionary modelState
    )
    {
        var category = await database.DrinkCategories.SingleOrDefaultAsync(category => category.Id == id);
        if (category == null)
            return SaveResult.NotFound;
        if (!await ApplyChangesAsync(category, model, modelState))
            return SaveResult.Invalid;
        database.Entry(category).Property(category => category.Revision).OriginalValue = model.Revision;
        category.Revision++;
        await database.SaveChangesAsync();
        return SaveResult.Success;
    }

    private async Task<bool> ApplyChangesAsync(
        Category category,
        DrinkCategoryEditViewModel model,
        ModelStateDictionary modelState
    )
    {
        model.CurrentImage = category.Image;
        model.CurrentIcon = category.Icon;
        var image = await uploads.ReadAsync(
            model.ImageUpload,
            modelState,
            nameof(model.ImageUpload)
        );
        var icon = await uploads.ReadAsync(model.IconUpload, modelState, nameof(model.IconUpload));
        if (image == null && string.IsNullOrEmpty(category.Image))
            modelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh chung cho danh mục.");
        if (
            model.ShowOnHome
            && icon == null
            && (model.RemoveIcon || string.IsNullOrEmpty(category.Icon))
        )
            modelState.AddModelError(nameof(model.IconUpload), "Chọn biểu tượng thẻ trang chủ.");
        if (!modelState.IsValid)
            return false;
        async Task<string> SaveImage(ImageUpload? file, string current)
        {
            if (file == null)
                return current;
            return await uploads.SaveAsync(file, "categories");
        }
        category.Image = await SaveImage(image, category.Image);
        category.Icon = await SaveImage(icon, model.RemoveIcon ? "" : category.Icon);
        category.Name = model.Name.Trim();
        category.SortOrder = model.SortOrder;
        category.ShowOnHome = model.ShowOnHome;
        category.IsPublished = model.IsPublished;
        return true;
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
