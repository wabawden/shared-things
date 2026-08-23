export function getSafeReturnTo(search: string): string {
    const returnTo =
        new URLSearchParams(search).get("returnTo");

    if (
        returnTo &&
        returnTo.startsWith("/") &&
        !returnTo.startsWith("//")
    ) {
        return returnTo;
    }

    return "/dashboard";
}