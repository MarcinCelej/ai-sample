import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config.ts'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['src/test/setup.ts'],
      include: ['src/**/*.test.{ts,tsx}'],
      coverage: {
        provider: 'v8',
        reporter: ['text', 'cobertura', 'html'],
        include: ['src/**/*.{ts,tsx}'],
        // Generated code, composition root and test helpers (see the spec's coverage exclusions).
        exclude: [
          'src/api/generated/**',
          'src/components/ui/**',
          'src/main.tsx',
          'src/test/**',
          'src/**/*.test.{ts,tsx}',
        ],
      },
    },
  }),
)
