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
import { FaBoxOpen } from "react-icons/fa";
import {FaHandshakeSimple, FaPeopleGroup} from "react-icons/fa6";

function App() {
    return (
        <Routes>
            <Route element={<AppLayout />}>
                <Route
                    index
                    element={
                    <>
                        <section className="mx-auto grid max-w-6xl items-center gap-10 px-4 py-10 sm:px-6 sm:py-16 lg:grid-cols-2 lg:gap-14 lg:px-8">
                            <div>
                                <h1 className="max-w-xl text-4xl font-bold tracking-tight text-stone-900 sm:text-5xl">
                                    Useful things are closer than you think.
                                </h1>

                                <p className="mt-5 max-w-xl text-lg leading-8 text-stone-600">
                                    Create a private catalogue of things you are happy
                                    to lend, and share them with people you already know.
                                </p>

                                {/* Registration/login actions */}
                            </div>

                            <div className="overflow-hidden rounded-2xl bg-stone-100 shadow-sm">
                                <img
                                    src="homesplash.jpg"
                                    alt="Neighbours sharing useful household items"
                                    width={1920}
                                    height={1280}
                                    className="aspect-[4/3] w-full object-cover sm:aspect-[3/2] lg:aspect-[4/3]"
                                />
                            </div>
                        </section>
                        <section className="border-t border-stone-200 bg-stone-50">
                            <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6 sm:py-20 lg:px-8">
                                <div className="mx-auto max-w-3xl text-center">
                                    <h2 className="text-3xl font-bold tracking-tight text-stone-900 sm:text-4xl">
                                        Share more. Buy less.
                                    </h2>

                                    <p className="mt-5 text-lg leading-8 text-stone-600">
                                        Most of us own useful things that spend much of
                                        their time sitting in cupboards, sheds and garages.
                                        Can I borrow…? helps trusted groups of neighbours,
                                        friends and families make those things easier to
                                        share.
                                    </p>

                                    <p className="mt-4 leading-7 text-stone-600">
                                        Create a catalogue of items you are happy to lend,
                                        then invite people you already know to a private
                                        sharing community.
                                    </p>
                                </div>

                                <div className="mt-12 grid gap-6 sm:grid-cols-3">
                                    <article className="rounded-2xl border border-stone-200 bg-white p-6 shadow-sm">
                                        <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-100 text-emerald-800">
                                            <FaBoxOpen
                                                className="h-6 w-6"
                                                aria-hidden="true"
                                            />
                                        </div>

                                        <h3 className="mt-5 text-xl font-semibold text-stone-900">
                                            Add your things
                                        </h3>

                                        <p className="mt-3 leading-7 text-stone-600">
                                            List anything you would be comfortable lending,
                                            from tools and kitchen equipment to camping gear
                                            and garden furniture.
                                        </p>
                                    </article>

                                    <article className="rounded-2xl border border-stone-200 bg-white p-6 shadow-sm">
                                        <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-100 text-emerald-800">
                                            <FaPeopleGroup
                                                className="h-6 w-6"
                                                aria-hidden="true"
                                            />
                                        </div>

                                        <h3 className="mt-5 text-xl font-semibold text-stone-900">
                                            Create a trusted community
                                        </h3>

                                        <p className="mt-3 leading-7 text-stone-600">
                                            Bring together people who already know one another,
                                            such as neighbours, friends, relatives or members
                                            of an existing group.
                                        </p>
                                    </article>

                                    <article className="rounded-2xl border border-stone-200 bg-white p-6 shadow-sm">
                                        <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-100 text-emerald-800">
                                            <FaHandshakeSimple
                                                className="h-6 w-6"
                                                aria-hidden="true"
                                            />
                                        </div>

                                        <h3 className="mt-5 text-xl font-semibold text-stone-900">
                                            Borrow locally
                                        </h3>

                                        <p className="mt-3 leading-7 text-stone-600">
                                            Browse useful items nearby and contact their owners
                                            directly. No marketplace, delivery network or
                                            payment system required.
                                        </p>
                                    </article>
                                </div>
                            </div>
                        </section>
                    </>
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