# Shared Things

A private catalogue of useful items that members of an existing community are
willing to lend.

The first vertical slice tests the foundational proposition: whether putting
everyone's underused belongings in one visible place feels useful and prompts
participation.

## Current scope

- Development-only header authentication
- List the authenticated user's communities
- View a community catalogue only as a member
- Seeded users, membership and items held in memory
- Integration tests for authentication and community isolation

## Run

Requires the .NET 10 SDK.

```bash
dotnet restore
dotnet run --project src/SharedThings.Api
```

The development authentication handler accepts an `X-User-Id` header. The
seeded users are:

| User | ID | Neighbourhood member |
| --- | --- | --- |
| Bill | `10000000-0000-0000-0000-000000000001` | Yes |
| Alex | `10000000-0000-0000-0000-000000000002` | Yes |
| Casey | `10000000-0000-0000-0000-000000000003` | No |

List Bill's communities:

```bash
curl -H 'X-User-Id: 10000000-0000-0000-0000-000000000001' \
  http://localhost:5000/api/communities
```

View the neighbourhood catalogue as a member:

```bash
curl -H 'X-User-Id: 10000000-0000-0000-0000-000000000002' \
  http://localhost:5000/api/communities/20000000-0000-0000-0000-000000000001/items
```

Casey receives `404 Not Found` for the same catalogue so its existence is not
disclosed to a non-member.

## Test

```bash
dotnet test
```

## Authentication direction

`X-User-Id` is only a temporary development mechanism. It creates a standard
claims-based ASP.NET Core identity so endpoint policies and membership rules do
not depend on it. It can later be replaced by ASP.NET Core Identity or an
OpenID Connect provider without changing the community authorization policy.
