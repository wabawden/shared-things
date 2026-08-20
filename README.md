# Can I borrow..?

**Can I borrow..?** is an early-stage community lending platform for sharing underused household items with people you already know.

The project explores a simple proposition:

> Would putting everyone’s underused belongings in one visible place make borrowing easier and encourage people to share more?

Rather than creating a public marketplace, the platform organises sharing around private communities such as neighbourhood groups, parent groups, friendship groups and repair communities.

Items remain the property of their owners. The platform does not currently support payments or commercial lending.

## Current project status

The project currently consists of a working .NET backend API with persistent PostgreSQL storage and integration tests.

A barebones React frontend and the first hosted pilot are the next milestone. The initial pilot will be shared with a small, selected group of users rather than released publicly.

The public-facing product name is **Can I borrow..?**. Some internal projects, namespaces and database configuration continue to use the original working name, **SharedThings**.

## Current user journeys

A user can currently:

* Register a persistent account.
* Log in using cookie authentication.
* Retrieve their authenticated account details.
* Log out.
* Add an item to their personal catalogue.
* View their own items.
* Create a community.
* Automatically become a member of a community they create.
* View the communities they belong to.
* View details for a specific community they belong to.
* View items owned by members of a community.
* Generate a shareable community invitation.
* Preview a valid invitation.
* Accept an invitation and join the community.
* Reopen an accepted invitation without creating a duplicate membership.

A user who is not a community member cannot view that community or its catalogue.

## Sharing model

Items belong to users, not communities.

In the current version, an item is automatically visible within every community its owner belongs to:

```text
User
├── owns Items
└── has Memberships
    └── belong to Communities
        └── display the Items owned by their members
```

This deliberately keeps the first sharing model simple. Selective visibility—for example, sharing a drill with a neighbourhood group but not a parent group—is outside the current milestone.

## Community invitations

Community invitations are designed to support sharing through existing channels such as WhatsApp.

The current journey is:

1. An existing community member creates an invitation.
2. The API returns a random invitation token.
3. The member shares a link containing that token.
4. Another authenticated user previews the invitation.
5. The user accepts it.
6. A membership is created.
7. The new member can view the community and its catalogue.

Invitations currently:

* are usable by multiple people;
* expire after seven days;
* store only a SHA-256 hash of the raw token;
* do not create membership during preview;
* can be accepted idempotently.

The raw invitation token is returned when the invitation is created but is not stored directly in the database.

## Technology

### Backend

* .NET 10
* ASP.NET Core Minimal APIs
* Entity Framework Core
* ASP.NET Core Identity
* Cookie authentication
* PostgreSQL
* Npgsql

### Testing

* xUnit
* `WebApplicationFactory`
* Testcontainers for PostgreSQL
* Respawn for database isolation

### Local tooling

* Docker Compose
* pgAdmin
* Insomnia

### Planned frontend

* React
* TypeScript
* Vite
* React Router

## API overview

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/me
```

### Items

```http
POST /api/items
GET  /api/items/myItems
GET  /api/items?communityId={communityId}
```

### Communities

```http
POST /api/communities
GET  /api/communities
GET  /api/communities/{communityId}
```

### Invitations

```http
POST /api/communities/{communityId}/invitations
GET  /api/invitations/{token}
POST /api/invitations/{token}/accept
```

### Operational endpoints

```http
GET /health
```

The health endpoint checks that the application can connect to PostgreSQL.

## Authentication and authorization

Production users authenticate using an ASP.NET Core Identity cookie.

The authentication cookie is:

* HTTP-only;
* secure in production;
* configured with `SameSite=Lax`;
* returned as `401 Unauthorized` or `403 Forbidden` for API requests rather than redirecting to an HTML login page.

Development and integration tests can authenticate using an `X-User-Id` header. This mechanism is enabled only in the `Development` and `Testing` environments and is ignored in production.

Authorization is applied at the resource level. Knowing a community GUID does not grant access to it.

When a community is missing or inaccessible, the API generally returns:

```http
404 Not Found
```

This avoids revealing whether a private community exists.

Registration and login endpoints also have basic rate limiting to reduce accidental or malicious abuse.

## Data model

The central entities are:

* `ApplicationUser`
* `Item`
* `Community`
* `Membership`
* `CommunityInvitation`

A membership links a user to a community using a composite key:

```text
(UserId, CommunityId)
```

This prevents the same user from holding duplicate memberships for the same community.

Items contain an `OwnerId` relationship to `ApplicationUser`. Owner display names are read through that relationship rather than duplicated on the item, avoiding data drift when a user’s details change.

## Local development

### Requirements

You will need:

* .NET 10 SDK
* Docker with Docker Compose
* an EF Core command-line tool installation
* optionally pgAdmin and Insomnia

### Start PostgreSQL

From the repository root:

```bash
docker compose up -d
```

The development PostgreSQL database and pgAdmin run as Docker containers.

Because PostgreSQL 18 uses version-specific data directories, the database volume is mounted at:

```text
/var/lib/postgresql
```

rather than `/var/lib/postgresql/data`.

### Configure the database

The development connection string is named:

```text
ConnectionStrings:SharedThings
```

Development-only secrets, including seeded user passwords, should be stored using .NET user secrets or other uncommitted local configuration.

Do not commit real passwords or production connection strings.

### Apply migrations

```bash
DOTNET_ENVIRONMENT=Development \
dotnet ef database update \
  --project src/SharedThings.Api \
  --startup-project src/SharedThings.Api
