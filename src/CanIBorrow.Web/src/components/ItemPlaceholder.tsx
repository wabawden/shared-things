import { FaBoxOpen } from "react-icons/fa";

type ItemPlaceholderProps = {
    className?: string;
};

export function ItemPlaceholder({
                                    className = "",
                                }: ItemPlaceholderProps) {
    return (
        <div
            className={`flex items-center justify-center bg-emerald-50 text-emerald-400 ${className}`}
            role="img"
            aria-label="No item image available"
        >
            <FaBoxOpen className="h-12 w-12" aria-hidden="true" />
        </div>
    );
}