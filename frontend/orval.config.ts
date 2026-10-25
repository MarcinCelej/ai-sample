import { defineConfig } from 'orval'

export default defineConfig({
  classbook: {
    input: '../backend/openapi/v1.json',
    output: {
      target: 'src/api/generated/classbook.ts',
      schemas: 'src/api/generated/model',
      client: 'react-query',
      httpClient: 'fetch',
      mode: 'tags-split',
      override: {
        mutator: { path: 'src/api/http.ts', name: 'http' },
        // Hooks return the response body; non-2xx responses throw ApiError (see http.ts).
        fetch: { includeHttpResponseReturnType: false },
      },
    },
  },
})
