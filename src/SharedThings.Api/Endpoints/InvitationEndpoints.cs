using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedThings.Api.Authorization;
using SharedThings.Api.Contracts;
using SharedThings.Api.Data;
using SharedThings.Api.Security;

namespace SharedThings.Api.Endpoints;

public static class InvitationEndpoints
{
    public static IEndpointRouteBuilder MapInvitationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost(
                "/api/communities/{communityId:guid}/invitations",
                CreateInvitation)
            .RequireAuthorization();
        endpoints
            .MapGet(
                "/api/invitations/{token}",
                GetInvitation)
            .RequireAuthorization();
        endpoints
            .MapPost(
                "/api/invitations/{token}/accept",
                AcceptInvitation)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> CreateInvitation(
        Guid communityId,
        ClaimsPrincipal principal,
        IAuthorizationService authorizationService,
        SharedThingsDbContext db,
        TimeProvider timeProvider,
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

        var userIdValue = principal.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var token = InvitationTokens.Generate();
        var tokenHash = InvitationTokens.Hash(token);
        var expiresAt = timeProvider.GetUtcNow().AddDays(7);

        var invitation = new CommunityInvitation(
            Guid.NewGuid(),
            communityId,
            userId,
            tokenHash,
            expiresAt);

        db.CommunityInvitations.Add(invitation);

        await db.SaveChangesAsync(cancellationToken);

        var response = new CreateCommunityInvitationResponse(
            communityId,
            token,
            expiresAt);

        return Results.Created(
            $"/api/invitations/{token}",
            response);
    }
    
    private static async Task<IResult> GetInvitation(
        string token,
        ClaimsPrincipal principal,
        SharedThingsDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var userIdValue = principal.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.NotFound();
        }

        var tokenHash = InvitationTokens.Hash(token);
        var now = timeProvider.GetUtcNow();

        var invitation = await db.CommunityInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.TokenHash == tokenHash &&
                invitation.RevokedAt == null &&
                invitation.ExpiresAt > now)
            .Select(invitation =>
                new CommunityInvitationPreviewResponse(
                    invitation.CommunityId,
                    invitation.Community.Name,
                    invitation.ExpiresAt,
                    invitation.Community.Memberships.Any(
                        membership => membership.UserId == userId)))
            .SingleOrDefaultAsync(cancellationToken);

        return invitation is null
            ? Results.NotFound()
            : Results.Ok(invitation);
    }
    
    private static async Task<IResult> AcceptInvitation(
    string token,
    ClaimsPrincipal principal,
    SharedThingsDbContext db,
    TimeProvider timeProvider,
    CancellationToken cancellationToken)
    {
        var userIdValue = principal.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        var userExists = await db.Users
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            return Results.Unauthorized();
        }

        var tokenHash = InvitationTokens.Hash(token);
        var now = timeProvider.GetUtcNow();

        var invitation = await db.CommunityInvitations
            .AsNoTracking()
            .Where(invitation =>
                invitation.TokenHash == tokenHash &&
                invitation.RevokedAt == null &&
                invitation.ExpiresAt > now)
            .Select(invitation => new
            {
                invitation.CommunityId,
                CommunityName = invitation.Community.Name
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (invitation is null)
        {
            return Results.NotFound();
        }

        var alreadyMember = await db.Memberships
            .AnyAsync(
                membership =>
                    membership.UserId == userId &&
                    membership.CommunityId == invitation.CommunityId,
                cancellationToken);

        if (alreadyMember)
        {
            return Results.Ok(
                new AcceptCommunityInvitationResponse(
                    invitation.CommunityId,
                    invitation.CommunityName,
                    MembershipCreated: false));
        }

        var membership = new Membership(
            userId,
            invitation.CommunityId);

        db.Memberships.Add(membership);

        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(
            new AcceptCommunityInvitationResponse(
                invitation.CommunityId,
                invitation.CommunityName,
                MembershipCreated: true));
    }
    
}