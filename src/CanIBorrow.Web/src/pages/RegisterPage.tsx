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

export function RegisterPage() {
    const { user, isLoading, register } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();

    const returnTo = getSafeReturnTo(location.search);

    const [displayName, setDisplayName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
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
            await register({
                displayName,
                email,
                password,
            });

            navigate(returnTo, { replace: true });
        } catch (error) {
            setErrors(
                getErrorMessages(
                    error,
                    "Unable to register this account.",
                ),
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    const loginUrl =
        `/login?returnTo=${encodeURIComponent(returnTo)}`;

    return (
        <section className="mx-auto max-w-md">
            <h1 className="text-3xl font-bold tracking-tight">
                Create an account
            </h1>

            <p className="mt-3 text-stone-600">
                Start sharing useful things with people you know.
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
                        htmlFor="displayName"
                        className="block font-medium"
                    >
                        Display name
                    </label>

                    <input
                        id="displayName"
                        type="text"
                        autoComplete="name"
                        required
                        maxLength={100}
                        value={displayName}
                        onChange={(event) =>
                            setDisplayName(event.target.value)
                        }
                        className="mt-2 w-full rounded-lg border border-stone-300 bg-white px-3 py-2 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-200"
                    />
                </div>

                <div>
                    <label
                        htmlFor="registerEmail"
                        className="block font-medium"
                    >
                        Email address
                    </label>

                    <input
                        id="registerEmail"
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
                        htmlFor="registerPassword"
                        className="block font-medium"
                    >
                        Password
                    </label>

                    <input
                        id="registerPassword"
                        type="password"
                        autoComplete="new-password"
                        required
                        minLength={8}
                        value={password}
                        onChange={(event) =>
                            setPassword(event.target.value)
                        }
                        className="mt-2 w-full rounded-lg border border-stone-300 bg-white px-3 py-2 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-200"
                    />

                    <p className="mt-2 text-sm text-stone-500">
                        Use at least eight characters, including an
                        uppercase letter, lowercase letter and number.
                    </p>
                </div>

                <p className="text-sm leading-6 text-stone-500">
                    When you create an account, we use your information
                    to provide and protect the service as described in
                    our{" "}
                    <Link
                        to="/privacy"
                        className="font-semibold text-emerald-800 hover:underline"
                    >
                        privacy and cookie notice
                    </Link>
                    .
                </p>
                
                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="w-full rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isSubmitting
                        ? "Creating account…"
                        : "Create account"}
                </button>
            </form>

            <p className="mt-6 text-center text-stone-600">
                Already have an account?{" "}
                <Link
                    to={loginUrl}
                    className="font-semibold text-emerald-800 hover:underline"
                >
                    Log in
                </Link>
            </p>
        </section>
    );
}