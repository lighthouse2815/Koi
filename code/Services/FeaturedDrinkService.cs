using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class FeaturedDrinkService(
    ApplicationDbContext database,
    ImageUploadService uploads
)
{
    public Task<FeaturedDrink[]> GetListAsync() =>
        database
            .FeaturedDrinks.AsNoTracking()
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArrayAsync();

    public async Task<FeaturedDrinkEditViewModel?> GetEditAsync(Guid id)
    {
        var value = await database
            .FeaturedDrinks.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null
            ? null
            : new FeaturedDrinkEditViewModel
            {
                Id = id,
                Revision = value.Revision,

                Name = value.Name,
                IsPublished = value.IsPublished,
                SortOrder = value.SortOrder,
                CurrentImage = value.Image,
                CurrentBackground = value.BackgroundImage,
            };
    }

    public async Task<SaveResult> SaveAsync(
        FeaturedDrinkEditViewModel model,
        Guid? id,
        ModelStateDictionary modelState
    )
    {
        var value = id.HasValue
            ? await database.FeaturedDrinks.SingleOrDefaultAsync(value => value.Id == id)
            : new FeaturedDrink();
        if (value == null)
            return SaveResult.NotFound;
        model.CurrentImage = value.Image;
        model.CurrentBackground = value.BackgroundImage;
        var image = await uploads.ReadAsync(
            model.ImageUpload,
            modelState,
            nameof(model.ImageUpload)
        );
        var background = await uploads.ReadAsync(
            model.BackgroundUpload,
            modelState,
            nameof(model.BackgroundUpload)
        );
        if (image == null && string.IsNullOrEmpty(value.Image))
            modelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh đồ uống.");
        if (background == null && string.IsNullOrEmpty(value.BackgroundImage))
            modelState.AddModelError(nameof(model.BackgroundUpload), "Chọn ảnh nền.");
        if (!modelState.IsValid)
        {
            return SaveResult.Invalid;
        }
        if (image != null)
            value.Image = await uploads.SaveAsync(image, "featured-drinks");
        if (background != null)
            value.BackgroundImage = await uploads.SaveAsync(background, "featured-drinks");
        value.Name = model.Name.Trim();

        value.SortOrder = model.SortOrder;
        value.IsPublished = model.IsPublished;
        if (id == null)
            database.FeaturedDrinks.Add(value);
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
            .FeaturedDrinks.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DeleteContentViewModel(id, value.Name);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var value = await database.FeaturedDrinks.FindAsync(id);
        if (value == null)
            return false;
        database.FeaturedDrinks.Remove(value);
        await database.SaveChangesAsync();
        return true;
    }
}
