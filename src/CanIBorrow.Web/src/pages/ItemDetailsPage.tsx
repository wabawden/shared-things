import { useEffect, useState } from "react";
import { FaEdit } from "react-icons/fa";
import { Link, useLocation, useParams } from "react-router";
import { ApiError } from "../api/apiClient";
import {
    getItem,
    type ItemDetails,
} from "../api/items";
import { ItemPlaceholder } from "../components/ItemPlaceholder";
import { PagePlaceholder } from "../components/PagePlaceholder";

export type ItemNavigationState = {
    returnTo?: string;
    returnLabel?: string;
};

export function ItemDetailsPage() {
    const { itemId } = useParams();
    const [item, setItem] = useState<ItemDetails | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [notFound, setNotFound] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const location = useLocation();

    const navigationState =
        location.state as ItemNavigationState | null;

    const returnTo =
        navigationState?.returnTo ?? "/dashboard";

    const returnLabel =
        navigationState?.returnLabel ?? "Back to dashboard";
    
    useEffect(() => {
        if (!itemId) {
            setNotFound(true);
            setIsLoading(false);
            return;
        }

        const controller = new AbortController();

        async function loadItem() {
            try {
                setIsLoading(true);
                setError(null);

                const result = await getItem(
                    itemId!,
                    controller.signal,
                );

                setItem(result);
            } catch (caughtError) {
                if (controller.signal.aborted) {
                    return;
                }

                if (
                    caughtError instanceof ApiError &&
                    caughtError.status === 404
                ) {
                    setNotFound(true);
                    return;
                }

                setError(
                    "We couldn't load this item. Please try again.",
                );
            } finally {
                if (!controller.signal.aborted) {
                    setIsLoading(false);
                }
            }
        }

        void loadItem();

        return () => controller.abort();
    }, [itemId]);

    if (isLoading) {
        return (
            <PagePlaceholder
                title="Loading item..."
                description="Please wait while we find the item."
            />
        );
    }

    if (notFound || !item) {
        return (
            <PagePlaceholder
                title="Item not found"
                description="This item does not exist, or it isn't shared with one of your communities."
            />
        );
    }

    if (error) {
        return (
            <PagePlaceholder
                title="Something went wrong"
                description={error}
            />
        );
    }

    return (
        <main className="mx-auto w-full max-w-5xl px-4 py-8">
            <Link
                to={returnTo}
                className="mb-6 inline-flex text-sm font-semibold text-emerald-800 hover:underline"
            >
                ← {returnLabel}
            </Link>
            <div className="grid gap-8 md:grid-cols-2">
                <ItemPlaceholder className="aspect-square w-full rounded-2xl" />

                <section>
                    <p className="text-sm font-medium text-slate-500">
                        Shared by {item.owner.displayName}
                    </p>

                    <h1 className="mt-2 text-3xl font-bold text-slate-900">
                        {item.name}
                    </h1>

                    {item.condition && (
                        <p className="mt-3 inline-block rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-700">
                            Condition: {item.condition}
                        </p>
                    )}

                    <div className="mt-6">
                        <h2 className="font-semibold text-slate-900">
                            About this item
                        </h2>

                        <p className="mt-2 whitespace-pre-wrap text-slate-700">
                            {item.description ||
                                "No description has been added."}
                        </p>
                    </div>

                    <div className="mt-8">
                        {item.canEdit ? (
                            <Link
                                to={`/items/${item.id}/edit`}
                                state={navigationState}
                                className="inline-flex items-center gap-2 rounded-lg bg-slate-900 px-4 py-2 font-medium text-white hover:bg-slate-700"
                            >
                                <FaEdit aria-hidden="true" />
                                Edit item
                            </Link>
                        ) : (
                            <div className="rounded-xl bg-amber-50 p-4 text-amber-950">
                                <p className="font-semibold">
                                    Interested in borrowing this item?
                                </p>

                                <p className="mt-1">
                                    Contact {item.owner.displayName} to
                                    ask about borrowing it.
                                </p>
                            </div>
                        )}
                    </div>
                </section>
            </div>
        </main>
    );
}