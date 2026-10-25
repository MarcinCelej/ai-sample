using ClassBook.Modules.Courses;
using ClassBook.Modules.Enrollments;
using ClassBook.Modules.Families;
using ClassBook.Modules.Identity;
using ClassBook.Modules.Payments;
using ClassBook.Shared;
using ClassBook.Shared.Persistence;
using Microsoft.AspNetCore.HttpLogging;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddJsonConsole();
builder.Services.AddHttpLogging(o =>
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders | HttpLoggingFields.Duration);
builder.Services.AddOpenApi();

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
