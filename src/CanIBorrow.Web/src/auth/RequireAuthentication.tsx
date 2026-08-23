import {
    Navigate,
    Outlet,
    useLocation,
} from "react-router";
import { useAuth } from "./AuthContext";

export function RequireAuthentication() {
    const { user, isLoading } = useAuth();
    const location = useLocation();

    if (isLoading) {
        return <p>Checking your account…</p>;
    }

    if (!user) {
        const returnTo =
            location.pathname + location.search;

        return (
            <Navigate
                to={`/login?returnTo=${encodeURIComponent(returnTo)}`}
                replace
            />
        );
    }

    return <Outlet />;
}