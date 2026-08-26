import { useState } from "react";
import Modal from "./Modal";

type Props = {
    open: boolean;
    onClose: () => void;
    args: ConfirmationModalArgs;
};

export type ConfirmationModalArgs = {
    title: string;
    description: string;
    buttonText: string;
    onConfirm: () => void | Promise<void>;
    destructive?: boolean;
};

const ConfirmationModal = ({
    open,
    onClose,
    args,
}: Props) => {
    const [isSubmitting, setIsSubmitting] =
        useState(false);
    const [error, setError] =
        useState<string | null>(null);

    async function handleConfirm() {
        setIsSubmitting(true);
        setError(null);

        try {
            await args.onConfirm();
            onClose();
        } catch {
            setError(
                "Something went wrong. Please try again.",
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <Modal
            open={open}
            onClose={
                isSubmitting ? () => undefined : onClose
            }
            title={args.title}
            description={args.description}
        >
            {error && (
                <p
                    role="alert"
                    className="mt-4 text-sm text-red-700"
                >
                    {error}
                </p>
            )}

            <div className="mt-5 flex flex-row-reverse gap-3">
                <button
                    type="button"
                    onClick={() => void handleConfirm()}
                    disabled={isSubmitting}
                    className={
                        args.destructive
                            ? "flex-1 rounded-lg bg-red-700 px-5 py-3 font-semibold text-white hover:bg-red-800 disabled:cursor-not-allowed disabled:opacity-60"
                            : "flex-1 rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-60"
                    }
                >
                    {isSubmitting
                        ? "Please wait…"
                        : args.buttonText}
                </button>

                <button
                    onClick={onClose}
                    type="button"
                    disabled={isSubmitting}
                    className="flex-1 rounded-lg border border-stone-300 bg-white px-5 py-3 font-semibold text-stone-800 hover:bg-stone-50 disabled:cursor-not-allowed disabled:opacity-60"
                >
                    Cancel
                </button>
            </div>
        </Modal>
    );
};

export default ConfirmationModal;