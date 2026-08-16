using System.Security.Claims;
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

        return endpoints;
    }

    private static IResult GetMyCommunities(ClaimsPrincipal principal, ICommunityStore store)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Results.Ok(store.GetCommunitiesFor(userId));
    }
    
    private static IResult CreateCommunity(
        CreateCommunityRequest request,
        ClaimsPrincipal principal,
        ICommunityStore store)
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

        var community = store.CreateCommunity(userId, name);

        return Results.Created(
            $"/api/communities/{community.Id}",
            community);
    }
}
