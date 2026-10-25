# Server output must not depend on the OS culture

## Context

Scaffold (#6), developed on a Windows machine with Polish culture.

## Finding

- `Money.ToString()` used string interpolation and printed `1234,50 PLN`. A unit test caught it.
- FluentValidation returned Polish validation messages, but the spec says API messages are English, for developers.

## Resolution

- `Money.ToString()` uses `string.Create(CultureInfo.InvariantCulture, ...)`.
- `AddClassBookShared` sets `ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture`.

## How to apply next time

- Format numbers and dates on the server with `CultureInfo.InvariantCulture`. User-facing formatting happens in the frontend (`lib/format.ts`) or in localized emails.
- Write at least one test that asserts formatted text, so culture-dependent output fails on any developer machine.