```

### Run the API

```bash
dotnet run --project src/SharedThings.Api
```

The exact development URL is shown in the console when the application starts.

The health endpoint can then be checked with:

```bash
curl http://localhost:5000/health
```

Adjust the port if the development profile uses a different one.

## Running the tests

Docker must be available because the integration suite starts a disposable PostgreSQL container.

Run the complete suite with:

```bash
dotnet test
```

The integration tests:

1. Start a PostgreSQL Testcontainer.
2. Apply the real EF Core migrations.
3. Reset the database before each test using Respawn.
4. Restore a predictable set of test users, memberships and items.
5. Exercise the API through `WebApplicationFactory`.

The test database is separate from the local development database.

## Development identities

The development and test environments use fixed users representing:

* Bill
* Alex
* Casey

The initial test community is **Our Neighbourhood**.

Bill and Alex are members. Casey is used to verify that non-members cannot access the community, and to test invitation acceptance.

These identities are development fixtures only and are not created in production.

## Architectural decisions

### Items are owned by users

A community never owns an item. Membership controls where a user’s items are currently visible.

### Communities are private

The platform does not provide public community discovery. Access requires membership or a valid invitation.

### Invitation tokens are bearer credentials

Anyone possessing an active invitation link may request to join its community. Raw tokens are therefore not stored in PostgreSQL.

### The backend remains a modular monolith

The current product does not require microservices. Authentication, communities, items and invitations remain in one deployable API and one relational database.

### The frontend and API will initially share one origin

The first deployment is intended to serve the compiled React application from ASP.NET Core.

This avoids unnecessary complexity around:

* cross-origin requests;
* cross-site authentication cookies;
* separate deployments;
* third-party-cookie restrictions.

The frontend can be separated later if there is a practical reason to do so.

## Next milestone

The next milestone is a small hosted pilot of **Can I borrow..?**

It will include a barebones React interface for:

* registration;
* login and logout;
* viewing personal items;
* adding an item;
* viewing communities;
* creating a community;
* viewing a community catalogue;
* creating and copying an invitation link;
* retaining an invitation through login or registration;
* previewing and accepting an invitation.

The first deployment will use a persistent hosted PostgreSQL database and will be shared with a small group of invited testers.

## Deliberately excluded from the first pilot

The following features are not part of the current milestone:

* item photographs;
* selective item visibility;
* borrowing requests;
* loan statuses;
* availability calendars;
* messaging;
* notifications;
* account recovery;
* email confirmation;
* profile editing;
* profile images;
* community roles;
* member removal;
* public search;
* payments;
* advertising;
* WhatsApp API integration;
* public IDs and readable slugs;
* polished visual design.

These features will be considered only after testing whether the core catalogue-and-community proposition is useful.

## Safety and responsibility

Items remain the property and responsibility of their owners. Borrowing arrangements are made directly between community members.

Users should not list illegal, hazardous or age-restricted items. The first pilot is an experimental community tool and not a commercial lending service.

## Project direction

The project is informed by ideas from the circular economy, repair culture and neighbourhood mutual aid.

Success does not depend on becoming a large public platform. A small, active community that finds the catalogue genuinely useful would be a meaningful result.
