using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class DrinkService(ApplicationDbContext database, CategoryService categoryService)
{
    public async Task<AdminDrinksViewModel> GetListAsync(
        string? search,
        Guid categoryId = default,
        int page = 1)
    {
        search = search?.Trim();

        var query = database
            .Drinks.AsNoTracking()
            .Where(value => categoryId == Guid.Empty || value.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(search))
        {
            var keyword = search.ToLower();
            query = query.Where(value => value.Name.ToLower().Contains(keyword));
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
                .ToListAsync(),

            new SelectList(await categoryService.GetListAsync(), nameof(Category.Id), nameof(Category.Name)),
            categoryId,
            search,
            page,
            pages
        );
    }

    public async Task CreateAsync(DrinkEditViewModel model)
    {
        var drink = new Drink();
        await ApplyChangesAsync(drink, model);
        database.Drinks.Add(drink);
        await database.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, DrinkEditViewModel model)
    {
        var drink = await database.Drinks.FindAsync(id)
            ?? throw new AppException(ErrorCode.DRINK_NOT_FOUND);

        await ApplyChangesAsync(drink, model);
        database.Entry(drink)
                .Property(value => value.Revision)
                .OriginalValue = model.Revision;

        drink.Revision++;
        await database.SaveChangesAsync();
    }

    private async Task ApplyChangesAsync(Drink drink, DrinkEditViewModel model)
    {
        if (!await database.Categories.AnyAsync(value => value.Id == model.CategoryId))
            throw new AppException(ErrorCode.DRINK_CATEGORY_INVALID);

        if (!string.IsNullOrWhiteSpace(model.ImageUploadUrl))
            drink.Image = model.ImageUploadUrl;
        else if (model.RemoveImage)
            drink.Image = "";

        drink.Name = model.Name.Trim();
        drink.CategoryId = model.CategoryId;
        drink.Description = model.Description?.Trim() ?? "";
        drink.SortOrder = model.SortOrder;
    }

    public async Task DeleteAsync(Guid id)
    {
        var drink = await database.Drinks.FindAsync(id)
            ?? throw new AppException(ErrorCode.DRINK_NOT_FOUND);

        database.Drinks.Remove(drink);
        await database.SaveChangesAsync();
    }
}
