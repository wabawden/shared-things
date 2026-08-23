import { useState } from "react";
import {
    ApiError,
    apiRequest,
} from "../api/apiClient";
import type { CreatedInvitation } from "../types/invitations";

type InvitationPanelProps = {
    communityId: string;
};

export function InvitationPanel({
                                    communityId,
                                }: InvitationPanelProps) {
    const [invitation, setInvitation] =
        useState<CreatedInvitation | null>(null);

    const [isCreating, setIsCreating] = useState(false);
    const [hasCopied, setHasCopied] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const invitationUrl = invitation
        ? `${window.location.origin}/join/${invitation.token}`
        : null;

    async function createInvitation() {
        setIsCreating(true);
        setError(null);
        setHasCopied(false);

        try {
            const created =
                await apiRequest<CreatedInvitation>(
                    `/api/communities/${encodeURIComponent(
                        communityId,
                    )}/invitations`,
                    {
                        method: "POST",
                    },
                );

            setInvitation(created);
        } catch (requestError) {
            if (
                requestError instanceof ApiError &&
                requestError.status === 404
            ) {
                setError(
                    "This community could not be found, or you no longer have access to it.",
                );
            } else {
                setError(
                    "We could not create an invitation. Please try again.",
                );
            }
        } finally {
            setIsCreating(false);
        }
    }

    async function copyInvitation() {
        if (!invitationUrl) {
            return;
        }

        try {
            await navigator.clipboard.writeText(
                invitationUrl,
            );

            setHasCopied(true);
            setError(null);
        } catch {
            setError(
                "The link could not be copied automatically. You can select and copy it below.",
            );
        }
    }

    return (
        <section className="mt-10 rounded-xl border border-emerald-200 bg-emerald-50 p-6">
            <h2 className="text-xl font-bold">
                Invite someone
            </h2>

            <p className="mt-2 text-sm leading-6 text-stone-600">
                Create a private link to share with people you
                want to join this community. Anyone with the
                active link can request membership.
            </p>

            {!invitation && (
                <button
                    type="button"
                    disabled={isCreating}
                    onClick={createInvitation}
                    className="mt-5 rounded-lg bg-emerald-700 px-4 py-2 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isCreating
                        ? "Creating invitation…"
                        : "Create invitation link"}
                </button>
            )}

            {invitation && invitationUrl && (
                <div className="mt-5">
                    <label
                        htmlFor="invitationUrl"
                        className="block text-sm font-semibold"
                    >
                        Invitation link
                    </label>

                    <div className="mt-2 flex flex-col gap-3 sm:flex-row">
                        <input
                            id="invitationUrl"
                            type="text"
                            readOnly
                            value={invitationUrl}
                            onFocus={(event) =>
                                event.currentTarget.select()
                            }
                            className="min-w-0 flex-1 rounded-lg border border-stone-300 bg-white px-3 py-2 text-sm"
                        />

                        <button
                            type="button"
                            onClick={copyInvitation}
                            className="rounded-lg bg-emerald-700 px-4 py-2 font-semibold text-white hover:bg-emerald-800"
                        >
                            {hasCopied ? "Copied" : "Copy link"}
                        </button>
                    </div>

                    <p className="mt-3 text-sm text-stone-500">
                        Expires{" "}
                        {new Date(
                            invitation.expiresAt,
                        ).toLocaleString()}
                    </p>

                    <button
                        type="button"
                        onClick={createInvitation}
                        disabled={isCreating}
                        className="mt-4 text-sm font-semibold text-emerald-800 hover:underline disabled:opacity-60"
                    >
                        Create a new link
                    </button>
                </div>
            )}

            {error && (
                <p
                    role="alert"
                    className="mt-4 text-sm text-red-800"
                >
                    {error}
                </p>
            )}
        </section>
    );
}