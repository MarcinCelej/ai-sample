# Local containers: Testcontainers finds Podman; use 127.0.0.1 for SQL Server

## Context

Scaffold (#6), developed on Windows with Podman (no Docker Desktop). The Podman machine has limited RAM and also runs containers of other projects.

## Finding

- The run log said "Docker is not installed", but Testcontainers found the Podman socket. It started a SQL Server container (about 2 GB RAM) for the integration tests. The interrupted run left the container behind (label `org.testcontainers=true`). It starved the Podman machine: another project's SQL Server container crashed, and the dev SQL Server stopped accepting logins.
- With `Server=localhost,1433`, SqlClient timed out. On Windows, `localhost` resolves to IPv6 `::1` first, and Podman's port forwarding listens on IPv4 only.

## Resolution

- Removed the leftover container and restarted the crashed one.
- The dev connection string uses `Server=127.0.0.1,1433`.

## How to apply next time

- Do not run the integration tests (`ClassBook.IntegrationTests`) locally unless the human asks for it. CI runs them.
- If you do run them, check afterwards that no container labeled `org.testcontainers=true` is left running (`podman ps --filter label=org.testcontainers=true`), and remove leftovers.
- Do not assume "no Docker" means "no containers". Check `DOCKER_HOST` and Podman before claiming a test cannot start containers.
- Use `127.0.0.1`, not `localhost`, in local SQL Server connection strings.
