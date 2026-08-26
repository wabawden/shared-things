import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router";
import { AuthProvider } from "./auth/AuthContext";
import "./index.css";
import App from "./App";
import { ModalProvider } from "./components/modals/ModalProvider";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <BrowserRouter>
            <AuthProvider>
                <ModalProvider>
                <App />
                </ModalProvider>
            </AuthProvider>
        </BrowserRouter>
    </StrictMode>,
);