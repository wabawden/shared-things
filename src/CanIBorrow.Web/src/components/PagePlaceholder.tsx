type PagePlaceholderProps = {
    title: string;
    description: string;
};

export function PagePlaceholder({
    title,
    description,
    }: PagePlaceholderProps) {
    return (
        <section className="max-w-2xl">
            <h1 className="text-3xl font-bold tracking-tight">
                {title}
            </h1>

            <p className="mt-4 text-lg leading-8 text-stone-600">
                {description}
            </p>
        </section>
    );
}