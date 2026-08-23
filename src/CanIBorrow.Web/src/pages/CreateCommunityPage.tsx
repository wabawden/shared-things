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
import type { Community } from "../types/entities";

export function CreateCommunityPage() {
    const navigate = useNavigate();

    const [name, setName] = useState("");
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
            const community =
                await apiRequest<Community>(
                    "/api/communities",
                    {
                        method: "POST",
                        body: JSON.stringify({ name }),
                    },
                );

            navigate(
                `/communities/${community.id}`,
                { replace: true },
            );
        } catch (error) {
            setErrors(
                getErrorMessages(
                    error,
                    "You must be logged in to create a community.",
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
                Create a community
            </h1>

            <p className="mt-3 text-stone-600">
                Create a private sharing space for people who
                already know one another.
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
                        htmlFor="communityName"
                        className="block font-medium"
                    >
                        Community name
                    </label>

                    <input
                        id="communityName"
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
                        For example, “Our Neighbourhood” or
                        “Saturday Repair Café”.
                    </p>
                </div>

                <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm leading-6 text-stone-700">
                    You will automatically become a member. After
                    creating the community, you can generate an
                    invitation link for other people.
                </div>

                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="w-full rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {isSubmitting
                        ? "Creating community…"
                        : "Create community"}
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