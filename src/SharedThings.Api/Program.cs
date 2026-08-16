using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SharedThings.Api.Authentication;
using SharedThings.Api.Authorization;
using SharedThings.Api.Data;
using SharedThings.Api.Data.Entities;
using SharedThings.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("SharedThings")
    ?? throw new InvalidOperationException(
        "Connection string 'SharedThings' was not found.");

builder.Services.AddDbContext<SharedThingsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SharedThingsDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

const string combinedAuthenticationScheme =
    "DevelopmentOrIdentity";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            combinedAuthenticationScheme;

        options.DefaultChallengeScheme =
            combinedAuthenticationScheme;

        options.DefaultForbidScheme =
            combinedAuthenticationScheme;
    })
    .AddPolicyScheme(
        combinedAuthenticationScheme,
        displayName: null,
        options =>
        {
            options.ForwardDefaultSelector = context =>
                context.Request.Headers.ContainsKey(
                    DevelopmentAuthenticationHandler.UserIdHeader)
                    ? DevelopmentAuthenticationHandler.SchemeName
                    : IdentityConstants.ApplicationScheme;
        })
    .AddScheme<
        DevelopmentAuthenticationOptions,
        DevelopmentAuthenticationHandler>(
        DevelopmentAuthenticationHandler.SchemeName,
        _ => { })
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "shared-things-session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSingleton<ICommunityStore, InMemoryCommunityStore>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, CommunityMemberHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedAsync(
        app.Services,
        app.Configuration);
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { name = "Shared Things API" }));
app.MapAuthenticationEndpoints();
app.MapCommunityEndpoints();
app.MapItemEndpoints();

app.Run();

namespace Microsoft.AspNetCore.Authorization.Policy
{
    public interface IAuthorizationMiddlewareResultHandler
    {
    }
}

public partial class Program;
