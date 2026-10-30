using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Koi.Web.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // One PostgreSQL session serializes migrations and Identity bootstrap across app instances.
        // https://www.postgresql.org/docs/17/explicit-locking.html#ADVISORY-LOCKS
        await database.Database.OpenConnectionAsync();
        try
        {
            await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_lock(542019361036)");
            try
            {
                await database.Database.MigrateAsync();
                var email = configuration["Admin:Email"];
                var password = configuration["Admin:Password"];
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
                var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                if (!await roles.RoleExistsAsync("Admin")) EnsureSuccess(await roles.CreateAsync(new IdentityRole("Admin")));
                var user = await users.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                    EnsureSuccess(await users.CreateAsync(user, password));
                }
                if (!await users.IsInRoleAsync(user, "Admin")) EnsureSuccess(await users.AddToRoleAsync(user, "Admin"));
            }
            finally
            {
                await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(542019361036)");
            }
        }
        finally
        {
            await database.Database.CloseConnectionAsync();
        }
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
