import { Route, Routes } from "react-router";
import { AppLayout } from "./components/AppLayout";
import { PagePlaceholder } from "./components/PagePlaceholder";
import { RequireAuthentication } from "./auth/RequireAuthentication";
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";

function App() {
    return (
        <Routes>
            <Route element={<AppLayout />}>
                <Route
                    index
                    element={
                        <PagePlaceholder
                            title="Useful things are closer than you think."
                            description="Create a private catalogue of things you are happy to lend, and share them with people you already know."
                        />
                    }
                />

                <Route path="login" element={<LoginPage />} />
                <Route path="register" element={<RegisterPage />} />

                <Route element={<RequireAuthentication />}>
                    <Route
                        path="dashboard"
                        element={
                            <PagePlaceholder
                                title="Your dashboard"
                                description="Your communities and personal catalogue will appear here."
                            />
                        }
                    />

                    <Route
                        path="items/new"
                        element={
                            <PagePlaceholder
                                title="Add an item"
                                description="Add something that you would be happy to lend."
                            />
                        }
                    />

                    <Route
                        path="communities/new"
                        element={
                            <PagePlaceholder
                                title="Create a community"
                                description="Create a private sharing group."
                            />
                        }
                    />

                    <Route
                        path="communities/:communityId"
                        element={
                            <PagePlaceholder
                                title="Community"
                                description="Community catalogue and invitation controls."
                            />
                        }
                    />
                </Route>

                <Route
                    path="join/:token"
                    element={
                        <PagePlaceholder
                            title="Join a community"
                            description="Preview and accept a community invitation."
                        />
                    }
                />

                <Route
                    path="*"
                    element={
                        <PagePlaceholder
                            title="Page not found"
                            description="The page you requested does not exist."
                        />
                    }
                />
            </Route>
        </Routes>
    );
}

export default App;