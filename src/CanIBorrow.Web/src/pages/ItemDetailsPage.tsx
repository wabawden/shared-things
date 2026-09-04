import {
    useEffect,
    useState,
    type FormEvent,
} from "react";
import { FaEdit } from "react-icons/fa";
import { Link, useLocation, useNavigate, useParams } from "react-router";
import { ApiError, apiRequest } from "../api/apiClient";
import {
    getItem,
    uploadItemImage,
    type ItemDetails,
} from "../api/items";
import { getErrorMessages } from "../api/getErrorMessages";
import { ItemPlaceholder } from "../components/ItemPlaceholder";
import { PagePlaceholder } from "../components/PagePlaceholder";
import { useModal } from "../components/modals/ModalProvider";

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
    const [isUploadingImage, setIsUploadingImage] =
        useState(false);
    const [imageUploadError, setImageUploadError] =
        useState<string | null>(null);
    const [imageUploadSucceeded, setImageUploadSucceeded] =
        useState(false);
    const { openModal } = useModal();

    const navigate = useNavigate();

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

    function handleDeleteClick() {
        openModal({
            type: "confirmation",
            title: "Delete this item?",
            description:
                "This will permanently remove the item from your catalogue.",
            buttonText: "Delete item",
            destructive: true,
            onConfirm: async () => {
                if (item === null) {
                    return;
                }
                await apiRequest<void>(
                    `/api/items/${item.id}`,
                    {
                        method: "DELETE",
                    },
                );

                navigate("/dashboard", {
                    replace: true,
                });
            },
        });
    }

    async function handleImageUpload(
        event: FormEvent<HTMLFormElement>,
    ) {
        event.preventDefault();

        const form = event.currentTarget;
        const imageInput = form.elements.namedItem("image");

        if (
            !(imageInput instanceof HTMLInputElement) ||
            !imageInput.files?.[0]
        ) {
            setImageUploadError("Select an image to upload.");
            return;
        }

        setImageUploadError(null);
        setImageUploadSucceeded(false);
        setIsUploadingImage(true);

        try {
            await uploadItemImage(itemId!, imageInput.files[0]);
            form.reset();
            setImageUploadSucceeded(true);
        } catch (caughtError) {
            setImageUploadError(
                getErrorMessages(
                    caughtError,
                    "Sign in to upload an image.",
                )[0],
            );
        } finally {
            setIsUploadingImage(false);
        }
    }

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
        <section>
            <Link
                to={returnTo}
                className="mb-6 inline-flex text-sm font-semibold text-emerald-800 hover:underline"
            >
                ← {returnLabel}
            </Link>
            <div className="grid gap-8 md:grid-cols-2">
                {item.url ? <img src={item.url} className="aspect-square object-cover w-full rounded-2xl" /> :
                <ItemPlaceholder className="aspect-square w-full rounded-2xl" />}

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
                            <>
                                {!item.url && (
                            <form
                                onSubmit={handleImageUpload}
                                className="mb-6 rounded-xl border border-slate-200 p-4"
                            >
                                <label
                                    htmlFor="itemImage"
                                    className="block font-semibold text-slate-900"
                                >
                                    Item image
                                </label>
                                <p className="mt-1 text-sm text-slate-600">
                                    Upload a JPEG, PNG or WebP image up to 5 MB.
                                </p>
                                <input
                                    id="itemImage"
                                    name="image"
                                    type="file"
                                    accept="image/jpeg,image/png,image/webp"
                                    required
                                    className="mt-3 block w-full text-sm text-slate-700 file:mr-4 file:rounded-lg file:border-0 file:bg-slate-100 file:px-4 file:py-2 file:font-medium file:text-slate-900 hover:file:bg-slate-200"
                                />
                                {imageUploadError && (
                                    <p
                                        role="alert"
                                        className="mt-3 text-sm text-red-700"
                                    >
                                        {imageUploadError}
                                    </p>
                                )}
                                {imageUploadSucceeded && (
                                    <p
                                        role="status"
                                        className="mt-3 text-sm text-emerald-700"
                                    >
                                        Image uploaded successfully.
                                    </p>
                                )}
                                <button
                                    type="submit"
                                    disabled={isUploadingImage}
                                    className="mt-4 rounded-lg bg-emerald-700 px-4 py-2 font-medium text-white hover:bg-emerald-600 disabled:cursor-not-allowed disabled:opacity-60"
                                >
                                    {isUploadingImage
                                        ? "Uploading..."
                                        : "Upload image"}
                                </button>
                            </form>)}
                            <Link
                                to={`/items/${item.id}/edit`}
                                state={navigationState}
                                className="inline-flex items-center gap-2 rounded-lg bg-slate-900 px-4 py-2 font-medium text-white hover:bg-slate-700"
                            >
                                <FaEdit aria-hidden="true" />
                                Edit item
                            </Link>
                            <div
                                onClick={handleDeleteClick}
                                className="cursor-pointer ml-4 inline-flex items-center gap-2 rounded-lg bg-slate-900 px-4 py-2 font-medium text-white hover:bg-slate-700"
                            >
                                <FaEdit aria-hidden="true" />
                                Delete item
                            </div></>
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
        </section>
    );
}
