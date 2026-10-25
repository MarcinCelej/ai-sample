using ClassBook.Modules.Courses;
using ClassBook.Modules.Enrollments;
using ClassBook.Modules.Families;
using ClassBook.Modules.Identity;
using ClassBook.Modules.Payments;
using ClassBook.Shared;
using ClassBook.Shared.Persistence;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddJsonConsole();
builder.Services.AddHttpLogging(o =>
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders | HttpLoggingFields.Duration);
builder.Services.AddOpenApi(o => o.CreateSchemaReferenceId = type =>
    OpenApiOptions.CreateDefaultSchemaReferenceId(type) is null ? null : SchemaName(type.Type));

builder.Services.AddClassBookShared();
builder.Services
    .AddIdentityModule()
    .AddCoursesModule()
    .AddFamiliesModule()
    .AddEnrollmentsModule()
    .AddPaymentsModule();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseClassBookAntiforgery();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.MapIdentityEndpoints()
    .MapCoursesEndpoints()
    .MapFamiliesEndpoints()
    .MapEnrollmentsEndpoints()
    .MapPaymentsEndpoints();

// The built SPA is served from wwwroot for client-side routes; unknown /api routes stay 404.
app.Map("/api/{**path}", () => TypedResults.NotFound()).AllowAnonymous().ExcludeFromDescription();
app.MapFallbackToFile("index.html").AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    await app.MigrateModuleDatabasesAsync();
}

await app.RunAsync();

// Slice types are nested (CreateCourse.Request), so prefix them with the slice to keep schema names unique:
// CreateCourseRequest, PagedResponseOfGetCoursesItem.
static string SchemaName(Type type)
{
    var name = type.IsGenericType ? type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)] : type.Name;
    if (type.IsNested && type.DeclaringType is { } slice)
    {
        name = slice.Name + name;
    }

    return type.IsGenericType ? name + "Of" + string.Concat(type.GetGenericArguments().Select(SchemaName)) : name;
}
