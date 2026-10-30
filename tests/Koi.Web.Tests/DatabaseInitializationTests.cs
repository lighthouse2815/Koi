using Koi.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Koi.Web.Tests;

public class DatabaseInitializationTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task ConcurrentInitializersKeepOneAdminRoleAndPreserveExistingPassword()
    {
        var configuration = new ConfigurationBuilder()
            .AddConfiguration(factory.Services.GetRequiredService<IConfiguration>())
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Admin:Password"] = factory.AdminPassword + "Changed2!" })
            .Build();
        await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => DatabaseInitializer.InitializeAsync(factory.Services, configuration)));
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await users.FindByEmailAsync(factory.AdminEmail);
        Assert.NotNull(user); Assert.True(await users.IsInRoleAsync(user, "Admin"));
        Assert.True(await users.CheckPasswordAsync(user, factory.AdminPassword));
        Assert.False(await users.CheckPasswordAsync(user, configuration["Admin:Password"]!));
        Assert.Equal(1, await roles.Roles.CountAsync(value => value.Name == "Admin"));
    }
}
