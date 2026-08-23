export type CurrentUser = {
    id: string;
    email: string;
    displayName: string;
};

export type LoginRequest = {
    email: string;
    password: string;
    rememberMe: boolean;
};

export type RegisterRequest = {
    email: string;
    password: string;
    displayName: string;
};

export type ValidationProblem = {
    title?: string;
    status?: number;
    errors?: Record<string, string[]>;
};