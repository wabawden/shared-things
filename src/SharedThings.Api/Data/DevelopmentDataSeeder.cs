using Microsoft.AspNetCore.Identity;
using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var password =
            configuration["DevelopmentSeed:Password"]
            ?? throw new InvalidOperationException(
                "Development seed password was not configured.");

        await CreateUserIfMissing(
            userManager,
            SeedIds.Bill,
            "bill@example.local",
            "Bill",
            password);

        await CreateUserIfMissing(
            userManager,
            SeedIds.Alex,
            "alex@example.local",
            "Alex",
            password);

        await CreateUserIfMissing(
            userManager,
            SeedIds.Casey,
            "casey@example.local",
            "Casey",
            password);
    }

    private static async Task CreateUserIfMissing(
        UserManager<ApplicationUser> userManager,
        Guid id,
        string email,
        string displayName,
        string password)
    {
        var existingUser =
            await userManager.FindByIdAsync(id.ToString());

        if (existingUser is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName
        };

        var result =
            await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(error =>
                $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException(
            $"Failed to create development user {email}: {errors}");
    }
}