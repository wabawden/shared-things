using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedThings.Api.Data;

namespace SharedThings.Api.Authorization;

public sealed class CommunityMemberHandler
    : AuthorizationHandler<CommunityMemberRequirement, Guid>
{
    private readonly SharedThingsDbContext _db;

    public CommunityMemberHandler(SharedThingsDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CommunityMemberRequirement requirement,
        Guid communityId)
    {
        var userIdValue = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        var isMember = await _db.Memberships
            .AsNoTracking()
            .AnyAsync(membership =>
                membership.UserId == userId &&
                membership.CommunityId == communityId);

        if (isMember)
        {
            context.Succeed(requirement);
        }
    }
}