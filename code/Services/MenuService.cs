using Koi.Web.Data;
using Koi.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Services;

public class MenuService(
    ApplicationDbContext database,
    ImageUploadService uploads
)
{
    public async Task<AdminMenusViewModel> GetListAsync() =>
        new(
            await database
                .Menus.AsNoTracking()
                .OrderBy(value => value.SortOrder)
                .ThenBy(value => value.Id)
                .ToArrayAsync()
        );

    public async Task<MenuEditViewModel?> GetEditAsync(Guid id)
    {
        var value = await database
            .Menus.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null
            ? null
            : new MenuEditViewModel
            {
                Id = id,
                Revision = value.Revision,

                Label = value.Label,
                SortOrder = value.SortOrder,
                IsPublished = value.IsPublished,
                CurrentImage = value.File,
            };
    }

    public async Task<SaveResult> SaveAsync(
        MenuEditViewModel model,
        Guid? id,
        ModelStateDictionary modelState
    )
    {
        var value = id.HasValue
            ? await database.Menus.SingleOrDefaultAsync(value => value.Id == id)
            : new Menu();
        if (value == null)
            return SaveResult.NotFound;
        model.CurrentImage = value.File;
        var image = await uploads.ReadAsync(
            model.ImageUpload,
            modelState,
            nameof(model.ImageUpload), model.ImageUploadId, "menus"
        );
        if (id == null && image == null)
            modelState.AddModelError(nameof(model.ImageUpload), "Chọn ảnh thực đơn.");
        if (!modelState.IsValid)
        {
            return SaveResult.Invalid;
        }
        if (image != null)
            value.File = await uploads.SaveAsync(image, "menus");
        value.Label = model.Label.Trim();

        value.SortOrder = model.SortOrder;
        value.IsPublished = model.IsPublished;
        if (id == null)
            database.Menus.Add(value);
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
            .Menus.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id);
        return value == null ? null : new DeleteContentViewModel(id, value.Label);
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
