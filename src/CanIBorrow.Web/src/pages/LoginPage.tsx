import { useState, type FormEvent } from "react";
import {
    Link,
    Navigate,
    useLocation,
    useNavigate,
} from "react-router";
import { getErrorMessages } from "../api/getErrorMessages";
import { getSafeReturnTo } from "../auth/returnTo";
import { useAuth } from "../auth/AuthContext";

export function LoginPage() {
    const { user, isLoading, login } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();

    const returnTo = getSafeReturnTo(location.search);

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [rememberMe, setRememberMe] = useState(false);
    const [errors, setErrors] = useState<string[]>([]);
    const [isSubmitting, setIsSubmitting] = useState(false);

    if (isLoading) {
        return <p>Checking your account…</p>;
    }

    if (user) {
        return <Navigate to={returnTo} replace />;
    }

    async function handleSubmit(
        event: FormEvent<HTMLFormElement>,
    ) {
        event.preventDefault();

        setErrors([]);
        setIsSubmitting(true);

        try {
            await login({
                email,
                password,
                rememberMe,
            });

            navigate(returnTo, { replace: true });
        } catch (error) {
            setErrors(
                getErrorMessages(
                    error,
                    "The email address or password was incorrect.",
                ),
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    const registerUrl =
        `/register?returnTo=${encodeURIComponent(returnTo)}`;

    return (
        <section className="mx-auto max-w-md">
            <h1 className="text-3xl font-bold tracking-tight">
                Log in
            </h1>

            <p className="mt-3 text-stone-600">
                Log in to view your items and communities.
            </p>

            {errors.length > 0 && (
                <div
                    role="alert"
                    className="mt-6 rounded-lg border border-red-200 bg-red-50 p-4 text-red-800"
                >
                    <ul className="list-disc space-y-1 pl-5">
                        {errors.map((error) => (
                            <li key={error}>{error}</li>
                        ))}
                    </ul>
                </div>
            )}

            <form
                onSubmit={handleSubmit}
                className="mt-8 space-y-6"
            >
                <div>
                    <label
                        htmlFor="email"
                        className="block font-medium"
                    >
                        Email address
                    </label>

                    <input
                        id="email"
                        type="email"
                        autoComplete="email"
                        required
                        value={email}
                        onChange={(event) =>
                            setEmail(event.target.value)
                        }
                        className="mt-2 w-full rounded-lg border border-stone-300 bg-white px-3 py-2 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-200"
                    />
                </div>

                <div>
                    <label
                        htmlFor="password"
                        className="block font-medium"
                    >
                        Password
                    </label>

                    <input
                        id="password"
                        type="password"
                        autoComplete="current-password"
                        required
                        value={password}
                        onChange={(event) =>
                            setPassword(event.target.value)
                        }
                        className="mt-2 w-full rounded-lg border border-stone-300 bg-white px-3 py-2 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-200"
                    />
                </div>

                <label className="flex items-center gap-3">
                    <input
                        type="checkbox"
                        checked={rememberMe}
                        onChange={(event) =>
                            setRememberMe(event.target.checked)
                        }
                    />

                    <span>Keep me logged in</span>
                </label>

                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="w-full rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isSubmitting ? "Logging in…" : "Log in"}
                </button>
            </form>

            <p className="mt-6 text-center text-stone-600">
                Don’t have an account?{" "}
                <Link
                    to={registerUrl}
                    className="font-semibold text-emerald-800 hover:underline"
                >
                    Register
                </Link>
            </p>
        </section>
    );
}