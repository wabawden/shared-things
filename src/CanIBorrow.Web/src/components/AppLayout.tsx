import { useState } from "react";
import {
    Link,
    NavLink,
    Outlet,
    useNavigate,
} from "react-router";
import { useAuth } from "../auth/AuthContext";

const authenticatedNavigation = [
    { to: "/dashboard", label: "Dashboard" },
    { to: "/items/new", label: "Add item" },
    { to: "/communities/new", label: "Create community" },
];

export function AppLayout() {
    const { user, isLoading, logout } = useAuth();
    const navigate = useNavigate();

    const [isLoggingOut, setIsLoggingOut] =
        useState(false);

    const [logoutError, setLogoutError] =
        useState<string | null>(null);

    async function handleLogout() {
        setLogoutError(null);
        setIsLoggingOut(true);

        try {
            await logout();
            navigate("/", { replace: true });
        } catch {
            setLogoutError(
                "We could not log you out. Please try again.",
            );
        } finally {
            setIsLoggingOut(false);
        }
    }

    const navigationClassName = ({
                                     isActive,
                                 }: {
        isActive: boolean;
    }) =>
        isActive
            ? "font-semibold text-emerald-800"
            : "text-stone-600 hover:text-stone-900";

    return (
        <div className="flex min-h-screen flex-col bg-stone-50 text-stone-900">
            <header className="border-b border-stone-200 bg-white">
                <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-4 px-6 py-5">
                    <Link
                        to={user ? "/dashboard" : "/"}
                        className="text-xl font-semibold text-emerald-800"
                    >
                        Can I borrow..?
                    </Link>

                    {!isLoading && (
                        <nav aria-label="Main navigation">
                            {user ? (
                                <div className="flex flex-wrap items-center gap-5">
                                    <ul className="flex flex-wrap items-center gap-5">
                                        {authenticatedNavigation.map((item) => (
                                            <li key={item.to}>
                                                <NavLink
                                                    to={item.to}
                                                    className={navigationClassName}
                                                >
                                                    {item.label}
                                                </NavLink>
                                            </li>
                                        ))}
                                    </ul>

                                    <span className="text-sm text-stone-500">
                    {user.displayName}
                  </span>

                                    <button
                                        type="button"
                                        disabled={isLoggingOut}
                                        onClick={handleLogout}
                                        className="rounded-lg border border-stone-300 px-3 py-2 text-sm font-semibold text-stone-700 hover:bg-stone-100 disabled:cursor-not-allowed disabled:opacity-60"
                                    >
                                        {isLoggingOut
                                            ? "Logging out…"
                                            : "Log out"}
                                    </button>
                                </div>
                            ) : (
                                <ul className="flex items-center gap-5">
                                    <li>
                                        <NavLink
                                            to="/"
                                            end
                                            className={navigationClassName}
                                        >
                                            Home
                                        </NavLink>
                                    </li>

                                    <li>
                                        <NavLink
                                            to="/login"
                                            className={navigationClassName}
                                        >
                                            Log in
                                        </NavLink>
                                    </li>

                                    <li>
                                        <NavLink
                                            to="/register"
                                            className="rounded-lg bg-emerald-700 px-4 py-2 font-semibold text-white hover:bg-emerald-800"
                                        >
                                            Register
                                        </NavLink>
                                    </li>
                                </ul>
                            )}
                        </nav>
                    )}
                </div>

                {logoutError && (
                    <div
                        role="alert"
                        className="border-t border-red-200 bg-red-50 px-6 py-3 text-center text-sm text-red-800"
                    >
                        {logoutError}
                    </div>
                )}
            </header>

            <main className="mx-auto w-full max-w-5xl flex-1 px-6 py-12">
                <Outlet />
            </main>

            <footer className="border-t border-stone-200 bg-white">
                <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-3 px-6 py-5 text-sm text-stone-500">
    <span>
      Share useful things with people you already know.
    </span>

                    <Link
                        to="/privacy"
                        className="font-medium text-stone-600 hover:text-emerald-800 hover:underline"
                    >
                        Privacy and cookies
                    </Link>
                </div>
            </footer>
        </div>
    );
}