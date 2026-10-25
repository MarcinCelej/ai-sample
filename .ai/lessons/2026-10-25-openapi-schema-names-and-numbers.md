# OpenAPI: nested slice types collide and numbers become "integer | string"

## Context

Scaffold (#6): the first two slices (`CreateCourse`, `GetCourses`) generated the OpenAPI document for the Orval client.

## Finding

- Slice types are nested (`CreateCourse.Request`), and the default schema id uses only the simple type name, so every slice produced schemas named `Request` and `Response`.
- ASP.NET Core's web JSON defaults allow reading numbers from strings, so the document typed every number as `["integer", "string"]` with a pattern. The generated client would have typed numbers as `number | string`.

## Resolution

- `Program.cs` sets `CreateSchemaReferenceId` to prefix nested types with their slice: `CreateCourseRequest`, `PagedResponseOfGetCoursesItem`.
- `AddClassBookShared` sets `JsonNumberHandling.Strict`.

## How to apply next time

- After adding or changing an endpoint, read the diff of `backend/openapi/v1.json` before regenerating the client: check the schema names and the number types.
- Name slice types for their role in the slice (`Request`, `Response`, `Item`); the slice prefix makes them unique.
