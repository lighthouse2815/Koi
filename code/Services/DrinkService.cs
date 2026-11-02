using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class DrinkService(
    ApplicationDbContext database,
    ImageUploadService uploads
)
{
    public async Task<AdminDrinksViewModel> GetListAsync(
        string? search,
        Guid categoryId = default,
        int page = 1
    )
    {
        search = search?.Trim();

        if (search?.Length > 200)
        {
            search = search[..200];
        }

        var query = database
            .Drinks.AsNoTracking()
            .Where(value => categoryId == Guid.Empty || value.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(value => EF.Functions.ILike(value.Name, $"%{search}%"));
        }

        var pages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 20d));

        page = Math.Clamp(page, 1, pages);

        return new AdminDrinksViewModel(
            await query
                .Include(value => value.Category)
                .OrderBy(value => value.Category.SortOrder)
                .ThenBy(value => value.CategoryId)
                .ThenBy(value => value.SortOrder)
                .ThenBy(value => value.Id)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToArrayAsync(),
            await Categories(),
            categoryId,
            search,
            page,
            pages
        );
    }

    public async Task<DrinkEditViewModel> CreateAsync() =>
        new() { Categories = await Categories() };

    public async Task<DrinkEditViewModel?> GetEditAsync(Guid id)
    {
        var value = await database
            .Drinks.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null
            ? null
            : new DrinkEditViewModel
            {
                Id = id,
                Revision = value.Revision,

                Name = value.Name,
                CategoryId = value.CategoryId,
                Description = value.Description,
                SortOrder = value.SortOrder,
                IsPublished = value.IsPublished,
                CurrentImage = value.Image,
                Categories = await Categories(),
            };
    }

    public Task<SaveResult> CreateAsync(
        DrinkEditViewModel model,
        ModelStateDictionary modelState) => SaveCoreAsync(model, null, modelState);

    public Task<SaveResult> UpdateAsync(
        Guid id,
        DrinkEditViewModel model,
        ModelStateDictionary modelState) => SaveCoreAsync(model, id, modelState);

    private async Task<SaveResult> SaveCoreAsync(
        DrinkEditViewModel model,
        Guid? id,
        ModelStateDictionary modelState
    )
    {
        var value = id.HasValue
            ? await database.Drinks.SingleOrDefaultAsync(value => value.Id == id)
            : new Drink();
        if (value == null)
            return SaveResult.NotFound;
        model.CurrentImage = value.Image;
        if (!await database.Categories.AnyAsync(value => value.Id == model.CategoryId))
            modelState.AddModelError(nameof(model.CategoryId), "Chọn danh mục hợp lệ.");
        var image = await uploads.ReadAsync(
            model.ImageUpload,
            modelState,
            nameof(model.ImageUpload)
        );
        if (!modelState.IsValid)
        {
            model.Categories = await Categories();
            return SaveResult.Invalid;
        }
        if (image != null)
            value.Image = await uploads.SaveAsync(image, "drinks");
        else if (model.RemoveImage)
            value.Image = "";
        value.Name = model.Name.Trim();

        value.CategoryId = model.CategoryId;
        value.Description = model.Description?.Trim() ?? "";
        value.SortOrder = model.SortOrder;
        value.IsPublished = model.IsPublished;
        if (id == null)
            database.Drinks.Add(value);
        else
        {
            database.Entry(value).Property(value => value.Revision).OriginalValue = model.Revision;
            value.Revision++;
        }
        await database.SaveChangesAsync();
        return SaveResult.Success;
    }

    public async Task<DeleteContentViewModel?> GetDeleteAsync(Guid id)
    {
        var value = await database
            .Drinks.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DeleteContentViewModel(id, value.Name);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var value = await database.Drinks.FindAsync(id);
        if (value == null)
            return false;
        database.Drinks.Remove(value);
        await database.SaveChangesAsync();
        return true;
    }

    private async Task<SelectList> Categories() =>
        new(
            await database
                .Categories.AsNoTracking()
                .OrderBy(value => value.SortOrder)
                .ThenBy(value => value.Id)
                .ToArrayAsync(),
            "Id",
            "Name"
        );
}
