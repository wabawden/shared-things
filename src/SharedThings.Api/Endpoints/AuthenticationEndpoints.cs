using Microsoft.AspNetCore.Identity;
using SharedThings.Api.Contracts;
using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/register", Register)
            .AllowAnonymous()
            .RequireRateLimiting("registration");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .RequireRateLimiting("login");

        group.MapPost("/logout", Logout)
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();

        var validationErrors =
            ValidateRegistration(email, displayName);

        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName
        };

        var result = await userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            return Results.ValidationProblem(
                result.Errors
                    .GroupBy(error => error.Code)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.Description)
                            .ToArray()));
        }

        await signInManager.SignInAsync(
            user,
            isPersistent: false);

        return Results.Created(
            "/api/auth/me",
            new CurrentUserResponse(
                user.Id,
                user.Email!,
                user.DisplayName));
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var email = request.Email.Trim();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(
            new CurrentUserResponse(
                user.Id,
                user.Email!,
                user.DisplayName));
    }

    private static async Task<IResult> Logout(
        SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUser(
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(principal);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(
            new CurrentUserResponse(
                user.Id,
                user.Email!,
                user.DisplayName));
    }

    private static Dictionary<string, string[]> ValidateRegistration(
        string email,
        string displayName)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(email))
        {
            errors["email"] = ["An email address is required."];
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors["displayName"] =
                ["A display name is required."];
        }
        else if (displayName.Length > 100)
        {
            errors["displayName"] =
                ["A display name cannot exceed 100 characters."];
        }

        return errors;
    }
}