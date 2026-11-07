using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Koi.Web.Services;

public class CategoryService(
    ApplicationDbContext database,
    ImageUploadService uploads
)
{
    public Task<Category[]> GetListAsync() =>
        database.Categories
                .AsNoTracking()
                .OrderBy(value => value.SortOrder)
                .ThenBy(value => value.Id)
                .ToArrayAsync();

   
    public async Task<Category> GetByIdAsync(Guid id) =>
        await database.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(category => category.Id == id)
        ?? throw new AppException(ErrorCode.CATEGORY_NOT_FOUND);

    public async Task<SaveResult> CreateAsync(
        CategoryEditViewModel model,
        ModelStateDictionary modelState)
    {
        var category = new Category();
        if (!await ApplyChangesAsync(category, model, modelState))
            return SaveResult.Invalid;
        database.Categories.Add(category);
        await database.SaveChangesAsync();
        return SaveResult.Success;
    }

    public async Task<SaveResult> UpdateAsync(
        CategoryEditViewModel model,
        ModelStateDictionary modelState)
    {
        var category = await database.Categories
            .SingleOrDefaultAsync(category => category.Id == model.Id)
            ?? throw new AppException(ErrorCode.CATEGORY_NOT_FOUND);
        if (!await ApplyChangesAsync(category, model, modelState))
            return SaveResult.Invalid;
        database.Entry(category).Property(category => category.Revision).OriginalValue = model.Revision;
        category.Revision++;
        await database.SaveChangesAsync();
        return SaveResult.Success;
    }

    private async Task<bool> ApplyChangesAsync(
        Category category,
        CategoryEditViewModel model,
        ModelStateDictionary modelState)
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
        {
            return false;
        }
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

    public async Task DeleteAsync(Guid id)
    {
        var category = await database.Categories.FindAsync(id)
            ?? throw new AppException(ErrorCode.CATEGORY_NOT_FOUND);

        database.Categories.Remove(category);
        try
        {
            await database.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException(ErrorCode.CATEGORY_CHANGED);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: "FK_Drinks_Category_CategoryId"
            })
        {
            throw new AppException(ErrorCode.CATEGORY_HAS_DRINKS);
        }
    }
}
