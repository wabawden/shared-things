import {
    useEffect,
    useState,
    type FormEvent,
} from "react";
import {
    Link,
    useLocation,
    useNavigate,
    useParams,
} from "react-router";
import { ApiError } from "../api/apiClient";
import { getErrorMessages } from "../api/getErrorMessages";
import {
    getItem,
    updateItem,
    type ItemDetails,
} from "../api/items";
import { PagePlaceholder } from "../components/PagePlaceholder";
import {type ItemNavigationState} from "../pages/ItemDetailsPage.tsx"

export function EditItemPage() {
    const { itemId } = useParams();
    const navigate = useNavigate();

    const [item, setItem] =
        useState<ItemDetails | null>(null);

    const [name, setName] = useState("");
    const [description, setDescription] =
        useState("");
    const [condition, setCondition] =
        useState("");

    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] =
        useState(false);
    const [notFound, setNotFound] = useState(false);
    const [errors, setErrors] = useState<string[]>([]);

    const location = useLocation();

    const navigationState =
        location.state as ItemNavigationState | null;
    
    useEffect(() => {
        if (!itemId) {
            setNotFound(true);
            setIsLoading(false);
            return;
        }

        const controller = new AbortController();

        async function loadItem() {
            try {
                const result = await getItem(
                    itemId!,
                    controller.signal,
                );

                setItem(result);
                setName(result.name);
                setDescription(result.description);
                setCondition(result.condition);
            } catch (error) {
                if (controller.signal.aborted) {
                    return;
                }

                if (
                    error instanceof ApiError &&
                    error.status === 404
                ) {
                    setNotFound(true);
                    return;
                }

                setErrors(
                    getErrorMessages(
                        error,
                        "The item could not be loaded.",
                    ),
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

    async function handleSubmit(
        event: FormEvent<HTMLFormElement>,
    ) {
        event.preventDefault();

        if (!itemId) {
            return;
        }

        setErrors([]);
        setIsSubmitting(true);

        try {
            await updateItem(itemId, {
                name,
                description,
                condition,
            });

            navigate(`/items/${itemId}`, {
                replace: true,
                state: navigationState,
            });
        } catch (error) {
            setErrors(
                getErrorMessages(
                    error,
                    "The item could not be updated.",
                ),
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    if (isLoading) {
        return (
            <PagePlaceholder
                title="Loading item..."
                description="Please wait while we load your item."
            />
        );
    }

    if (notFound || !item) {
        return (
            <PagePlaceholder
                title="Item not found"
                description="This item does not exist, or you do not have access to it."
            />
        );
    }

    if (!item.canEdit) {
        return (
            <section className="mx-auto max-w-xl">
                <Link
                    to={`/items/${item.id}`}
                    state={navigationState}
                >
                    ← Back to item
                </Link>

                <h1 className="mt-6 text-3xl font-bold tracking-tight">
                    You cannot edit this item
                </h1>

                <p className="mt-3 text-stone-600">
                    Only {item.owner.displayName} can edit this
                    listing.
                </p>
            </section>
        );
    }

    return (
        <section className="mx-auto max-w-xl">
            <Link
                to={`/items/${item.id}`}
                className="text-sm font-semibold text-emerald-800 hover:underline"
            >
                ← Back to item
            </Link>

            <h1 className="mt-6 text-3xl font-bold tracking-tight">
                Edit item
            </h1>

            <p className="mt-3 text-stone-600">
                Correct or update the information shown to your
                communities.
            </p>

            {errors.length > 0 && (
                <ErrorList errors={errors} />
            )}

            <form
                onSubmit={handleSubmit}
                className="mt-8 space-y-6"
            >
                <div>
                    <label
                        htmlFor="itemName"
                        className="block font-medium"
                    >
                        Item name
                    </label>

                    <input
                        id="itemName"
                        type="text"
                        required
                        maxLength={100}
                        autoFocus
                        value={name}
                        onChange={(event) =>
                            setName(event.target.value)
                        }
                        className="mt-2 w-full rounded-lg border border-stone-300 bg-white px-3 py-2 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-200"
                    />
                </div>

                <div>
                    <label
                        htmlFor="itemDescription"
                        className="block font-medium"
                    >
                        Description{" "}
                        <span className="font-normal text-stone-500">
                            (optional)
                        </span>
                    </label>

                    <textarea
                        id="itemDescription"
                        rows={5}
                        maxLength={1000}
                        value={description}
                        onChange={(event) =>
                            setDescription(event.target.value)
                        }
                        className="mt-2 w-full resize-y rounded-lg border border-stone-300 bg-white px-3 py-2 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-200"
                    />

                    <div className="mt-2 text-right text-sm text-stone-500">
                        {description.length}/1000
                    </div>
                </div>

                <div>
                    <label
                        htmlFor="itemCondition"
                        className="block font-medium"
                    >
                        Condition{" "}
                        <span className="font-normal text-stone-500">
                            (optional)
                        </span>
                    </label>

                    <input
                        id="itemCondition"
                        type="text"
                        maxLength={100}
                        value={condition}
                        onChange={(event) =>
                            setCondition(event.target.value)
                        }
                        placeholder="For example, good or well used"
                        className="mt-2 w-full rounded-lg border border-stone-300 bg-white px-3 py-2 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-200"
                    />
                </div>

                <div className="flex flex-col-reverse gap-3 sm:flex-row">
                    <Link
                        to={`/items/${item.id}`}
                        className="rounded-lg border border-stone-300 px-5 py-3 text-center font-semibold hover:bg-stone-50"
                    >
                        Cancel
                    </Link>

                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className="flex-1 rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                        {isSubmitting
                            ? "Saving changes…"
                            : "Save changes"}
                    </button>
                </div>
            </form>
        </section>
    );
}

type ErrorListProps = {
    errors: string[];
};

function ErrorList({ errors }: ErrorListProps) {
    return (
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
    );
}