using Microsoft.AspNetCore.Authorization;
using SharedThings.Api.Data;
using System.Security.Claims;

namespace SharedThings.Api.Authorization;

public sealed class CommunityMemberHandler(ICommunityStore store)
    : AuthorizationHandler<CommunityMemberRequirement, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CommunityMemberRequirement requirement,
        Guid communityId)
    {
        var userIdValue = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdValue, out var userId) &&
            store.IsMember(userId, communityId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}