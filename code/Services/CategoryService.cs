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

    public async Task CreateAsync(
        CategoryEditViewModel model,
        ModelStateDictionary modelState)
    {
        var category = new Category();
        await ApplyChangesAsync(category, model, modelState);
        database.Categories.Add(category);
        await database.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        CategoryEditViewModel model,
        ModelStateDictionary modelState)
    {
        var category = await database.Categories.FindAsync(model.Id)
            ?? throw new AppException(ErrorCode.CATEGORY_NOT_FOUND);
        await ApplyChangesAsync(category, model, modelState);
        database.Entry(category).Property(category => category.Revision).OriginalValue = model.Revision;
        category.Revision++;
        await database.SaveChangesAsync();
    }

    private async Task ApplyChangesAsync(
        Category category,
        CategoryEditViewModel model,
        ModelStateDictionary modelState)
    {
        model.CurrentImage = category.Image;
        model.CurrentIcon = category.Icon;
        if (!string.IsNullOrWhiteSpace(model.Name) && await database.Categories.AnyAsync(value =>
            value.Id != category.Id && value.Name.Trim().ToLower() == model.Name.Trim().ToLower()))
            throw new AppException(ErrorCode.CATEGORY_NAME_EXISTS);

        await ApplyImagesAsync(category, model, modelState);

        category.Name = model.Name.Trim();
        category.SortOrder = model.SortOrder;
        category.ShowOnHome = model.ShowOnHome;
        category.IsPublished = model.IsPublished;
    }

    private async Task ApplyImagesAsync(
        Category category,
        CategoryEditViewModel model,
        ModelStateDictionary modelState)
    {
        var image = await uploads.GetImageUrlAsync(model.ImageUploadId, modelState, nameof(model.ImageUploadId), "categories");
        var icon = await uploads.GetImageUrlAsync(model.IconUploadId, modelState, nameof(model.IconUploadId), "categories");
        if (image == null && string.IsNullOrEmpty(category.Image))
            modelState.AddModelError(nameof(model.ImageUploadId), "Chọn ảnh chung cho danh mục.");
        if (
            model.ShowOnHome
            && icon == null
            && (model.RemoveIcon || string.IsNullOrEmpty(category.Icon))
        )
            modelState.AddModelError(nameof(model.IconUploadId), "Chọn biểu tượng thẻ trang chủ.");
        if (!modelState.IsValid)
        {
            var message = modelState.Values.SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
                ?? ErrorCode.CATEGORY_INVALID.Message;
            throw new AppException(ErrorCode.CATEGORY_INVALID with { Message = message });
        }

        if (image != null)
            category.Image = image;
        if (icon != null)
            category.Icon = icon;
        else if (model.RemoveIcon)
            category.Icon = "";
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
