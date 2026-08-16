using Microsoft.EntityFrameworkCore;
using SharedThings.Api.Authentication;
using SharedThings.Api.Authorization;
using SharedThings.Api.Data;
using SharedThings.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("SharedThings")
    ?? throw new InvalidOperationException(
        "Connection string 'SharedThings' was not found.");

builder.Services.AddDbContext<SharedThingsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
    .AddScheme<DevelopmentAuthenticationOptions, DevelopmentAuthenticationHandler>(
        DevelopmentAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<ICommunityStore, InMemoryCommunityStore>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, CommunityMemberHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { name = "Shared Things API" }));
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
