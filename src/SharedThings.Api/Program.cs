using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SharedThings.Api.Authentication;
using SharedThings.Api.Authorization;
using SharedThings.Api.Data;
using SharedThings.Api.Data.Entities;
using SharedThings.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SharedThingsDbContext>(
    (serviceProvider, options) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var connectionString =
            configuration.GetConnectionString("SharedThings")
            ?? throw new InvalidOperationException(
                "Connection string 'SharedThings' was not found.");

        options.UseNpgsql( connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
    });

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

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<SharedThingsDbContext>(
        name: "postgres");

const string combinedAuthenticationScheme =
    "DevelopmentOrIdentity";

var developmentAuthenticationEnabled =
    builder.Environment.IsDevelopment() ||
    builder.Environment.IsEnvironment("Testing");

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
            {
                var hasDevelopmentHeader =
                    context.Request.Headers.ContainsKey(
                        DevelopmentAuthenticationHandler.UserIdHeader);

                return developmentAuthenticationEnabled &&
                       hasDevelopmentHeader
                    ? DevelopmentAuthenticationHandler.SchemeName
                    : IdentityConstants.ApplicationScheme;
            };
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
    options.Cookie.SecurePolicy =
        builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;

    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

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
builder.Services.AddScoped<
    IAuthorizationHandler,
    CommunityMemberHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);

var authenticationPermitLimit =
    builder.Environment.IsEnvironment("Testing")
        ? 10_000
        : 10;

var registrationPermitLimit =
    builder.Environment.IsEnvironment("Testing")
        ? 10_000
        : 5;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
            context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authenticationPermitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("registration", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
            context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = registrationPermitLimit,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    
});

var app = builder.Build();

if (app.Environment.IsProduction())
{
    await using var scope =
        app.Services.CreateAsyncScope();

    var db = scope.ServiceProvider
        .GetRequiredService<SharedThingsDbContext>();

    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedAsync(
        app.Services,
        app.Configuration);
}


app.UseExceptionHandler();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet(
    "/api",
    () => Results.Ok(new { name = "Shared Things API" }));
app.MapHealthChecks("/health")
    .AllowAnonymous();
app.MapAuthenticationEndpoints();
app.MapCommunityEndpoints();
app.MapItemEndpoints();
app.MapInvitationEndpoints();

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
