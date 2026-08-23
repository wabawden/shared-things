import type { ValidationProblem } from "../auth/types";

export class ApiError extends Error {
    public readonly status: number;
    public readonly problem?: ValidationProblem;

    constructor(
        status: number,
        problem?: ValidationProblem,
    ) {
        super(
            problem?.title ??
            `Request failed with status ${status}`,
        );

        this.name = "ApiError";
        this.status = status;
        this.problem = problem;
    }
}

export async function apiRequest<T>(
    path: string,
    options: RequestInit = {},
): Promise<T> {
    const response = await fetch(path, {
        ...options,
        credentials: "include",
        headers: {
            ...(options.body
                ? { "Content-Type": "application/json" }
                : {}),
            ...options.headers,
        },
    });

    if (!response.ok) {
        let problem: ValidationProblem | undefined;

        try {
            problem = await response.json();
        } catch {
            // Some responses, including 401, may have no body.
        }

        throw new ApiError(response.status, problem);
    }

    if (response.status === 204) {
        return undefined as T;
    }

    return response.json() as Promise<T>;
}