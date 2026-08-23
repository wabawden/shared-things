import { ApiError } from "./apiClient";

export function getErrorMessages(
    error: unknown,
    unauthorizedMessage: string,
): string[] {
    if (!(error instanceof ApiError)) {
        return ["Something went wrong. Please try again."];
    }

    if (error.status === 401) {
        return [unauthorizedMessage];
    }

    const validationErrors = Object.values(
        error.problem?.errors ?? {},
    ).flat();

    if (validationErrors.length > 0) {
        return validationErrors;
    }

    if (error.status === 429) {
        return [
            "Too many attempts. Please wait before trying again.",
        ];
    }

    return ["Something went wrong. Please try again."];
}