# shadcn CLI: use the Radix base and keep cn() local

## Context

Scaffold (#6): `npx shadcn@latest init` in `frontend/`.

## Finding

- `shadcn init -d` now defaults to the Base UI preset (`base-nova`) and the Next.js template, not Radix as the spec says.
- The CLI installs the npm package `cn` (0.x, published days before) and generates `export { cn } from "cn"`. Every `shadcn add` imports `cn` from that package and reinstalls it.

## Resolution

- Initialized with `npx shadcn@latest init --base radix --preset nova --template vite`; `components.json` has `"style": "radix-nova"`.
- `src/lib/utils.ts` implements `cn()` with clsx + tailwind-merge. The `cn` package is uninstalled.

## How to apply next time

After every `npx shadcn@latest add <component>`:

1. Replace `import { cn } from "cn"` with `import { cn } from "@/lib/utils"` in the new files.
2. Run `npm uninstall cn` if the CLI added it back to `package.json`.
3. Check the new component imports from `radix-ui`, not `@base-ui/react`.
