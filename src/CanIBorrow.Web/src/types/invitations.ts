export type CreatedInvitation = {
    communityId: string;
    token: string;
    expiresAt: string;
};

export type InvitationPreview = {
    communityId: string;
    communityName: string;
    expiresAt: string;
    alreadyMember: boolean;
};

export type AcceptedInvitation = {
    communityId: string;
    communityName: string;
    membershipCreated: boolean;
};