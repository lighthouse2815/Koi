using Koi.Web.Data;
using Koi.Web.Exceptions;
using Koi.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class MenuService(
    ApplicationDbContext database,
    ImageUploadService uploads
)
{
    public Task<List<Menu>> GetListAsync() =>
        database
            .Menus.AsNoTracking()
            .OrderBy(menu => menu.SortOrder)
            .ThenBy(menu => menu.Id)
            .ToListAsync();

    public async Task CreateAsync(MenuEditViewModel model)
    {
        var menu = new Menu();
        await ApplyChangesAsync(menu, model);
        if (string.IsNullOrEmpty(menu.File))
            throw new AppException(ErrorCode.MENU_IMAGE_REQUIRED);

        database.Menus.Add(menu);
        await database.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, MenuEditViewModel model)
    {
        var menu = await database.Menus.FindAsync(id)
            ?? throw new AppException(ErrorCode.MENU_NOT_FOUND);

        await ApplyChangesAsync(menu, model);
        database.Entry(menu).Property(value => value.Revision).OriginalValue = model.Revision;
        menu.Revision++;
        await database.SaveChangesAsync();
    }

    private async Task ApplyChangesAsync(Menu menu, MenuEditViewModel model)
    {
        var image = await uploads.GetImageUrlAsync(model.ImageUploadId, "menus");
        if (image != null)
            menu.File = image;

        menu.Label = model.Label.Trim();
        menu.SortOrder = model.SortOrder;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var value = await database.Menus.SingleOrDefaultAsync(value => value.Id == id);
        if (value == null)
            return false;
        database.Menus.Remove(value);
        await database.SaveChangesAsync();
        return true;
    }
}
