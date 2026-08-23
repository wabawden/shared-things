import {
    useEffect,
    useState,
} from "react";
import { Link } from "react-router";
import { apiRequest } from "../api/apiClient";
import { useAuth } from "../auth/AuthContext";
import type {
    Community,
    Item,
} from "../types/entities";

export function DashboardPage() {
    const { user } = useAuth();

    const [communities, setCommunities] =
        useState<Community[]>([]);

    const [items, setItems] = useState<Item[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [loadError, setLoadError] =
        useState<string | null>(null);

    const [refreshNumber, setRefreshNumber] = useState(0);

    useEffect(() => {
        const abortController = new AbortController();

        async function loadDashboard() {
            setIsLoading(true);
            setLoadError(null);

            try {
                const [
                    loadedCommunities,
                    loadedItems,
                ] = await Promise.all([
                    apiRequest<Community[]>(
                        "/api/communities",
                        {
                            signal: abortController.signal,
                        },
                    ),

                    apiRequest<Item[]>(
                        "/api/items/myItems",
                        {
                            signal: abortController.signal,
                        },
                    ),
                ]);

                setCommunities(loadedCommunities);
                setItems(loadedItems);
            } catch (error) {
                if (
                    error instanceof DOMException &&
                    error.name === "AbortError"
                ) {
                    return;
                }

                setLoadError(
                    "We could not load your catalogue. Please try again.",
                );
            } finally {
                if (!abortController.signal.aborted) {
                    setIsLoading(false);
                }
            }
        }

        void loadDashboard();

        return () => {
            abortController.abort();
        };
    }, [refreshNumber]);

    return (
        <div>
            <div className="flex flex-wrap items-start justify-between gap-4">
                <div>
                    <p className="text-sm font-semibold uppercase tracking-wider text-emerald-700">
                        Your dashboard
                    </p>

                    <h1 className="mt-2 text-3xl font-bold tracking-tight">
                        Hello, {user?.displayName}
                    </h1>
                </div>

                <button
                    type="button"
                    disabled={isLoading}
                    onClick={() =>
                        setRefreshNumber((current) => current + 1)
                    }
                    className="rounded-lg border border-stone-300 bg-white px-4 py-2 font-semibold text-stone-700 hover:bg-stone-100 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isLoading ? "Loading…" : "Refresh"}
                </button>
            </div>

            {loadError && (
                <div
                    role="alert"
                    className="mt-8 rounded-lg border border-red-200 bg-red-50 p-4 text-red-800"
                >
                    {loadError}
                </div>
            )}

            {isLoading && communities.length === 0 &&
                items.length === 0 && (
                    <p className="mt-8 text-stone-600">
                        Loading your catalogue…
                    </p>
                )}

            {!isLoading && !loadError && (
                <div className="mt-10 grid gap-12 lg:grid-cols-2">
                    <CommunitiesSection
                        communities={communities}
                    />

                    <ItemsSection items={items} />
                </div>
            )}
        </div>
    );
}

type CommunitiesSectionProps = {
    communities: Community[];
};

function CommunitiesSection({
                                communities,
                            }: CommunitiesSectionProps) {
    return (
        <section>
            <div className="flex items-center justify-between gap-4">
                <h2 className="text-2xl font-bold">
                    Your communities
                </h2>

                <Link
                    to="/communities/new"
                    className="text-sm font-semibold text-emerald-800 hover:underline"
                >
                    Create community
                </Link>
            </div>

            {communities.length === 0 ? (
                <div className="mt-5 rounded-xl border border-dashed border-stone-300 bg-white p-6">
                    <p className="font-medium">
                        You haven’t joined a community yet.
                    </p>

                    <p className="mt-2 text-sm text-stone-600">
                        Create one or follow an invitation from
                        someone you know.
                    </p>
                </div>
            ) : (
                <ul className="mt-5 space-y-3">
                    {communities.map((community) => (
                        <li key={community.id}>
                            <Link
                                to={`/communities/${community.id}`}
                                className="block rounded-xl border border-stone-200 bg-white p-5 transition hover:border-emerald-300 hover:shadow-sm"
                            >
                                <h3 className="font-semibold">
                                    {community.name}
                                </h3>

                                <p className="mt-1 text-sm text-stone-500">
                                    View shared catalogue
                                </p>
                            </Link>
                        </li>
                    ))}
                </ul>
            )}
        </section>
    );
}

type ItemsSectionProps = {
    items: Item[];
};

function ItemsSection({
                          items,
                      }: ItemsSectionProps) {
    return (
        <section>
            <div className="flex items-center justify-between gap-4">
                <h2 className="text-2xl font-bold">
                    Your items
                </h2>

                <Link
                    to="/items/new"
                    className="text-sm font-semibold text-emerald-800 hover:underline"
                >
                    Add item
                </Link>
            </div>

            {items.length === 0 ? (
                <div className="mt-5 rounded-xl border border-dashed border-stone-300 bg-white p-6">
                    <p className="font-medium">
                        Your catalogue is empty.
                    </p>

                    <p className="mt-2 text-sm text-stone-600">
                        Add something that you would be happy to
                        lend.
                    </p>
                </div>
            ) : (
                <ul className="mt-5 space-y-3">
                    {items.map((item) => (
                        <li
                            key={item.id}
                            className="rounded-xl border border-stone-200 bg-white p-5"
                        >
                            <h3 className="font-semibold">
                                {item.name}
                            </h3>

                            {item.description && (
                                <p className="mt-2 text-sm leading-6 text-stone-600">
                                    {item.description}
                                </p>
                            )}

                            {item.condition && (
                                <p className="mt-3 text-sm text-stone-500">
                                    Condition: {item.condition}
                                </p>
                            )}
                        </li>
                    ))}
                </ul>
            )}
        </section>
    );
}