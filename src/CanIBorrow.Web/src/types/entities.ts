export type Community = {
    id: string;
    name: string;
    memberCount: number;
};

export type Item = {
    id: string;
    ownerId: string;
    ownerDisplayName: string;
    name: string;
    description: string;
    condition: string;
    url?: string;
};