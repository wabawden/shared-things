using System.Security.Claims;
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
        

        return endpoints;
    }

    public static async Task<IResult> GetCommunityItems(
        Guid communityId,
        ClaimsPrincipal principal,
        IAuthorizationService authorizationService,
        ICommunityStore store)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(
            principal,
            communityId,
            new CommunityMemberRequirement());

        if (!authorizationResult.Succeeded)
        {
            return Results.NotFound();
        }

        return Results.Ok(store.GetCommunityItems(communityId));
    }
    
    public static async Task<IResult> GetMyItems(
        ClaimsPrincipal principal,
        IAuthorizationService authorizationService,
        ICommunityStore store)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Results.Ok(store.GetMyItems(userId));
    }

    private static async Task<IResult> CreateItem(
        CreateItemRequest request,
        ClaimsPrincipal principal,
        IAuthorizationService authorizationService,
        ICommunityStore store)
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
        
        if (description?.Length > 255)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["description"] =
                    [
                        "An item description cannot exceed 255 characters."
                    ]
                });
        }
        
        if (condition?.Length > 100)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] =
                    [
                        "An item condition cannot exceed 100 characters."
                    ]
                });
        }

        var userId = Guid.Parse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var item = store.CreateItem(
            userId,
            name,
            description,
            condition);

        return Results.Created(
            $"/api/items/{item.Id}",
            item);
    }
}
