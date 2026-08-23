import {
    useState,
    type FormEvent,
} from "react";
import {
    Link,
    useNavigate,
} from "react-router";
import { apiRequest } from "../api/apiClient";
import { getErrorMessages } from "../api/getErrorMessages";
import type { Item } from "../types/entities";

export function CreateItemPage() {
    const navigate = useNavigate();

    const [name, setName] = useState("");
    const [description, setDescription] =
        useState("");
    const [condition, setCondition] =
        useState("");

    const [errors, setErrors] = useState<string[]>([]);
    const [isSubmitting, setIsSubmitting] =
        useState(false);

    async function handleSubmit(
        event: FormEvent<HTMLFormElement>,
    ) {
        event.preventDefault();

        setErrors([]);
        setIsSubmitting(true);

        try {
            await apiRequest<Item>("/api/items", {
                method: "POST",
                body: JSON.stringify({
                    name,
                    description:
                        description.trim() || null,
                    condition:
                        condition.trim() || null,
                }),
            });

            navigate("/dashboard", {
                replace: true,
            });
        } catch (error) {
            setErrors(
                getErrorMessages(
                    error,
                    "You must be logged in to add an item.",
                ),
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <section className="mx-auto max-w-xl">
            <Link
                to="/dashboard"
                className="text-sm font-semibold text-emerald-800 hover:underline"
            >
                ← Back to dashboard
            </Link>

            <h1 className="mt-6 text-3xl font-bold tracking-tight">
                Add an item
            </h1>

            <p className="mt-3 text-stone-600">
                Add something you would be happy to lend to
                people in your communities.
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

                    <p className="mt-2 text-sm text-stone-500">
                        For example, “Cordless drill” rather than
                        simply “Drill”.
                    </p>
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

                    <div className="mt-2 flex justify-between gap-4 text-sm text-stone-500">
            <span>
              Include useful accessories or lending
              information.
            </span>

                        <span>{description.length}/1000</span>
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

                <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm leading-6 text-amber-950">
                    Items remain your property and responsibility.
                    Do not list illegal, hazardous or
                    age-restricted items.
                </div>

                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="w-full rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isSubmitting
                        ? "Adding item…"
                        : "Add item"}
                </button>
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