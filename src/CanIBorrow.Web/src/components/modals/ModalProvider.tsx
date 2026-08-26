import {
    createContext,
    useContext,
    useState,
    type ReactNode,
} from "react";
import ConfirmationModal, {
    type ConfirmationModalArgs,
} from "./ConfirmationModal";

export type OpenModalArgs =
    | ({
          type: "confirmation";
      } & ConfirmationModalArgs);

type ModalContextValue = {
    openModal: (args: OpenModalArgs) => void;
    closeModal: () => void;
};

const ModalContext =
    createContext<ModalContextValue | undefined>(undefined);

export function ModalProvider({
    children,
}: {
    children: ReactNode;
}) {
    const [open, setOpen] = useState(false);
    const [modalArgs, setModalArgs] =
        useState<OpenModalArgs | null>(null);

    function openModal(args: OpenModalArgs) {
        setModalArgs(args);
        setOpen(true);
    }

    function closeModal() {
        setOpen(false);
    }

    return (
        <ModalContext.Provider
            value={{
                openModal,
                closeModal,
            }}
        >
            {children}

            {modalArgs?.type === "confirmation" && (
                <ConfirmationModal
                    open={open}
                    onClose={closeModal}
                    args={modalArgs}
                />
            )}
        </ModalContext.Provider>
    );
}

export function useModal() {
    const context = useContext(ModalContext);

    if (context === undefined) {
        throw new Error(
            "useModal must be used within ModalProvider",
        );
    }

    return context;
}