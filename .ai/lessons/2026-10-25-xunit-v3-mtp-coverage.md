# xUnit v3 4.x needs Microsoft.Testing.Platform and coverlet.MTP

## Context

Scaffold (#6): backend test projects with xUnit v3 (4.0.2) on the .NET 10 SDK, coverage with Coverlet as the spec required.

## Finding

- `dotnet test` failed with "Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK". xUnit v3 4.x runs only on Microsoft.Testing.Platform (MTP).
- `coverlet.collector` is a VSTest data collector and does nothing under MTP.
- Without Docker, the integration tests do not fail fast: Testcontainers keeps waiting for a Docker endpoint, and the run hangs.

## Resolution

- `backend/global.json` sets `"test": { "runner": "Microsoft.Testing.Platform" }`.
- Test projects reference `coverlet.MTP`; `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are not needed.
- Coverage: `dotnet test --coverlet --coverlet-output-format cobertura` writes `TestResults/coverage.cobertura.*.xml`.
- `dotnet test` uses `--project <path>`, not a positional path.

## How to apply next time

- Run a single test project with `dotnet test --project tests/<Project>`. Run `dotnet test` without `--project` only where Docker is available, because it includes the integration tests.
- If a test run hangs at "Starting: ClassBook.IntegrationTests", check that Docker is running.
