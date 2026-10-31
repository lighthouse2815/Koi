using Koi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Koi.Web.Tests;

internal sealed class SalesTestBrand(NewsApplicationFactory factory, Guid? previous) : IAsyncDisposable
{
    public static async Task<SalesTestBrand> SelectFirst(NewsApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var settings = await db.ShopSettings.FindAsync(1) ?? throw new InvalidOperationException("Thiếu cấu hình cửa hàng.");
        var previous = settings.BrandId;
        settings.BrandId = (await db.Stores.AsNoTracking().FirstAsync()).BrandId;
        await db.SaveChangesAsync();
        return new SalesTestBrand(factory, previous);
    }
    public async ValueTask DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.ShopSettings.FindAsync(1))!.BrandId = previous;
        await db.SaveChangesAsync();
    }
}
