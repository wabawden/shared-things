import { Route, Routes } from "react-router";
import { AppLayout } from "./components/AppLayout";
import { PagePlaceholder } from "./components/PagePlaceholder";
import { RequireAuthentication } from "./auth/RequireAuthentication";
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";
import { DashboardPage } from "./pages/DashboardPage";
import { CommunityPage } from "./pages/CommunityPage";
import { PrivacyPage } from "./pages/PrivacyPage";
import { JoinCommunityPage } from "./pages/JoinCommunityPage";
import { CreateCommunityPage } from "./pages/CreateCommunityPage";
import { CreateItemPage } from "./pages/CreateItemPage";
import { ItemDetailsPage } from "./pages/ItemDetailsPage";
import { EditItemPage } from "./pages/EditItemPage";

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
                        element={<DashboardPage />}
                    />

                    <Route
                        path="items/new"
                        element={<CreateItemPage />}
                    />

                    <Route
                        path="communities/new"
                        element={<CreateCommunityPage />}
                    />

                    <Route
                        path="communities/:communityId"
                        element={<CommunityPage />}
                    />

                    <Route path="items/:itemId" element={<ItemDetailsPage />} />
                    <Route path="items/:itemId/edit" element={<EditItemPage />} />
                </Route>

                <Route
                    path="join/:token"
                    element={<JoinCommunityPage />}
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

            <Route
                path="privacy"
                element={<PrivacyPage />}
            />
        </Routes>
    );
}

export default App;