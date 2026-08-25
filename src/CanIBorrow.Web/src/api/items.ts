import { apiRequest } from "./apiClient";

export type ItemOwner = {
    id: string;
    displayName: string;
};

export type ItemDetails = {
    id: string;
    name: string;
    description: string;
    condition: string;
    owner: ItemOwner;
    canEdit: boolean;
};

export type UpdateItemRequest = {
    name: string;
    description: string;
    condition: string;
};

export function getItem(
    itemId: string,
    signal?: AbortSignal,
): Promise<ItemDetails> {
    return apiRequest<ItemDetails>(
        `/api/items/${itemId}`,
        { signal },
    );
}

export function updateItem(
    itemId: string,
    request: UpdateItemRequest,
): Promise<ItemDetails> {
    return apiRequest<ItemDetails>(
        `/api/items/${itemId}`,
        {
            method: "PUT",
            body: JSON.stringify(request),
        },
    );
}