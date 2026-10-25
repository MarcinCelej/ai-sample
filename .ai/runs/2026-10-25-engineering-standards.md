# Run: engineering standards spec

- **Issue:** #4
- **Branch:** `docs/4-engineering-standards`
- **Spec:** [.ai/specs/2026-10-25-engineering-standards.md](../specs/2026-10-25-engineering-standards.md)

## Task

Research and write the engineering standards spec. No implementation.

## Steps

1. Read `AGENTS.md`, `README.md`, `.ai/lessons.md` (no entries yet) and issue #4.
2. Got the human's approval for a plan covering the spec outline and key decisions.
3. Ran web checks on:
   - .NET 10 built-in Minimal API validation vs FluentValidation. Result: built-in validation covers attribute rules only, so FluentValidation is kept.
   - Orval vs `@hey-api/openapi-ts`. Result: Orval chosen for its mature TanStack Query output.
   - Licensing of MediatR, AutoMapper and FluentAssertions. Result: all three are commercial now, so they are excluded.
   - Testcontainers.MsSql + `WebApplicationFactory` setup.
4. Wrote the spec.

## Decisions and assumptions

- Single backend project with modules as folders; architecture tests enforce boundaries.
- SPA served from the same origin with cookie auth. This is recommended and listed as an open question.
- Backend coverage combines unit and integration tests. This is recommended and listed as an open question.
- No exact package versions are pinned in the spec; it states a versions policy instead.

## Verification

- Checked that all 10 requested topics are covered.
- Checked that the fixed decisions are unchanged.
- Checked for conflicts with `AGENTS.md`.

## Open items for the human

- Answer the open questions at the end of the spec.
- Review the spec and approve a commit, push and PR.

## Review outcome

The project owner reviewed the spec and resolved its open questions. The changes:

- **Backend structure:** changed from a single project to one project per module plus a thin host.
- **Data access:** one `DbContext` per module, each with its own schema and migrations, and no cross-schema FK constraints.
- **Local database:** an existing SQL Server instance via user-secrets, instead of a `docker-compose.yml` in the repository.
- **Coverage gate:** lines only, because `diff-cover` does not gate branches.
- **`Money`:** the constructor is private, so every value is rounded.
- **Remaining questions:** the hosting, email, bank format, minors' logins and retention questions are deferred to feature specs. All others are recorded under "Resolved decisions".
