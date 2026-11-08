using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
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

    public async Task CreateAsync(FeaturedDrinkEditViewModel model)
    {
        var drink = new FeaturedDrink();
        await ApplyChangesAsync(drink, model);
        database.FeaturedDrinks.Add(drink);
        await database.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, FeaturedDrinkEditViewModel model)
    {
        var drink = await database.FeaturedDrinks.FindAsync(id)
            ?? throw new AppException(ErrorCode.FEATURED_DRINK_NOT_FOUND);

        await ApplyChangesAsync(drink, model);
        database.Entry(drink).Property(value => value.Revision).OriginalValue = model.Revision;
        drink.Revision++;
        await database.SaveChangesAsync();
    }

    private async Task ApplyChangesAsync(FeaturedDrink drink, FeaturedDrinkEditViewModel model)
    {
        var image = await uploads.GetImageUrlAsync(model.ImageUploadId, "featured-drinks");
        var background = await uploads.GetImageUrlAsync(model.BackgroundUploadId, "featured-drinks");

        if (image == null && string.IsNullOrEmpty(drink.Image))
            throw new AppException(ErrorCode.FEATURED_DRINK_IMAGE_REQUIRED);
        if (background == null && string.IsNullOrEmpty(drink.BackgroundImage))
            throw new AppException(ErrorCode.FEATURED_DRINK_BACKGROUND_REQUIRED);

        if (image != null)
            drink.Image = image;
        if (background != null)
            drink.BackgroundImage = background;

        drink.Name = model.Name.Trim();
        drink.SortOrder = model.SortOrder;
        drink.IsPublished = model.IsPublished;
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
