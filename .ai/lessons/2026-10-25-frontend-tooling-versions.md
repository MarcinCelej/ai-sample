# Frontend tooling: "latest" is not always compatible

## Context

Scaffold (#6): installing the frontend tooling with "latest stable" versions.

## Finding

- ESLint 10 is the latest, but `eslint-plugin-jsx-a11y` (required by the spec) and `eslint-plugin-import` support only up to ESLint 9. npm fails with ERESOLVE.
- `npm install react-router` installs v8, while the spec names React Router v7.
- The Vite template now ships oxlint instead of ESLint, and a tsconfig without `"strict": true`.
- MSW 3 renamed `onUnhandledRequest` to `onUnhandledFrame`. TypeScript 6 deprecates `baseUrl`; `paths` work without it.
- npm 11 blocks install scripts by default (`esbuild` postinstall). The native binary still installs as an optional dependency, so nothing broke.

## Resolution

- ESLint 9 with `@eslint/js` 9. Feature isolation uses ESLint's built-in `no-restricted-imports` instead of `eslint-plugin-import`.
- React Router pinned to `^7`.
- oxlint removed, `strict` and `noUncheckedIndexedAccess` set explicitly.

## How to apply next time

- When the spec names a major version, install it explicitly (`pkg@7`). Upgrading a major is its own issue.
- Before upgrading ESLint to 10, check that `eslint-plugin-jsx-a11y`, `eslint-plugin-react-hooks` and `eslint-plugin-i18next` declare support for it.
