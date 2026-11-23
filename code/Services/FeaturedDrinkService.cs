using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class FeaturedDrinkService(ApplicationDbContext database)
{
    public Task<List<FeaturedDrink>> GetListAsync() =>
        database
            .FeaturedDrinks.AsNoTracking()
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToListAsync();

    public async Task CreateAsync(FeaturedDrinkEditViewModel model)
    {
        var drink = new FeaturedDrink();
        ApplyChanges(drink, model);
        database.FeaturedDrinks.Add(drink);
        await database.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, FeaturedDrinkEditViewModel model)
    {
        var drink = await database.FeaturedDrinks.FindAsync(id)
            ?? throw new AppException(ErrorCode.FEATURED_DRINK_NOT_FOUND);

        ApplyChanges(drink, model);
        database.Entry(drink).Property(value => value.Revision).OriginalValue = model.Revision;
        drink.Revision++;
        await database.SaveChangesAsync();
    }

    private static void ApplyChanges(FeaturedDrink drink, FeaturedDrinkEditViewModel model)
    {
        var image = model.ImageUploadUrl;
        var background = model.BackgroundUploadUrl;

        if (string.IsNullOrWhiteSpace(image) && string.IsNullOrEmpty(drink.Image))
            throw new AppException(ErrorCode.FEATURED_DRINK_IMAGE_REQUIRED);
        if (string.IsNullOrWhiteSpace(background) && string.IsNullOrEmpty(drink.BackgroundImage))
            throw new AppException(ErrorCode.FEATURED_DRINK_BACKGROUND_REQUIRED);

        if (!string.IsNullOrWhiteSpace(image))
            drink.Image = image;
        if (!string.IsNullOrWhiteSpace(background))
            drink.BackgroundImage = background;

        drink.Name = model.Name.Trim();
        drink.SortOrder = model.SortOrder;
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
