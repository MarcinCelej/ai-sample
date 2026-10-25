import js from '@eslint/js'
import prettier from 'eslint-config-prettier'
import i18next from 'eslint-plugin-i18next'
import jsxA11y from 'eslint-plugin-jsx-a11y'
import reactHooks from 'eslint-plugin-react-hooks'
import globals from 'globals'
import tseslint from 'typescript-eslint'

const features = ['courses', 'families', 'enrollments', 'payments', 'auth']

/** A feature may not import from another feature; shared code goes to components/common or lib. */
const featureIsolation = features.map((feature) => ({
  files: [`src/features/${feature}/**`],
  rules: {
    'no-restricted-imports': [
      'error',
      {
        patterns: [
          {
            group: features.filter((f) => f !== feature).flatMap((f) => [`@/features/${f}/**`, `**/features/${f}/**`]),
            message: 'Features must not import from other features. Move shared code to components/common or lib.',
          },
        ],
      },
    ],
  },
}))

export default tseslint.config(
  {
    // Generated code: the API client (orval) and shadcn/ui components.
    ignores: ['dist', 'coverage', 'playwright-report', 'test-results', 'src/api/generated', 'src/components/ui'],
  },
  js.configs.recommended,
  tseslint.configs.strictTypeChecked,
  tseslint.configs.stylisticTypeChecked,
  {
    languageOptions: {
      globals: globals.browser,
      parserOptions: { projectService: true, tsconfigRootDir: import.meta.dirname },
    },
    rules: {
      // A leading underscore marks an intentionally unused name (e.g. a type parameter orval requires).
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_', varsIgnorePattern: '^_' }],
    },
  },
  {
    files: ['src/**/*.{ts,tsx}'],
    extends: [
      reactHooks.configs.flat['recommended-latest'],
      jsxA11y.flatConfigs.recommended,
      i18next.configs['flat/recommended'],
    ],
    rules: {
      'no-restricted-globals': [
        'error',
        { name: 'fetch', message: 'Use the generated API hooks. Only src/api/http.ts calls fetch.' },
      ],
    },
  },
  { files: ['src/api/http.ts'], rules: { 'no-restricted-globals': 'off' } },
  ...featureIsolation,
  {
    // Tests and tooling: literal strings are test data, not UI texts.
    files: ['src/**/*.test.{ts,tsx}', 'src/test/**', 'e2e/**', '*.config.{js,ts}'],
    rules: { 'i18next/no-literal-string': 'off' },
  },
  { files: ['*.config.js'], extends: [tseslint.configs.disableTypeChecked] },
  prettier,
)
