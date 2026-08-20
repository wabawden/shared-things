using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedThings.Api.Authorization;
using SharedThings.Api.Data;
using SharedThings.Api.Contracts;
namespace SharedThings.Api.Endpoints;

public static class CommunityEndpoints
{
    public static IEndpointRouteBuilder MapCommunityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/communities")
            .RequireAuthorization();

        group.MapGet("/", GetMyCommunities);
        
        group.MapPost("/", CreateCommunity);
        
        group.MapGet("/{communityId:guid}", GetCommunity);

        return endpoints;
    }

    public static async Task<IResult> GetMyCommunities(
        ClaimsPrincipal principal,
        SharedThingsDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var communities = await db.Memberships
            .AsNoTracking()
            .Where(membership => membership.UserId == userId)
            .Select(membership => new CommunityResponse(
                membership.Community.Id,
                membership.Community.Name))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(communities);
    }
    
    private static async Task<IResult> CreateCommunity(
        CreateCommunityRequest request,
        ClaimsPrincipal principal,
        SharedThingsDbContext db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] = ["A community name is required."]
                });
        }

        var name = request.Name.Trim();

        if (name.Length > 100)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] =
                    [
                        "A community name cannot exceed 100 characters."
                    ]
                });
        }

        var userId = Guid.Parse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var userExists = await db.Users
            .AnyAsync(user => user.Id == userId, cancellationToken);

        if (!userExists)
        {
            return Results.Unauthorized();
        }

        var community = new Community(
            Guid.NewGuid(),
            name);

        var membership = new Membership(
            userId,
            community.Id);

        db.Communities.Add(community);
        db.Memberships.Add(membership);

        await db.SaveChangesAsync(cancellationToken);

        var response = new CommunityResponse(
            community.Id,
            community.Name);

        return Results.Created(
            $"/api/communities/{community.Id}",
            response);
    }
    
    public static async Task<IResult> GetCommunity(
        Guid communityId,
        ClaimsPrincipal principal,
        IAuthorizationService authorizationService,
        SharedThingsDbContext db,
        CancellationToken cancellationToken)
    {
        var authorizationResult =
            await authorizationService.AuthorizeAsync(
                principal,
                communityId,
                new CommunityMemberRequirement());

        if (!authorizationResult.Succeeded)
        {
            return Results.NotFound();
        }

        var community = await db.Communities
            .AsNoTracking()
            .Where(community => community.Id == communityId)
            .Select(community => new CommunityResponse(
                community.Id,
                community.Name))
            .SingleOrDefaultAsync(cancellationToken);

        return community is null
            ? Results.NotFound()
            : Results.Ok(community);
    }
}
