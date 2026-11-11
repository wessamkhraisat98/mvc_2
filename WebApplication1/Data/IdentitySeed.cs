using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Data;

public static class IdentitySeed
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "User", "Admin", "SuperAdmin" })
            if (!await roles.RoleExistsAsync(role))
                Check(await roles.CreateAsync(new IdentityRole(role)));

        var email = configuration["SuperAdmin:Email"];
        var password = configuration["SuperAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await users.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser { FullName = "Super Admin", UserName = email, Email = email };
        var db = services.GetRequiredService<ApplicationDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        Check(await users.CreateAsync(user, password));
        Check(await users.AddToRoleAsync(user, "SuperAdmin"));
        await transaction.CommitAsync();
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
