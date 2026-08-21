function App() {
    return (
        <div className="min-h-screen bg-stone-50 text-stone-900">
            <header className="border-b border-stone-200 bg-white">
                <div className="mx-auto max-w-5xl px-6 py-5">
          <span className="text-xl font-semibold text-emerald-800">
            Can I borrow..?
          </span>
                </div>
            </header>

            <main className="mx-auto max-w-5xl px-6 py-16">
                <div className="max-w-2xl">
                    <p className="mb-3 text-sm font-semibold uppercase tracking-wider text-emerald-700">
                        Share locally
                    </p>

                    <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">
                        Useful things are closer than you think.
                    </h1>

                    <p className="mt-6 text-lg leading-8 text-stone-600">
                        Create a private catalogue of things you are happy to lend,
                        and share them with people you already know.
                    </p>

                    <button
                        type="button"
                        className="mt-8 rounded-lg bg-emerald-700 px-5 py-3 font-semibold text-white transition hover:bg-emerald-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-700"
                    >
                        Get started
                    </button>
                </div>
            </main>
        </div>
    );
}

export default App;