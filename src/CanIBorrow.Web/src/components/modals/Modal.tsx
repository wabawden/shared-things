import { Fragment } from "react";
import { MdClose } from "react-icons/md";
import {
  Description,
  Dialog,
  DialogPanel,
  DialogTitle,
  Transition,
  TransitionChild,
} from "@headlessui/react";
import { cn } from "../../utils/cn";

type Props = {
  open: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: React.ReactNode;
  wide?: boolean;
  closeButton?: boolean;
};

const Modal = ({
  open,
  onClose,
  title,
  description,
  children,
  wide,
  closeButton,
}: Props) => {
  return (
    <Transition show={open} as={Fragment} appear>
      <Dialog onClose={onClose}>
        <TransitionChild
          as={Fragment}
          enter="ease-out duration-300"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="ease-in duration-200"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-black/30 z-40" aria-hidden="true" />
        </TransitionChild>
        <div className="fixed inset-0 flex items-center justify-center sm:p-4 z-40">
          <TransitionChild
            as={Fragment}
            enter="ease-out duration-300"
            enterFrom="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
            enterTo="opacity-100 translate-y-0 sm:scale-100"
            leave="ease-in duration-200"
            leaveFrom="opacity-100 translate-y-0 sm:scale-100"
            leaveTo="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
          >
            <DialogPanel
              className={cn(
                wide ? "max-w-3xl" : "max-w-sm",
                "relative mx-auto rounded-lg bg-white p-6 shadow-xl max-h-dvh sm:max-h-[95dvh] overflow-y-auto z-40"
              )}
            >
              {closeButton && (
                <div className="absolute top-0 right-0 pt-4 pr-4 block">
                  <button
                    type="button"
                    className="rounded-md bg-white text-gray-400 hover:text-gray-500 focus:outline-hidden focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                    onClick={onClose}
                  >
                    <span className="sr-only">Close</span>
                    <MdClose className="h-6 w-6" aria-hidden="true" />
                  </button>
                </div>
              )}
              <DialogTitle
                as="h3"
                className="text-lg leading-6 font-semibold text-gray-900"
              >
                {title}
              </DialogTitle>
              {description && (
                <Description className="text-sm text-gray-500 mt-2">
                  {description}
                </Description>
              )}
              {children}
            </DialogPanel>
          </TransitionChild>
        </div>
      </Dialog>
    </Transition>
  );
};

export default Modal;
