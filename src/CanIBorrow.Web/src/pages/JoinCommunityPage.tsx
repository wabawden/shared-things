import {
    useEffect,
    useState,
} from "react";
import {
    Link,
    useLocation,
    useParams,
} from "react-router";
import {
    ApiError,
    apiRequest,
} from "../api/apiClient";
import { useAuth } from "../auth/AuthContext";
import type {
    AcceptedInvitation,
    InvitationPreview,
} from "../types/invitations";

export function JoinCommunityPage() {
    const { token } = useParams();
    const location = useLocation();
    const { user, isLoading: isLoadingUser } =
        useAuth();

    const [preview, setPreview] =
        useState<InvitationPreview | null>(null);

    const [accepted, setAccepted] =
        useState<AcceptedInvitation | null>(null);

    const [isLoadingPreview, setIsLoadingPreview] =
        useState(false);

    const [isAccepting, setIsAccepting] =
        useState(false);

    const [error, setError] =
        useState<string | null>(null);

    useEffect(() => {
        if (!user || !token) {
            return;
        }

        const abortController = new AbortController();

        async function loadPreview() {
            setIsLoadingPreview(true);
            setError(null);

            try {
                const loadedPreview =
                    await apiRequest<InvitationPreview>(
                        `/api/invitations/${encodeURIComponent(
                            token!,
                        )}`,
                        {
                            signal: abortController.signal,
                        },
                    );

                setPreview(loadedPreview);
            } catch (requestError) {
                if (
                    requestError instanceof DOMException &&
                    requestError.name === "AbortError"
                ) {
                    return;
                }

                if (
                    requestError instanceof ApiError &&
                    requestError.status === 404
                ) {
                    setError(
                        "This invitation is invalid, expired or no longer available.",
                    );
                } else {
                    setError(
                        "We could not load this invitation. Please try again.",
                    );
                }
            } finally {
                if (!abortController.signal.aborted) {
                    setIsLoadingPreview(false);
                }
            }
        }

        void loadPreview();

        return () => {
            abortController.abort();
        };
    }, [token, user]);

    if (isLoadingUser) {
        return <p>Checking your account…</p>;
    }

    if (!token) {
        return (
            <InvitationError message="No invitation token was provided." />
        );
    }

    if (!user) {
        const returnTo =
            location.pathname + location.search;

        const encodedReturnTo =
            encodeURIComponent(returnTo);

        return (
            <section className="mx-auto max-w-xl rounded-xl border border-stone-200 bg-white p-8 text-center">
                <p className="text-sm font-semibold uppercase tracking-wider text-emerald-700">
                    Community invitation
                </p>

                <h1 className="mt-3 text-3xl font-bold">
                    You’ve been invited
                </h1>

                <p className="mt-4 text-stone-600">
                    Log in or create an account to see which
                    community invited you.
                </p>

                <div className="mt-8 flex flex-col justify-center gap-3 sm:flex-row">
                    <Link
                        to={`/login?returnTo=${encodedReturnTo}`}
                        className="rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800"
                    >
                        Log in
                    </Link>

                    <Link
                        to={`/register?returnTo=${encodedReturnTo}`}
                        className="rounded-lg border border-stone-300 bg-white px-5 py-3 font-semibold text-stone-700 hover:bg-stone-100"
                    >
                        Create account
                    </Link>
                </div>
            </section>
        );
    }

    if (isLoadingPreview && !preview) {
        return <p>Loading invitation…</p>;
    }

    if (error) {
        return <InvitationError message={error} />;
    }

    if (!preview) {
        return null;
    }

    async function acceptInvitation() {
        setIsAccepting(true);
        setError(null);

        try {
            const result =
                await apiRequest<AcceptedInvitation>(
                    `/api/invitations/${encodeURIComponent(
                        token!,
                    )}/accept`,
                    {
                        method: "POST",
                    },
                );

            setAccepted(result);
        } catch (requestError) {
            if (
                requestError instanceof ApiError &&
                requestError.status === 404
            ) {
                setError(
                    "This invitation has expired or is no longer available.",
                );
            } else {
                setError(
                    "We could not accept this invitation. Please try again.",
                );
            }
        } finally {
            setIsAccepting(false);
        }
    }

    if (accepted) {
        return (
            <section className="mx-auto max-w-xl rounded-xl border border-emerald-200 bg-emerald-50 p-8 text-center">
                <h1 className="text-3xl font-bold">
                    {accepted.membershipCreated
                        ? `You’ve joined ${accepted.communityName}`
                        : `You’re already a member of ${accepted.communityName}`}
                </h1>

                <Link
                    to={`/communities/${accepted.communityId}`}
                    className="mt-8 inline-block rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800"
                >
                    View community
                </Link>
            </section>
        );
    }

    return (
        <section className="mx-auto max-w-xl rounded-xl border border-stone-200 bg-white p-8 text-center">
            <p className="text-sm font-semibold uppercase tracking-wider text-emerald-700">
                Community invitation
            </p>

            <h1 className="mt-3 text-3xl font-bold">
                {preview.communityName}
            </h1>

            <p className="mt-4 text-stone-600">
                You have been invited to join this private
                sharing community.
            </p>

            <p className="mt-3 text-sm text-stone-500">
                Invitation expires{" "}
                {new Date(preview.expiresAt).toLocaleString()}
            </p>

            {preview.alreadyMember ? (
                <Link
                    to={`/communities/${preview.communityId}`}
                    className="mt-8 inline-block rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800"
                >
                    View community
                </Link>
            ) : (
                <button
                    type="button"
                    disabled={isAccepting}
                    onClick={acceptInvitation}
                    className="mt-8 rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isAccepting
                        ? "Joining community…"
                        : "Join community"}
                </button>
            )}
        </section>
    );
}

type InvitationErrorProps = {
    message: string;
};

function InvitationError({
                             message,
                         }: InvitationErrorProps) {
    return (
        <section className="mx-auto max-w-xl">
            <div
                role="alert"
                className="rounded-lg border border-red-200 bg-red-50 p-5 text-red-800"
            >
                {message}
            </div>

            <Link
                to="/dashboard"
                className="mt-6 inline-block font-semibold text-emerald-800 hover:underline"
            >
                Go to dashboard
            </Link>
        </section>
    );
}