import { Link } from "react-router";

export function PrivacyPage() {
    return (
        <article className="mx-auto max-w-3xl">
            <Link
                to="/"
                className="text-sm font-semibold text-emerald-800 hover:underline"
            >
                ← Back to Can I borrow..?
            </Link>

            <header className="mt-6">
                <p className="text-sm font-semibold uppercase tracking-wider text-emerald-700">
                    Privacy
                </p>

                <h1 className="mt-2 text-3xl font-bold tracking-tight">
                    Privacy and cookie notice
                </h1>

                <p className="mt-3 text-sm text-stone-500">
                    Last updated: 23 August 2026
                </p>
            </header>

            <div className="mt-10 space-y-10 text-stone-700">
                <NoticeSection title="About this service">
                    <p>
                        Can I borrow..? is an experimental,
                        non-commercial community lending service.
                        It helps people share information about
                        items they may be willing to lend within
                        private communities.
                    </p>

                    <p>
                        The service is currently being tested with
                        a small group of invited participants.
                    </p>
                </NoticeSection>

                <NoticeSection title="Who is responsible for your information?">
                    <p>
                        The person responsible for the use of your
                        personal information is:
                    </p>

                    <p className="rounded-lg bg-amber-50 p-4 font-medium text-amber-900">
                        Bill Bawden
                    </p>
                </NoticeSection>

                <NoticeSection title="Information we collect">
                    <p>
                        We may collect and store the following
                        information:
                    </p>

                    <ul className="list-disc space-y-2 pl-6">
                        <li>Your email address and display name.</li>
                        <li>
                            Your account identifier and securely
                            hashed password.
                        </li>
                        <li>
                            Information about items you add, including
                            their names, descriptions and condition.
                        </li>
                        <li>
                            Communities you create or join.
                        </li>
                        <li>
                            Community invitations you create or
                            accept.
                        </li>
                        <li>
                            Technical information such as request
                            logs, IP addresses, browser information
                            and error records where collected by the
                            application or hosting provider.
                        </li>
                    </ul>

                    <p>
                        Passwords are processed using ASP.NET Core
                        Identity and are not stored as readable
                        plain text.
                    </p>
                </NoticeSection>

                <NoticeSection title="How your information is used">
                    <p>We use this information to:</p>

                    <ul className="list-disc space-y-2 pl-6">
                        <li>Create and authenticate your account.</li>
                        <li>
                            Show your catalogue to members of your
                            communities.
                        </li>
                        <li>
                            Allow you to create, join and view private
                            communities.
                        </li>
                        <li>
                            Process community invitations.
                        </li>
                        <li>
                            Maintain the security and reliability of
                            the service.
                        </li>
                        <li>
                            Diagnose faults and improve the pilot.
                        </li>
                    </ul>
                </NoticeSection>

                <NoticeSection title="Who can see your information?">
                    <p>
                        Your email address is used for account
                        administration and is not intended to be
                        displayed to other community members.
                    </p>

                    <p>
                        Members of a community can see the display
                        names and shared item information belonging
                        to other members of that community.
                    </p>

                    <p>
                        People who do not share a community with you
                        should not be able to see your catalogue
                        through the service.
                    </p>

                    <p>
                        Anyone possessing a valid community
                        invitation link can use it to preview and
                        request membership of that community.
                        Invitation links should therefore only be
                        shared with intended participants.
                    </p>
                </NoticeSection>

                <NoticeSection title="Our reason for processing your information">
                    <p>
                        We process account, catalogue and community
                        information because it is necessary to
                        provide the service you request when you
                        create and use an account.
                    </p>

                    <p>
                        We may also process limited technical
                        information where necessary for our
                        legitimate interests in protecting,
                        maintaining and improving the service,
                        provided those interests do not override
                        your rights and interests.
                    </p>
                </NoticeSection>

                <NoticeSection title="Service providers">
                    <p>
                        We may use hosting and database providers to
                        operate the application. These providers
                        process information on our behalf to provide
                        infrastructure, storage, security and
                        diagnostic services.
                    </p>

                    <p>
                        This notice will be updated with details of
                        the production providers when the pilot is
                        deployed.
                    </p>
                </NoticeSection>

                <NoticeSection title="How long we keep information">
                    <p>
                        We retain account and catalogue information
                        while your account remains active and while
                        the pilot is operating.
                    </p>

                    <p>
                        You may ask us to delete your account and
                        associated information. Some limited
                        information may remain temporarily in
                        backups, security records or logs before
                        being automatically removed.
                    </p>
                </NoticeSection>

                <NoticeSection title="Essential authentication cookie">
                    <p>
                        Can I borrow..? uses an essential
                        authentication cookie named:
                    </p>

                    <p>
                        <code className="rounded bg-stone-200 px-2 py-1 text-sm">
                            shared-things-session
                        </code>
                    </p>

                    <p>
                        This cookie keeps you logged in and allows
                        the service to protect private items and
                        communities. It is not used for advertising,
                        analytics or cross-site tracking.
                    </p>

                    <p>
                        If you select “Keep me logged in”, the
                        cookie may remain on your device for up to
                        seven days, and its expiry may be renewed
                        while you use the service. Otherwise, it
                        normally expires when your browser session
                        ends.
                    </p>

                    <p>
                        You can remove the cookie by logging out or
                        clearing your browser cookies.
                    </p>

                    <p>
                        Because this cookie is strictly necessary
                        to provide the authenticated service, we do
                        not ask for consent before setting it.
                    </p>
                </NoticeSection>

                <NoticeSection title="Your rights">
                    <p>
                        Depending on the circumstances, UK data
                        protection law may give you rights to:
                    </p>

                    <ul className="list-disc space-y-2 pl-6">
                        <li>Ask for a copy of your information.</li>
                        <li>
                            Ask us to correct inaccurate information.
                        </li>
                        <li>
                            Ask us to delete your information.
                        </li>
                        <li>
                            Object to or restrict certain uses of your
                            information.
                        </li>
                        <li>
                            Make a complaint about how your
                            information has been handled.
                        </li>
                    </ul>

                    <p>
                        During the pilot, requests can be made using
                        the contact email listed in this notice.
                    </p>

                    <p>
                        You can also complain to the{" "}
                        <a
                            href="https://ico.org.uk/make-a-complaint/"
                            target="_blank"
                            rel="noreferrer"
                            className="font-semibold text-emerald-800 hover:underline"
                        >
                            Information Commissioner’s Office
                        </a>
                        .
                    </p>
                </NoticeSection>

                <NoticeSection title="Security">
                    <p>
                        We use authentication, authorization and
                        access controls intended to prevent users
                        from viewing communities they do not belong
                        to. Communication with the deployed service
                        will use HTTPS.
                    </p>

                    <p>
                        No online service can guarantee absolute
                        security. Please do not include sensitive
                        personal information in item descriptions
                        or community names.
                    </p>
                </NoticeSection>

                <NoticeSection title="Children">
                    <p>
                        Accounts in the initial pilot are intended
                        for adults aged 18 or over. The service is
                        not currently designed for children to
                        create or manage their own accounts.
                    </p>
                </NoticeSection>

                <NoticeSection title="Changes to this notice">
                    <p>
                        This notice may change as the pilot develops,
                        particularly when hosting providers or
                        optional features are added. The date at the
                        top of the page will be updated when
                        material changes are made.
                    </p>
                </NoticeSection>
            </div>
        </article>
    );
}

type NoticeSectionProps = {
    title: string;
    children: React.ReactNode;
};

function NoticeSection({
                           title,
                           children,
                       }: NoticeSectionProps) {
    return (
        <section>
            <h2 className="text-xl font-bold text-stone-900">
                {title}
            </h2>

            <div className="mt-4 space-y-4 leading-7">
                {children}
            </div>
        </section>
    );
}