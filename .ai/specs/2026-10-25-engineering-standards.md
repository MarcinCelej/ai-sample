# Engineering standards

- **Issue:** #4
- **Status:** Accepted – reviewed by the project owner
- **Date:** 2026-10-25

## Goal

Define the binding engineering rules for ClassBook: structure, naming, patterns, data access, errors, API, frontend, security, testing and tooling. Agents and humans follow these rules for the scaffold and every feature after it. Where this spec and code disagree, fix one of them in the same change.

## Scope and non-goals

- **In scope:** conventions, patterns, library choices, quality gates.
- **Not in scope:** the scaffold itself (separate issue), feature specs (courses, enrollments, payments, …), hosting and deployment pipeline (see [Deferred decisions](#deferred-decisions)).

## Fixed decisions

These were decided by the project owner. This spec refines them and does not change them.

- **Backend:** ASP.NET Core Web API on .NET 10, EF Core, SQL Server, and built-in DI. It is a modular monolith with vertical slices, grouped into the modules Courses, Families, Enrollments, Payments and Identity.
- **Frontend:** React, TypeScript, Vite, Tailwind and shadcn/ui, in `frontend/`. It is a SPA that calls the API through a client generated from OpenAPI.
- **Repository:** a monorepo with `backend/`, `frontend/` and `.ai/`.
- **Tests:**
  - xUnit with Testcontainers.MsSql for integration tests
  - Vitest with Testing Library
  - Playwright for end-to-end tests
  - 100% coverage of new code, enforced in CI
- **Tenancy:** one organization at start; the data model and code are ready for multi-tenancy.
- **Authentication:** email + password, plus Google and Microsoft sign-in. Roles are administrator, instructor, guardian and participant.
- **Localization:** the UI is in Polish, with i18n from day one.
- **Payments:** money is in PLN, and payments come from imported bank statements.

---

## 1. Solution and folder structure

### Repository

```
/
├── backend/
│   ├── ClassBook.slnx
│   ├── Directory.Build.props        Shared MSBuild settings (nullable, warnings as errors, analyzers)
│   ├── Directory.Packages.props     Central package versions
│   ├── openapi/v1.json              Generated OpenAPI document (committed)
│   ├── src/
│   │   ├── ClassBook.Api/           Host: Program.cs, composition, serves the built SPA
│   │   ├── ClassBook.Shared/        Cross-cutting building blocks, references no module
│   │   └── Modules/
│   │       ├── ClassBook.Modules.Courses/
│   │       ├── ClassBook.Modules.Families/
│   │       ├── ClassBook.Modules.Enrollments/
│   │       ├── ClassBook.Modules.Payments/
│   │       └── ClassBook.Modules.Identity/
│   └── tests/
│       ├── ClassBook.UnitTests/
│       ├── ClassBook.IntegrationTests/
│       └── ClassBook.ArchitectureTests/
├── frontend/
├── .github/workflows/
├── .ai/
├── .editorconfig
├── AGENTS.md
└── README.md
```

### Backend: one project per module

Each module is its own class library. `ClassBook.Api` is a thin host that composes the modules. Within a module, there are no layer projects: a module is organized by slices.

**Rationale:** project references let the compiler enforce module boundaries, and `internal` hides module internals. One project per module, not per layer, keeps the ceremony low.

```
ClassBook.Api/
├── Program.cs                       Calls Add<X>Module() / Map<X>Endpoints() for every module
└── wwwroot/                         Built SPA (copied at publish, not committed)

ClassBook.Shared/
├── Persistence/                     ModuleDbContext base, interceptors (tenant, audit), OutboxMessage
├── Tenancy/                         ITenantContext, ITenantOwned
├── Errors/                          Result, Error, ProblemDetails mapping
├── Validation/                      Validation endpoint filter
├── Paging/                          PagedResponse<T>, paging rules and query helper
├── Domain/                          Money
├── Security/                        Policies, roles, current user, CSRF middleware
├── Email/                           Outbox dispatcher + sender
└── Time/                            Clock helpers (Europe/Warsaw)

ClassBook.Modules.Courses/
├── CoursesModule.cs                 public: AddCoursesModule() + MapCoursesEndpoints()
├── Public/                          public: the only types other modules may use
├── Domain/                          internal: entities, value objects, domain logic
├── Persistence/                     internal: CoursesDbContext, configurations, Migrations/
├── CreateCourse/                    internal: one folder per use case (slice)
│   └── CreateCourse.cs
└── GetCourse/
```

### Module rules

- **Visibility:** only `<X>Module.cs` and the types in `Public/` are `public`. Everything else in a module is `internal`, except the migrations EF Core generates as `public` in `Persistence/Migrations`. Test projects get access through `InternalsVisibleTo`, configured once in `Directory.Build.props`.
- Module projects set `DisableTransitiveProjectReferences`, so a module can use only the modules it references directly.
- Allowed dependencies form an acyclic graph, expressed as project references. A module references only the modules it depends on and uses only their `Public/` types.

  | Module      | May depend on              |
  |-------------|----------------------------|
  | Identity    | –                          |
  | Courses     | –                          |
  | Families    | Identity                   |
  | Enrollments | Courses, Families          |
  | Payments    | Families, Enrollments      |

- Every module references `ClassBook.Shared`. `ClassBook.Shared` references no module. Only `ClassBook.Api` references all modules.
- `Public/` contains only:
  - read-only query services, e.g. `ICourseCatalog.GetCapacity(courseId)`
  - IDs and DTOs
  - enums

  It contains no entities and no mutating access to another module's data.
- A module never touches another module's `DbContext` or tables. EF navigation properties across modules are not possible and not wanted. Store the other module's ID only, with no cross-schema FK constraint. Referential integrity across modules is the owning handler's job, done via `Public/` services.
- The compiler enforces references and visibility. `ClassBook.ArchitectureTests` (ArchUnitNET) adds the rest: `public` types only in `Public/` or the module entry class, naming rules, and tenant filters.
- Adding a new edge to the module graph needs a spec change.

### Frontend

```
frontend/
├── orval.config.ts
├── src/
│   ├── main.tsx
│   ├── app/                 Router, providers (QueryClient, i18n, auth), layouts
│   ├── api/
│   │   ├── generated/       Orval output – never edited by hand
│   │   └── http.ts          Custom fetch mutator (base URL, CSRF header, ProblemDetails)
│   ├── features/
│   │   ├── courses/
│   │   │   ├── pages/       Route components (CourseListPage.tsx)
│   │   │   ├── components/  Feature-specific components
│   │   │   └── forms/       Form components + Zod schemas
│   │   ├── families/
│   │   ├── enrollments/
│   │   ├── payments/
│   │   └── auth/
│   ├── components/
│   │   ├── ui/              shadcn/ui components (generated, minimal edits)
│   │   └── common/          Shared app components (PageHeader, MoneyText, …)
│   ├── lib/                 Utilities (format.ts, utils.ts with cn(), problem-details.ts, i18n.ts)
│   └── locales/pl/          Translation JSON per namespace
└── e2e/                     Playwright tests
```

- A feature may import from `components/`, `lib/` and `api/`, and from its own folder.
- A feature must not import from another feature's folder. If two features need the same thing, move it to `components/common` or `lib`. ESLint enforces this with `no-restricted-imports` patterns per feature folder.

---

## 2. Naming conventions

### C#

| Element | Rule | Example |
|---|---|---|
| Namespace | Follows the project and folder path | `ClassBook.Modules.Courses.CreateCourse` |
| File | One top-level type per file, named after it. Exception: a slice file holds its own nested types. | `CreateCourse.cs` |
| Slice | `<Verb><Noun>`, imperative | `EnrollParticipant`, `ImportBankStatement`, `GetFamilyBalance` |
| Classes, records, methods, properties | PascalCase | `WaitingListEntry` |
| Interfaces | `I` prefix | `ITenantContext` |
| Private fields | `_camelCase` | `_dbContext` |
| Async methods | `Async` suffix, take a `CancellationToken` as last parameter | `HandleAsync(…, CancellationToken ct)` |
| Constants and static readonly | PascalCase | `MaxPageSize` |
| Test classes | `<Subject>Tests`; methods `Method_Scenario_ExpectedResult` | `Enroll_WhenCourseFull_AddsToWaitingList` |

### Database

- There is one schema per module: `courses`, `families`, `enrollments`, `payments` and `identity`.

  **Rationale:** module ownership is visible in the database, and a later split stays possible.
- Tables are PascalCase and singular, e.g. `courses.Course` and `enrollments.WaitingListEntry`. The name is set explicitly in configuration.
- Columns are PascalCase and match property names.
- Keys and constraints:
  - Primary key: `Id`
  - Foreign key: `<Entity>Id`
  - Index: `IX_<Table>_<Columns>`, the EF default
- Enums are stored as strings with `HasConversion<string>()` and a `MaxLength`.

  **Rationale:** the data is readable, and reordering enum members can't corrupt it.

### API

- Routes are `/api/<resource>`, with resources in plural kebab-case and nesting at most one level deep.
  - `GET /api/courses`, `GET /api/courses/{courseId}`, `POST /api/courses/{courseId}/enrollments`
  - Commands that don't map to CRUD are a sub-resource verb: `POST /api/enrollments/{id}/approve`
- JSON is camelCase, the System.Text.Json default. Enums are strings.
- The OpenAPI `operationId` equals the slice name, e.g. `CreateCourse`, so generated hooks read `useCreateCourse` and `useGetCourse`.

### Frontend

| Element | Rule | Example |
|---|---|---|
| Component files and components | PascalCase `.tsx` | `CourseCard.tsx` |
| Route components | `<Name>Page` | `CourseDetailPage` |
| Hooks | `useX`, camelCase file | `useCurrentUser.ts` |
| Other modules | kebab-case `.ts` | `problem-details.ts` |
| Folders | kebab-case | `features/waiting-list` |
| URL routes | English, kebab-case, plural resources | `/courses/:courseId`, `/family/members` |
| i18n keys | `namespace:area.key`, camelCase segments | `courses:list.emptyState` |
| Tests | `*.test.ts(x)` next to the file; E2E `e2e/*.spec.ts` | `CourseCard.test.tsx` |

---

## 3. Patterns

### Feature slice

A use case lives in one file: request, response, validator, handler and endpoint mapping. The handler is a plain class that uses its module's `DbContext` directly. Domain rules live on entities.

```csharp
namespace ClassBook.Modules.Courses.CreateCourse;

internal static class CreateCourse
{
    public sealed record Request(string Name, string? Description, int Capacity, decimal MonthlyFee);
    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Capacity).GreaterThan(0);
            RuleFor(x => x.MonthlyFee).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        }
    }

    public sealed class Handler(CoursesDbContext db)
    {
        public async Task<Result<Response>> HandleAsync(Request request, CancellationToken ct)
        {
            var course = Course.Create(request.Name, request.Description, request.Capacity, Money.Pln(request.MonthlyFee));
            db.Courses.Add(course);
            await db.SaveChangesAsync(ct);
            return new Response(course.Id);
        }
    }

    public static void MapEndpoint(RouteGroupBuilder group) =>
        group.MapPost("/", async (Request request, Handler handler, CancellationToken ct) =>
                (await handler.HandleAsync(request, ct)).ToCreatedResult(r => $"/api/courses/{r.Id}"))
            .WithName(nameof(CreateCourse))
            .WithValidation<Request>()
            .RequireAuthorization(Policies.ManageCourses)
            .Produces<Response>(StatusCodes.Status201Created);
}
```

The module registration file wires up the module:

```csharp
public static class CoursesModule
{
    public static IServiceCollection AddCoursesModule(this IServiceCollection services) => services
        .AddModuleDbContext<CoursesDbContext>()        // Shared helper: connection string, interceptors, schema-scoped history table
        .AddScoped<CreateCourse.Handler>()
        .AddScoped<ICourseCatalog, CourseCatalog>();

    public static IEndpointRouteBuilder MapCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");
        CreateCourse.MapEndpoint(group);
        GetCourse.MapEndpoint(group);
        return app;
    }
}
```

### Rules

- **Endpoints:** use Minimal APIs, with one `MapGroup` per module.

  **Rationale:** less ceremony than controllers, and it fits slices naturally.
- **Handlers:**
  - Handlers are registered explicitly as scoped services, not by assembly scanning, so registrations stay greppable.
  - A handler returns `Result` or `Result<T>` for expected failures and throws only for bugs.
  - Handlers don't call other handlers. Shared logic goes in the domain or in a module-internal service.
- **Domain:**
  - Entities protect their invariants: private setters, factory methods, behavior methods such as `course.Enroll(participant)` and `family.ApplyPayment(payment)`.
  - Use rich domain logic where rules are real: capacity, waiting list, dues allocation. Simple reference data can stay anemic.
- **Reads:** project directly to response DTOs with `Select(...)` and `AsNoTracking()`. Don't load entities just to map them.
- **Mapping:** write it by hand, in the slice, next to the response.
- **Background work:** use `BackgroundService` with `PeriodicTimer` for scheduled jobs such as overdue reminders and the email outbox. Add Hangfire or Quartz only when a spec needs it.
- **Emails:** write emails to an outbox table in the same transaction. Every module `DbContext` maps the shared `OutboxMessage` entity into its own schema, e.g. `enrollments.OutboxMessage`, and one background dispatcher sends from all of them.

  **Rationale:** a rollback never sends an email, and a failed send is retried.
- **Cross-module operations:** one request writes to one module's `DbContext`. If a use case must change data in two modules atomically, its spec must justify it and use one explicit transaction shared by both contexts (`Database.UseTransaction`).

### Deliberately not used

| Not used | Why |
|---|---|
| MediatR / mediator pipelines | Indirection without benefit in a monolith; commercially licensed since v13. A direct handler call is easier to follow and debug. |
| AutoMapper / mapping libraries | Hides mapping errors until runtime; commercially licensed. Manual mapping is explicit and refactor-safe. |
| Repository / Unit of Work over EF Core | `DbContext` already is both. Wrapping it removes LINQ, projections and change tracking for no gain. |
| Clean/Onion architecture layer projects | Spreads one feature over 4 projects. Slices keep a feature together. |
| CQRS buses, separate read database, event sourcing | Not needed at this scale. Commands and queries are simply separate slices. |
| Generic CRUD base classes or services | Couple unrelated features and grow flags. Duplicate small things; extract only after the third repetition. |
| Exceptions for business outcomes | Use `Result`. Exceptions are for bugs and infrastructure failures. |
| In-process domain event dispatch across modules | Hidden control flow. Call the other module's `Public/` service explicitly. Revisit if real decoupling is needed. |
| Generic soft delete | See [Data access](#4-data-access). |

---

## 4. Data access

### EF Core

- There is one `DbContext` per module: `CoursesDbContext`, `FamiliesDbContext`, `EnrollmentsDbContext`, `PaymentsDbContext`, and `IdentityDbContext`. Each is `internal`, lives in the module's `Persistence/` folder and implements `IModuleDbContext` (its schema). All but Identity derive from `ModuleDbContext` in `ClassBook.Shared`, which applies tenant filters, sets the default schema, and applies the configurations from its own assembly.
- `IdentityDbContext` derives from the ASP.NET Core Identity context instead, because C# allows one base class and Identity tables are not tenant-owned. It sets the `identity` schema itself and has no tenant filter.
- Every context is registered with `AddModuleDbContext<T>()`, which sets the connection string, the interceptors and the schema-scoped migrations history table.
- All contexts use the same database and connection string, each in its own schema.
- Every entity has an `IEntityTypeConfiguration<T>` in its module's `Persistence/` folder.
- Configure everything explicitly in configuration classes, not with data annotations on entities: table and schema, max lengths on every string, precision on every decimal, indexes, relationships.
- Primary keys are `Guid`, using EF Core's default client-side value generator for SQL Server, which produces sequential GUIDs.

  **Rationale:** IDs exist before saving and aren't enumerable in URLs, and sequential values avoid index fragmentation. Don't use `Guid.CreateVersion7()`, because SQL Server's GUID sort order defeats it.
- `DbSet` properties are `public DbSet<Course> Courses => Set<Course>();` and are grouped by module.
- No lazy loading. Use explicit `Include` or projections.
- **Concurrency:** use a `rowversion` concurrency token (`byte[] Version`) on aggregates edited concurrently: `Course`, which holds capacity, and `FamilyAccount`, which holds the balance. A `DbUpdateConcurrencyException` maps to `409 Conflict`.
- **Transactions:** one `SaveChangesAsync` per request is the unit of work. Use an explicit transaction only when a handler must save twice.

### Migrations workflow

- Migrations are per module, in `Persistence/Migrations` of the module project. Each context keeps its own history table in its schema, e.g. `courses.__EFMigrationsHistory`. Create them from `backend/`:
  `dotnet ef migrations add <PascalCaseName> --project src/Modules/ClassBook.Modules.Courses --startup-project src/ClassBook.Api --context CoursesDbContext --output-dir Persistence/Migrations`
  Design-time tools need no connection string (`EF.IsDesignTime`); at runtime a missing one fails with a clear message.
- Every migration is reviewed in the PR, including the generated SQL (`dotnet ef migrations script`). Data loss, such as dropping or renaming columns, must be called out in the PR description.
- Never edit a migration that has been merged to `main`. Add a new one.
- **Development:** the API applies the migrations of all module contexts at startup in the `Development` environment only.
- **CI and deployed environments:** apply an idempotent script or a migration bundle as a separate pipeline step, never at application startup.
- **Integration tests:** apply migrations to the Testcontainers database, not `EnsureCreated`, so migrations are tested too.
- **Seed data:**
  - reference data goes through `HasData`
  - demo or test data comes from a dev-only seeder, never from migrations

### Tenant scoping

All business entities are tenant-owned from day one. Today there is a single default tenant.

```csharp
public interface ITenantOwned { Guid TenantId { get; } }

public interface ITenantContext { Guid TenantId { get; } }

// Now: returns the single configured tenant. Later: resolve from host name or user claim.
internal sealed class SingleTenantContext(IOptions<TenancyOptions> options) : ITenantContext
{
    public Guid TenantId => options.Value.DefaultTenantId;
}
```

```csharp
// ModuleDbContext.OnModelCreating – applied to every ITenantOwned entity of the module (EF Core 10 named filters)
foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(e => typeof(ITenantOwned).IsAssignableFrom(e.ClrType)))
    entity.SetTenantFilter(TenantFilter, () => _tenant.TenantId);   // helper in ClassBook.Shared
```

- Every entity except `Tenant` and Identity framework tables implements `ITenantOwned`.
- A `SaveChanges` interceptor sets `TenantId` on added entities. Code never sets it manually.
- **Tests:**
  - An architecture/unit test asserts that every `ITenantOwned` entity has the tenant filter.
  - An integration test asserts that data from another tenant is invisible.
- Unique indexes on tenant-owned tables include `TenantId`, e.g. `(TenantId, Email)`.
- `IgnoreQueryFilters()` is forbidden outside `ClassBook.Shared` and admin tooling explicitly approved in a spec.

### Auditing

- Every entity has `CreatedAt`, `CreatedBy`, `UpdatedAt` and `UpdatedBy`, via an `IAuditable` interface. A `SaveChanges` interceptor fills them from `TimeProvider` and the current user.
- **Financial records are append-only:** payments, statement imports, allocations and dues. They are never updated or deleted. A correction is a reversing entry with a reference to the original.

  **Rationale:** balances must be reproducible from history.
- A full change-history audit log is out of scope until a spec needs it.

### Soft delete: no

- There is no generic `IsDeleted` flag and no soft-delete query filter.
- Lifecycle is modelled explicitly as status: `Course.Status = Archived`, `Enrollment.Status = Cancelled`, `FamilyMember.IsActive`.
- Deleting is a real delete, allowed only where no financial history depends on the row.
- GDPR erasure anonymizes personal fields instead of deleting rows that financial history depends on (see [Security](#8-security)).

**Rationale:** explicit states carry business meaning, and soft delete leaks into every query and unique index.

### Money

```csharp
public readonly record struct Money
{
    public const string Currency = "PLN";
    public decimal Amount { get; }
    private Money(decimal amount) => Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
    public static Money Pln(decimal amount) => new(amount);
    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount);
}
```

- Money is always `decimal` and never `double` or `float`. It is stored as `decimal(18,2)`, mapped via a value converter or complex property.
- PLN only. There is no currency column now. Adding currency later means a `Currency` column plus a constructor change.
- **API:** amounts are JSON numbers, e.g. `"monthlyFee": 150.00`.
- **Frontend:** amounts are formatted only with `Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' })`.
- Rounding happens only in the private `Money` constructor, so every value is rounded to 2 decimals. Allocation logic never produces fractions of a grosz.

### Date and time

| Concept | Type | Storage |
|---|---|---|
| Instant: created, paid, imported | `DateTimeOffset`, always UTC (offset 0) | `datetimeoffset` |
| Calendar date: booking date, due date, semester start | `DateOnly` | `date` |
| Time of day in a schedule | `TimeOnly` | `time` |
| Schedule in local time | `DateOnly` + `TimeOnly`, interpreted in the organization time zone | – |

- The organization time zone is a setting, default `Europe/Warsaw`. Convert to local time only at the edges: UI, emails, and "which month is due" calculations.
- `DateTime.Now` and `DateTimeOffset.UtcNow` are forbidden in application code. Inject `TimeProvider`; tests use `FakeTimeProvider`.
- The API exchanges ISO 8601 strings, with dates as `yyyy-MM-dd`.

---

## 5. Validation, errors, logging, configuration

### Validation

- **Input validation:** FluentValidation, one validator per request, nested in the slice. An endpoint filter, `.WithValidation<T>()`, runs it and returns `400` with `ValidationProblemDetails`. FluentValidation's legacy ASP.NET auto-validation package is not used.

  **Rationale:** FluentValidation supports async and cross-field rules and stays Apache-licensed. .NET 10's built-in `AddValidation()` covers only attribute rules.
- **Business rules** such as "course full" or "already enrolled" are checked in the domain or handler and returned as `Result` errors, not as validator rules.
- Every validation failure carries a stable error code, e.g. `WithErrorCode("course.capacity.min")`, so the frontend can translate it.
- The `400` body is `ValidationProblemDetails` with `code: "validation.failed"`. `errors` holds the English messages and the extension `errorCodes` holds the codes, both keyed by camelCase field name (`address.postalCode`).
- FluentValidation messages are always English, independent of the server culture.

### Errors – ProblemDetails (RFC 9457)

```csharp
public sealed record Error(string Code, string Message, ErrorType Type);
public enum ErrorType { Validation, NotFound, Conflict, Forbidden }

// in a module
public static class CourseErrors
{
    public static readonly Error Full = new("course.full", "Course has no free places.", ErrorType.Conflict);
}
```

- `Result` maps to HTTP in one place, `ClassBook.Shared/Errors/ResultExtensions.cs`:

  | `ErrorType` | HTTP status |
  |---|---|
  | `Validation` | 400 |
  | `Forbidden` | 403 |
  | `NotFound` | 404 |
  | `Conflict` | 409 |

  The body is `ProblemDetails` with `type`, `title`, `status`, and the extension `code`.
- `AddProblemDetails()` plus an `IExceptionHandler` turn unhandled exceptions into `500` ProblemDetails with a `traceId` and no exception details outside Development.
- Error messages in the API are English and meant for developers. Users see Polish texts that the frontend translates from `code`.

### Logging

- Use the built-in `ILogger<T>` with structured message templates:
  `logger.LogInformation("Imported {Count} payments from statement {StatementId}", n, id)`.
  Never use string interpolation in log calls.
- Output:
  - Development: JSON console.
  - Deployed: JSON console plus OpenTelemetry. The exporter is chosen with hosting.
- Levels:
  - `Information` for business events
  - `Warning` for handled anomalies, e.g. an unmatched payment
  - `Error` for failures
- **Never log personal data:** names, emails, children's data, bank account numbers, statement contents. Log IDs instead.
- Request logging uses the built-in HTTP logging middleware with bodies disabled.

### Configuration and secrets

- Use the Options pattern: one class per section, bound with `.BindConfiguration("Email").ValidateDataAnnotations().ValidateOnStart()`.
- `appsettings.json` holds non-secret defaults only. Secrets never go in the repository.
- **Local secrets:** `dotnet user-secrets` (connection string, Google and Microsoft client secrets, email credentials).
- **Deployed secrets:** environment variables or a secret store, decided with hosting.
- **Local database:** any SQL Server 2022 instance that the developer already runs, e.g. a local container. The repository ships no `docker-compose.yml`. The connection string `ConnectionStrings:ClassBook` is kept in user-secrets of `ClassBook.Api`. Integration tests start their own Testcontainers instance and need no local database.

---

## 6. API design and client generation

### API rules

- **Style:** a pragmatic RESTful JSON API, at Richardson maturity level 2. Resources are nouns, HTTP methods carry their standard meaning (`GET` safe, `PUT`/`DELETE` idempotent), and status codes are used correctly.
  - Business actions that are not CRUD, such as approve, reject or resign, are explicit command sub-resources (`POST /api/enrollments/{id}/approve`), not status `PATCH`es. This keeps intent, validation and authorization per action.
  - No HATEOAS links. The client is generated from OpenAPI.
- Every endpoint has a `WithName` (= `operationId`), a typed response (`Produces<T>`), its error statuses, and an authorization requirement.
- **Status codes:**

  | Situation | Status |
  |---|---|
  | `GET` | `200` |
  | `POST` creating a resource | `201` with `Location` |
  | Commands without a body | `204` |
  | Errors | ProblemDetails, see [§5](#5-validation-errors-logging-configuration) |

- **Pagination** for lists that can grow: `?page=1&pageSize=20`, with a maximum of 100. The response is `{ items: T[], totalCount: number }`. Short bounded lists return arrays.
- Filters and sorting are explicit query parameters, e.g. `?status=active&sort=startDate`. No generic query languages.
- **Versioning:** none for now. The API and SPA ship together. A breaking change is fine if the client is regenerated in the same PR.
- **File upload:** bank statements are imported via `multipart/form-data`, with a size limit and content-type checks.

### OpenAPI and client generation

- **Document:** built-in `Microsoft.AspNetCore.OpenApi`. `Microsoft.Extensions.ApiDescription.Server` writes the document at build time to `backend/openapi/v1.json`, which is committed. No Swashbuckle.
- **Development UI:** Scalar serves the document in Development only.
- **Client generation:** Orval generates a fetch client and TanStack Query hooks into `frontend/src/api/generated`.

  **Rationale:** it is a mainstream generator with first-class TanStack Query output, so components get typed hooks without hand-written glue.

```ts
// frontend/orval.config.ts
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
        fetch: { includeHttpResponseReturnType: false }, // hooks return the body; non-2xx throws ApiError
      },
    },
  },
});
```

- `npm run api:generate` regenerates the client. Any API change is committed together with the regenerated `v1.json` and client in the same PR.
- CI rebuilds the document and the client and fails on `git diff --exit-code`.
- **Single place for API calls:** `src/api/http.ts` is the only code that calls `fetch`. It:
  - sets credentials
  - attaches the CSRF header
  - parses ProblemDetails into a typed `ApiError`

  Components and features use only the generated hooks, and ESLint forbids `fetch` elsewhere (`no-restricted-globals`).

```tsx
const { data, isPending } = useGetCourses({ page: 1, pageSize: 20 });
const createCourse = useCreateCourse({ mutation: { onSuccess: () => queryClient.invalidateQueries({ queryKey: getGetCoursesQueryKey() }) } });
```

---

## 7. Frontend

### State and data fetching

- **Server state:** TanStack Query via the generated hooks only. Never copy query data into local state.
- **UI state:** `useState` or `useReducer`. **URL state** (filters, paging, selected tab) lives in search params.
- **Cross-cutting client state:** current user and locale use React context. There is no global store (Redux, Zustand) until a spec justifies one.
- Invalidate affected queries after mutations. Use optimistic updates only where a spec asks for them.
- **Routing:** React Router (v7, declarative/data mode, not framework mode). Pages are lazy-loaded per feature, and route guards check auth and role.

  **Rationale:** it is the mainstream router, and framework mode is unnecessary for a SPA.

### Forms and validation

- React Hook Form with Zod, through the shadcn `Form` components.
- Schemas are hand-written per form in `features/<x>/forms/`. Messages are i18n keys, not literal text.
- The server is the source of truth. A `400` ValidationProblem is mapped onto form fields by `code`, through a shared helper `applyServerErrors(form, apiError)`. Other errors show a toast or alert with the translated `code`.
- Submit buttons are disabled while pending, and every form has a visible error summary.

### i18n

- **Library:** react-i18next. `pl` is the default and only locale for now. Namespaces follow features (`common`, `courses`, `enrollments`, …), stored in `src/locales/pl/<ns>.json`.
- No hard-coded user-facing strings in components; `eslint-plugin-i18next` (`no-literal-string`) enforces this for JSX.
- Plurals use i18next plural keys, because Polish has `one`/`few`/`many` forms.
- **Formatting:** dates, numbers and money go only through helpers in `lib/format.ts`, which wrap `Intl`. Never format by string concatenation.
- **Backend texts:**
  - API errors are translated on the frontend from `code`.
  - Emails use `.resx` resources (`IStringLocalizer`) with `pl` as the default culture.

### Accessibility basics

- Use semantic HTML first: buttons are `<button>`, links are `<a>`, and every table has headers.
- Every input has a `<label>`. Errors are linked via `aria-describedby`. Dialogs trap focus and return it on close; shadcn/Radix provide this.
- Everything works with the keyboard and has a visible focus ring. Never remove outlines without a replacement.
- Colour contrast is WCAG 2.1 AA. Colour is never the only carrier of meaning, e.g. payment status also shows text.
- `eslint-plugin-jsx-a11y` is enabled, and Playwright runs `@axe-core/playwright` on key pages with no serious or critical violations allowed.

---

## 8. Security

### Authentication

- **Accounts:** ASP.NET Core Identity, used UI-less: the `UserManager` and `SignInManager` stores live in the `identity` schema. The Identity module exposes its own endpoints: register, login, logout, confirm email, reset password, external login.
- **Session:** a cookie (`HttpOnly`, `Secure`, `SameSite=Lax`), with the SPA served from the same origin as the API.

  **Rationale:** no tokens in browser storage and no CORS, and it is the simplest secure option for a first-party SPA.
  - In development, Vite proxies `/api` to the backend.
- **External sign-in:**
  - Google and Microsoft use the built-in `AddGoogle` / `AddMicrosoftAccount` handlers.
  - An external login links to an existing account only when the email is verified.
- **Passwords:**
  - Identity defaults with a minimum length of 10.
  - Lockout after repeated failures.
  - Email confirmation required before enrolling.
- **CSRF:** ASP.NET Core antiforgery. The SPA reads the `XSRF-TOKEN` cookie and sends `X-XSRF-TOKEN` on unsafe methods; `http.ts` does this. All non-GET endpoints validate it.
- **Rate limiting:** the built-in rate limiter applies to login, register, password reset and statement import.

### Authorization

- **Default:** a fallback policy requires an authenticated user for every endpoint. Anonymous endpoints opt out explicitly with `AllowAnonymous()`.
- **Roles:** `Administrator`, `Instructor`, `Guardian` and `Participant`, as Identity roles carried as claims.
- **Policies:** endpoints use named policies, never role strings. They are defined in `ClassBook.Shared/Security/Policies.cs`:

```csharp
public static class Policies
{
    public const string ManageCourses = nameof(ManageCourses);      // Administrator
    public const string ManagePayments = nameof(ManagePayments);    // Administrator
    public const string ViewOwnClasses = nameof(ViewOwnClasses);    // Instructor, Administrator
}
```

- **Resource access:** questions like "is this the guardian of this family?" and "does this instructor teach this class?" are checked in the handler through module-internal services, e.g. `familyAccess.EnsureCanManage(familyId)`, returning `Forbidden` or `NotFound`.

  Return `404` rather than `403` when the caller must not learn that the resource exists.
- **Testing:** integration tests cover every endpoint with at least one allowed role and one denied role.

### Personal data (GDPR)

- **Inventory:** personal data is kept in the Families and Identity modules and in payment records. It includes:
  - names, emails, phone numbers
  - children's birth dates
  - bank account numbers and payer names from statements

  Each new personal field must be justified in its spec, which keeps data collection minimal.
- **Children's data:** collect only what enrollment needs. No health or special-category data without a dedicated spec.
- **Rights:**
  - **Export:** an admin function returns a family's data as JSON.
  - **Erasure:** anonymize the personal fields and keep financial records with a pseudonymous reference, because accounting retention obligations apply.
- **Access:** personal data is visible only to administrators, the family's guardian, and instructors for the participants of their own classes (names only).
- **Leaks:** no personal data in logs, URLs, query strings or error messages. Bank statement files are not stored after import unless a spec requires it.
- **Transport and storage:** HTTPS with HSTS everywhere, and security headers (CSP, `X-Content-Type-Options`, `Referrer-Policy`, frame-ancestors). Encryption at rest is a hosting setting.

---

## 9. Testing strategy

| Layer | Tool | What | Where |
|---|---|---|---|
| Backend unit | xUnit v3, Shouldly, NSubstitute, `FakeTimeProvider` | Domain entities and value objects, allocation and matching logic, parsers (MT940/CSV), validators | `ClassBook.UnitTests` |
| Backend integration | xUnit v3, `WebApplicationFactory`, Testcontainers.MsSql, Respawn | Each slice through HTTP: success, validation, business errors, authorization, tenant isolation | `ClassBook.IntegrationTests` |
| Architecture | ArchUnitNET | Module dependency rules, `Public/` usage, tenant filters, naming | `ClassBook.ArchitectureTests` |
| Frontend unit/component | Vitest, Testing Library, MSW, `user-event` | Components, hooks, forms, format helpers; API mocked with MSW at the network level | `*.test.tsx` next to the code |
| E2E | Playwright, `@axe-core/playwright` | Critical journeys: sign-in, enroll a child, admin approves, import a statement and see the balance | `frontend/e2e` |

### Rules

- **Integration setup:** one SQL Server container per test run, started in an assembly fixture; Respawn resets data between tests. Never use the EF InMemory provider or SQLite as a substitute for SQL Server.
- **Doubles:** don't mock module `DbContext`s. Mock only external boundaries: email sender, external auth, time.
- **Test shape:** tests are independent and order-free, follow Arrange-Act-Assert, and test one behavior each. Test data comes from small builders, e.g. `CourseBuilder`.
- **Frontend:** test behavior through roles and labels (`getByRole`), not implementation details or snapshots.
- **Bug fixes:** start with a failing test that reproduces the bug (see AGENTS.md).

### Coverage

- 100% line coverage of new and changed lines, enforced in CI as diff coverage against the PR base. Branch coverage is reported and reviewed, but not gated, because `diff-cover` measures lines only.
- Tools:
  - Backend: Coverlet (`coverlet.MTP`, Cobertura output). xUnit v3 runs on Microsoft.Testing.Platform, enabled in `backend/global.json`; the VSTest-only `coverlet.collector` does not work with it.
  - Frontend: Vitest with `@vitest/coverage-v8` (Cobertura output).
  - Diff coverage: `diff-cover --fail-under=100` over both reports.
- Backend coverage combines unit and integration tests.
- **Excluded:** `api/generated/**`, `components/ui/**` (shadcn), `Migrations/**`, `Program.cs` composition, and `*.config.ts`. Any other exclusion needs `[ExcludeFromCodeCoverage(Justification = "...")]` or `/* v8 ignore next */` with a reason, and it is reviewed in the PR.
- Global coverage is reported but not gated. The diff gate keeps it moving upward.

### Definition of done

A change is done when all of the following hold:

1. The spec exists and is up to date, and its acceptance criteria are met.
2. The backend builds with zero warnings, and the frontend type-checks and lints cleanly.
3. All tests pass locally and in CI, including E2E for affected journeys.
4. Diff coverage is 100%.
5. The OpenAPI document and generated client are up to date.
6. Migrations are included and reviewed, if the schema changed.
7. New UI strings are in `locales/pl`, and there are no a11y lint errors.
8. The AGENTS.md checklist is complete: lessons recorded, run log written, issue referenced.

---

## 10. Libraries and tooling

### Versions policy

- Use the latest stable version at adoption time. No prereleases.
- .NET stays on LTS (currently .NET 10). Node uses the active LTS, pinned in `.nvmrc` and `package.json` `engines`.
- **Backend:** NuGet versions are central in `Directory.Packages.props` (Central Package Management).
- **Frontend:** npm with a committed `package-lock.json`; install with `npm ci` in CI.
- Dependabot opens weekly grouped update PRs. Major upgrades get their own issue.
- **New dependencies:**
  - A new dependency must be justified in the PR: what it replaces and why the built-in option isn't enough.
  - Prefer the platform and the libraries listed here.
  - Check licenses. No commercial, copyleft or "source-available" licenses without approval.

### Backend libraries

| Purpose | Library |
|---|---|
| Web, DI, logging, options, rate limiting, ProblemDetails | ASP.NET Core 10 (built-in) |
| ORM | EF Core 10 + `Microsoft.EntityFrameworkCore.SqlServer`, `dotnet-ef` local tool |
| Identity | ASP.NET Core Identity + Google and Microsoft authentication handlers |
| OpenAPI | `Microsoft.AspNetCore.OpenApi`, `Microsoft.Extensions.ApiDescription.Server`; Scalar (dev UI) |
| Validation | FluentValidation |
| Observability | OpenTelemetry (.NET) |
| Email | `System.Net.Mail` / MailKit, depending on provider (deferred) |
| Tests | xUnit v3, Shouldly, NSubstitute, Testcontainers.MsSql, Respawn, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.Extensions.TimeProvider.Testing`, ArchUnitNET, coverlet.MTP |

Avoid: MediatR, AutoMapper, FluentAssertions v8+, Moq, Swashbuckle, Newtonsoft.Json, and EF InMemory. FluentAssertions v8+ is commercially licensed. Moq has a history of a privacy incident. The others are replaced by built-in features or by choices made above.

### Frontend libraries

| Purpose | Library |
|---|---|
| Build/dev | Vite, TypeScript (`strict`) |
| UI | React, Tailwind CSS v4, shadcn/ui (Radix base: `shadcn init --base radix`), `lucide-react` icons; `cn()` from clsx + tailwind-merge |
| Routing | React Router v7 |
| Server state | TanStack Query v5 |
| API client | Orval (generated) |
| Forms | React Hook Form, Zod, `@hookform/resolvers` |
| i18n | i18next, react-i18next |
| Tests | Vitest, `@vitest/coverage-v8`, Testing Library (`react`, `user-event`, `jest-dom`), MSW, Playwright, `@axe-core/playwright` |

### Tooling, linters and formatters

**Backend** (`Directory.Build.props`):

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
</PropertyGroup>
```

- `.editorconfig` at the repository root is the single source of code style. It uses file-scoped namespaces, `var` when the type is apparent, and sorted usings with `System` first.
- `dotnet format --verify-no-changes` runs in CI.

**Frontend:**

- ESLint 9 flat config: `typescript-eslint` (strict-type-checked), `eslint-plugin-react-hooks`, `eslint-plugin-jsx-a11y`, `eslint-plugin-i18next`, and import boundary rules. ESLint 10 waits until `eslint-plugin-jsx-a11y` supports it.
- Prettier with `prettier-plugin-tailwindcss`.
- `tsc --noEmit` in CI.

**Repository:**

- **Git hooks:** none. CI is the gate.
- **CI** (GitHub Actions, on every PR):
  1. Backend: `dotnet format --verify-no-changes`, then build, then unit, architecture and integration tests with coverage.
  2. Frontend: `npm ci`, lint, type-check, then Vitest with coverage.
  3. OpenAPI and client drift check.
  4. `diff-cover` at 100%.
  5. Playwright E2E against the API and SPA built in the job.

---

## Acceptance criteria

- [ ] All 10 topics above contain concrete, checkable rules.
- [ ] The scaffold issue can be implemented from this spec without new architectural decisions.
- [ ] No contradiction with AGENTS.md or the fixed decisions.
- [x] The project owner has resolved or explicitly deferred all open questions.

## Resolved decisions

| Question | Decision |
|---|---|
| Backend structure | One project per module plus a thin host. The compiler enforces boundaries. |
| Data access | One `DbContext` per module, each with its own schema and migrations. |
| Coverage scope | Unit and integration tests combined. |
| SPA hosting | Same origin: the API serves the built SPA, with cookie auth. |
| Table naming | Singular (`courses.Course`). |
| Package manager | npm. |
| Git hooks | None; CI only. |
| Local database | An existing SQL Server instance via user-secrets; no compose file in the repository. |

## Deferred decisions

These are decided in the spec of the feature that needs them, not in the scaffold.

1. **Hosting target:** this decides the secret store, the migration deploy step, the OpenTelemetry exporter and the email service.
2. **Email provider:** SMTP, Azure Communication Services or SendGrid.
3. **Bank statements:** which bank(s) and format(s) come first (MT940 or a bank-specific CSV), and whether to adopt a .NET MT940 parser.
4. **Participant logins:** whether minors can have their own login, and from what age.
5. **Data retention:** how long personal and financial data are kept after a family leaves.
