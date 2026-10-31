using Koi.Web.Data;
using System.Security.Claims;
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
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var user = await users.FindByEmailAsync(factory.AdminEmail);
        Assert.NotNull(user); Assert.True(await users.IsInRoleAsync(user, "Admin"));
        Assert.True(await users.CheckPasswordAsync(user, factory.AdminPassword));
        Assert.False(await users.CheckPasswordAsync(user, configuration["Admin:Password"]!));
        Assert.Equal(1, await roles.Roles.CountAsync(value => value.Name == "Admin"));
    }
    [Fact]
    public async Task IdentityUsesGuidKeysForUsersRolesAndClaims()
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var email = $"guid-{Guid.NewGuid():N}@example.test";
        var user = new IdentityUser<Guid> { UserName = email, Email = email };
        var role = new IdentityRole<Guid>($"guid-{Guid.NewGuid():N}");
        Assert.True((await users.CreateAsync(user, factory.AdminPassword)).Succeeded);
        Assert.True((await roles.CreateAsync(role)).Succeeded);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.NotEqual(Guid.Empty, role.Id);

        var userClaim = new Claim("koi-test-user", "value");
        var roleClaim = new Claim("koi-test-role", "value");
        Assert.True((await users.AddClaimAsync(user, userClaim)).Succeeded);
        Assert.True((await roles.AddClaimAsync(role, roleClaim)).Succeeded);
        var storedUserClaim = await database.Set<UserClaim>().AsNoTracking().SingleAsync(value => value.UserId == user.Id);
        var storedRoleClaim = await database.Set<RoleClaim>().AsNoTracking().SingleAsync(value => value.RoleId == role.Id);
        Assert.NotEqual(Guid.Empty, storedUserClaim.Id);
        Assert.NotEqual(Guid.Empty, storedRoleClaim.Id);
        Assert.Contains(await users.GetClaimsAsync(user), value => value.Type == userClaim.Type && value.Value == userClaim.Value);
        Assert.Contains(await roles.GetClaimsAsync(role), value => value.Type == roleClaim.Type && value.Value == roleClaim.Value);
        Assert.True((await users.RemoveClaimAsync(user, userClaim)).Succeeded);
        Assert.True((await roles.RemoveClaimAsync(role, roleClaim)).Succeeded);
    }

}
