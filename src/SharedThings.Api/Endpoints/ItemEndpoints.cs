using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SharedThings.Api.Authorization;
using SharedThings.Api.Contracts;
using SharedThings.Api.Data;

namespace SharedThings.Api.Endpoints;

public static class ItemEndpoints
{
    public static IEndpointRouteBuilder MapItemEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/items")
            .RequireAuthorization();

        group.MapGet("/", GetCommunityItems);
        group.MapGet("/myItems", GetMyItems);
        group.MapPost("/", CreateItem);
        group.MapGet("/{itemId:guid}", GetItem);
        group.MapPut("/{itemId:guid}", UpdateItem);
        group.MapDelete("/{itemId:guid}", DeleteItem);
        
        return endpoints;
    }

    public static async Task<IResult> GetCommunityItems(
        Guid communityId,
        ClaimsPrincipal principal,
        IAuthorizationService authorizationService,
        SharedThingsDbContext dbContext,
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

        var items = await dbContext.Items
            .AsNoTracking()
            .Where(item =>
                item.Owner.Memberships.Any(
                    membership =>
                        membership.CommunityId == communityId))
            .Select(item => new ItemSummaryResponse(
                item.Id,
                item.OwnerId,
                item.Owner.DisplayName,
                item.Name,
                item.Description,
                item.Condition))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(items);
    }
    
    public static async Task<IResult> GetMyItems(
        ClaimsPrincipal principal,
        SharedThingsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(
            principal.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        var items = await dbContext.Items
            .AsNoTracking()
            .Where(item => item.OwnerId == userId)
            .Select(item => new ItemSummaryResponse(
                item.Id,
                item.OwnerId,
                item.Owner.DisplayName,
                item.Name,
                item.Description,
                item.Condition))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult> CreateItem(
        CreateItemRequest request,
        ClaimsPrincipal principal,
        SharedThingsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] = ["A item name is required."]
                });
        }

        var name = request.Name.Trim();
        var description = request.Description?.Trim();
        var condition = request.Condition?.Trim();

        if (name.Length > 100)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] =
                    [
                        "An item name cannot exceed 100 characters."
                    ]
                });
        }
        
        if (description?.Length > 1_000)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["description"] =
                    [
                        "An item description cannot exceed 1,000 characters."
                    ]
                });
        }
        
        if (condition?.Length > 100)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["condition"] =
                    [
                        "An item condition cannot exceed 100 characters."
                    ]
                });
        }

        var userId = Guid.Parse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ownerDisplayName = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);

        if (ownerDisplayName is null)
        {
            return Results.Unauthorized();
        }

        var item = new Item(
            Guid.NewGuid(),
            userId,
            name,
            description ?? string.Empty,
            condition ?? string.Empty);

        dbContext.Items.Add(item);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/items/{item.Id}",
            new ItemSummaryResponse(
                item.Id,
                item.OwnerId,
                ownerDisplayName,
                item.Name,
                item.Description,
                item.Condition));
    }
    
    public static async Task<IResult> GetItem(
        Guid itemId,
        ClaimsPrincipal principal,
        SharedThingsDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var myCommunityIds = db.Memberships
            .Where(membership => membership.UserId == userId)
            .Select(membership => membership.CommunityId);
        
        var item = await db.Items
            .AsNoTracking()
            .Where(i => i.Id == itemId &&
                        (
                            i.OwnerId == userId ||
                            db.Memberships.Any(ownerMembership =>
                                ownerMembership.UserId == i.OwnerId &&
                                myCommunityIds.Contains(ownerMembership.CommunityId))
                        ))
            .Select(i => new ItemDetailsResponse(
                i.Id,
                i.Name,
                i.Description,
                i.Condition,
                new ItemOwnerResponse(i.OwnerId, i.Owner.DisplayName),
                i.OwnerId == userId))
            .SingleOrDefaultAsync(cancellationToken);

        return item is null
            ? Results.NotFound()
            : Results.Ok(item);
    }
    
    private static async Task<IResult> UpdateItem(
    Guid itemId,
    UpdateItemRequest request,
    ClaimsPrincipal principal,
    SharedThingsDbContext db,
    CancellationToken cancellationToken)
{
    var userId = Guid.Parse(
        principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["name"] = ["An item name is required."]
            });
    }

    var name = request.Name.Trim();
    var description = request.Description?.Trim() ?? string.Empty;
    var condition = request.Condition?.Trim() ?? string.Empty;

    if (name.Length > 100)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["name"] =
                    ["An item name cannot exceed 100 characters."]
            });
    }

    if (description.Length > 255)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["description"] =
                    ["An item description cannot exceed 255 characters."]
            });
    }

    if (condition.Length > 100)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["condition"] =
                    ["An item condition cannot exceed 100 characters."]
            });
    }

    var item = await db.Items
        .Include(i => i.Owner)
        .SingleOrDefaultAsync(
            i => i.Id == itemId && i.OwnerId == userId,
            cancellationToken);
    
    if (item is null)
    {
        return Results.NotFound();
    }

    item.Name = name;
    item.Description = description;
    item.Condition = condition;

    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(
        new ItemDetailsResponse(
            item.Id,
            item.Name,
            item.Description,
            item.Condition,
            new ItemOwnerResponse(
                item.OwnerId,
                item.Owner.DisplayName),
            CanEdit: true));
}
    
    private static async Task<IResult> DeleteItem(
    Guid itemId,
    ClaimsPrincipal principal,
    SharedThingsDbContext db,
    CancellationToken cancellationToken)
{
    var userId = Guid.Parse(
        principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var item = await db.Items
        .SingleOrDefaultAsync(
            i => i.Id == itemId && i.OwnerId == userId,
            cancellationToken);
    
    if (item is null)
    {
        return Results.NotFound();
    }

    db.Items.Remove(item);
    
    await db.SaveChangesAsync(cancellationToken);

    return Results.NoContent();
}
}
