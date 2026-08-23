import {
    createContext,
    useContext,
    useEffect,
    useState,
    type ReactNode,
} from "react";
import { ApiError, apiRequest } from "../api/apiClient";
import type {
    CurrentUser,
    LoginRequest,
    RegisterRequest,
} from "./types";

type AuthContextValue = {
    user: CurrentUser | null;
    isLoading: boolean;
    login: (request: LoginRequest) => Promise<void>;
    register: (request: RegisterRequest) => Promise<void>;
    logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

type AuthProviderProps = {
    children: ReactNode;
};

export function AuthProvider({
                                 children,
                             }: AuthProviderProps) {
    const [user, setUser] =
        useState<CurrentUser | null>(null);

    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        async function loadCurrentUser() {
            try {
                const currentUser =
                    await apiRequest<CurrentUser>("/api/auth/me");

                setUser(currentUser);
            } catch (error) {
                if (
                    error instanceof ApiError &&
                    error.status === 401
                ) {
                    setUser(null);
                    return;
                }

                console.error(
                    "Unable to load the current user.",
                    error,
                );
            } finally {
                setIsLoading(false);
            }
        }

        void loadCurrentUser();
    }, []);

    async function login(request: LoginRequest) {
        const currentUser =
            await apiRequest<CurrentUser>("/api/auth/login", {
                method: "POST",
                body: JSON.stringify(request),
            });

        setUser(currentUser);
    }

    async function register(request: RegisterRequest) {
        const currentUser =
            await apiRequest<CurrentUser>("/api/auth/register", {
                method: "POST",
                body: JSON.stringify(request),
            });

        setUser(currentUser);
    }

    async function logout() {
        await apiRequest<void>("/api/auth/logout", {
            method: "POST",
        });

        setUser(null);
    }

    return (
        <AuthContext.Provider
            value={{
                user,
                isLoading,
                login,
                register,
                logout,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error(
            "useAuth must be used inside AuthProvider.",
        );
    }

    return context;
}