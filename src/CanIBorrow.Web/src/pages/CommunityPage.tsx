import {
    useEffect,
    useState,
} from "react";
import { Link, useParams } from "react-router";
import {
    ApiError,
    apiRequest,
} from "../api/apiClient";
import type {
    Community,
    Item,
} from "../types/entities";
import { InvitationPanel } from "../components/InvitationPanel";
import {ItemPlaceholder} from "../components/ItemPlaceholder";

export function CommunityPage() {
    const { communityId } = useParams();

    const [community, setCommunity] =
        useState<Community | null>(null);

    const [items, setItems] = useState<Item[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [loadError, setLoadError] =
        useState<string | null>(null);

    const [refreshNumber, setRefreshNumber] = useState(0);

    useEffect(() => {
        const abortController = new AbortController();

        async function loadCommunity() {
            if (!communityId) {
                setLoadError("No community was specified.");
                setIsLoading(false);
                return;
            }

            setIsLoading(true);
            setLoadError(null);

            try {
                const [
                    loadedCommunity,
                    loadedItems,
                ] = await Promise.all([
                    apiRequest<Community>(
                        `/api/communities/${encodeURIComponent(
                            communityId,
                        )}`,
                        {
                            signal: abortController.signal,
                        },
                    ),

                    apiRequest<Item[]>(
                        `/api/items?communityId=${encodeURIComponent(
                            communityId,
                        )}`,
                        {
                            signal: abortController.signal,
                        },
                    ),
                ]);

                setCommunity(loadedCommunity);
                setItems(loadedItems);
            } catch (error) {
                if (
                    error instanceof DOMException &&
                    error.name === "AbortError"
                ) {
                    return;
                }

                if (
                    error instanceof ApiError &&
                    error.status === 404
                ) {
                    setLoadError(
                        "This community could not be found, or you do not have access to it.",
                    );
                } else {
                    setLoadError(
                        "We could not load this community. Please try again.",
                    );
                }
            } finally {
                if (!abortController.signal.aborted) {
                    setIsLoading(false);
                }
            }
        }

        void loadCommunity();

        return () => {
            abortController.abort();
        };
    }, [communityId, refreshNumber]);

    if (isLoading && !community) {
        return <p>Loading community…</p>;
    }

    if (loadError) {
        return (
            <section className="max-w-2xl">
                <Link
                    to="/dashboard"
                    className="text-sm font-semibold text-emerald-800 hover:underline"
                >
                    ← Back to dashboard
                </Link>

                <div
                    role="alert"
                    className="mt-6 rounded-lg border border-red-200 bg-red-50 p-5 text-red-800"
                >
                    {loadError}
                </div>
            </section>
        );
    }

    if (!community) {
        return null;
    }

    return (
        <div>
            <Link
                to="/dashboard"
                className="text-sm font-semibold text-emerald-800 hover:underline"
            >
                ← Back to dashboard
            </Link>

            <div className="mt-6 flex flex-wrap items-start justify-between gap-4">
                <div>
                    <p className="text-sm font-semibold uppercase tracking-wider text-emerald-700">
                        Community catalogue
                    </p>

                    <h1 className="mt-2 text-3xl font-bold tracking-tight">
                        {community.name}
                    </h1>

                    <p className="mt-3 text-stone-600">
                        Things shared by members of this community.
                    </p>
                </div>

                <button
                    type="button"
                    disabled={isLoading}
                    onClick={() =>
                        setRefreshNumber((current) => current + 1)
                    }
                    className="rounded-lg border border-stone-300 bg-white px-4 py-2 font-semibold text-stone-700 hover:bg-stone-100 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isLoading ? "Refreshing…" : "Refresh"}
                </button>
            </div>

            <InvitationPanel communityId={community.id} />

            <section className="mt-10">
                <div className="flex flex-wrap items-center justify-between gap-4">
                    <h2 className="text-2xl font-bold">
                        Shared items
                    </h2>

                    <Link
                        to="/items/new"
                        className="text-sm font-semibold text-emerald-800 hover:underline"
                    >
                        Add one of your own
                    </Link>
                </div>

                {items.length === 0 ? (
                    <div className="mt-5 rounded-xl border border-dashed border-stone-300 bg-white p-8 text-center">
                        <h3 className="font-semibold">
                            Nothing has been shared yet
                        </h3>

                        <p className="mt-2 text-stone-600">
                            Items added by community members will
                            appear here.
                        </p>
                    </div>
                ) : (
                    <ul className="mt-5 grid gap-4 sm:grid-cols-2">
                        {items.map((item) => (
                            <li
                                key={item.id}
                                className=""
                            >
                                <Link
                                    to={`/items/${item.id}`}
                                    state={{
                                        returnTo: `/communities/${community.id}`,
                                        returnLabel: `Back to ${community.name}`,}}
                                    className="block rounded-xl border border-stone-200 bg-white p-5 transition hover:border-emerald-300 hover:shadow-sm flex gap-4"
                                >
                                    <ItemPlaceholder className="aspect-square rounded-xl w-24 h-24" />
                                    <div className="shrink-0">
                                <h3 className="font-semibold">
                                    {item.name}
                                </h3>

                                <p className="mt-1 text-sm text-emerald-800">
                                    Shared by {item.ownerDisplayName}
                                </p>

                                {item.description && (
                                    <p className="mt-4 flex-1 text-sm leading-6 text-stone-600">
                                        {item.description}
                                    </p>
                                )}

                                {item.condition && (
                                    <p className="mt-4 border-t border-stone-100 pt-3 text-sm text-stone-500">
                                        Condition: {item.condition}
                                    </p>
                                )}</div></Link>
                            </li>
                        ))}
                    </ul>
                )}
            </section>
        </div>
    );
}