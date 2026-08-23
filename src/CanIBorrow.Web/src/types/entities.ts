export type Community = {
    id: string;
    name: string;
};

export type Item = {
    id: string;
    ownerId: string;
    ownerDisplayName: string;
    name: string;
    description: string;
    condition: string;
};