# Lessons

Index of long-term lessons learned by agents and humans. Each entry links to a detailed note in `.ai/lessons/`.

Read this file before starting any task. Add a new entry after fixing a bug, finding a pitfall, or discovering an improvement.

## How to add a lesson

1. Create `.ai/lessons/YYYY-MM-DD-slug.md` with: **Context**, **Finding**, **Resolution**, **How to apply next time**.
2. Add a one-line entry below, newest first: `- YYYY-MM-DD [slug](lessons/YYYY-MM-DD-slug.md) – one-line summary`.

## Entries

- 2026-10-25 [local-containers-and-sql-host](lessons/2026-10-25-local-containers-and-sql-host.md) – Testcontainers uses Podman too: don't run integration tests locally unless asked, clean up leftovers; use `127.0.0.1` for local SQL Server
- 2026-10-25 [shadcn-cli-defaults](lessons/2026-10-25-shadcn-cli-defaults.md) – init with `--base radix`; after every `shadcn add`, repoint `cn` to `@/lib/utils` and uninstall the `cn` package
- 2026-10-25 [frontend-tooling-versions](lessons/2026-10-25-frontend-tooling-versions.md) – stay on ESLint 9 (jsx-a11y), pin majors named in the spec, MSW 3 and TS 6 API changes
- 2026-10-25 [culture-dependent-output](lessons/2026-10-25-culture-dependent-output.md) – server text uses the invariant culture; a Polish OS exposed `Money.ToString()` and FluentValidation messages
- 2026-10-25 [openapi-schema-names-and-numbers](lessons/2026-10-25-openapi-schema-names-and-numbers.md) – prefix nested slice types in schema ids and keep JSON numbers strict; read the `v1.json` diff
- 2026-10-25 [xunit-v3-mtp-coverage](lessons/2026-10-25-xunit-v3-mtp-coverage.md) – xUnit v3 4.x needs MTP (`global.json`) and `coverlet.MTP`; integration tests hang without Docker
