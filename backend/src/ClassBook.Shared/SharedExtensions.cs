using System.Globalization;
using System.Text.Json.Serialization;
using ClassBook.Shared.Errors;
using ClassBook.Shared.Persistence;
using ClassBook.Shared.Security;
using ClassBook.Shared.Tenancy;
using ClassBook.Shared.Time;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Shared;

public static class SharedExtensions
{
    /// <summary>Registers the cross-cutting building blocks every module relies on.</summary>
    public static IServiceCollection AddClassBookShared(this IServiceCollection services)
    {
        // API messages are English for developers; users see texts translated from error codes.
        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // Numbers are numbers: no reading from strings, which also keeps OpenAPI types exact.
            o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });

        services.AddOptions<TenancyOptions>()
            .BindConfiguration(TenancyOptions.SectionName)
            .Validate(o => o.DefaultTenantId != Guid.Empty, $"{TenancyOptions.SectionName}:DefaultTenantId must be set.")
            .ValidateOnStart();
        services.AddScoped<ITenantContext, SingleTenantContext>();

        services.AddOptions<OrganizationTimeOptions>()
            .BindConfiguration(OrganizationTimeOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<OrganizationClock>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<TenantInterceptor>();
        services.AddScoped<AuditInterceptor>();

        services.AddAntiforgery(o => o.HeaderName = AntiforgeryMiddleware.HeaderName);
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Policies.ManageCourses, p => p.RequireRole(Roles.Administrator))
            .AddPolicy(Policies.ManagePayments, p => p.RequireRole(Roles.Administrator))
            .AddPolicy(Policies.ViewOwnClasses, p => p.RequireRole(Roles.Instructor, Roles.Administrator));

        return services;
    }

    /// <summary>Issues and validates CSRF tokens for <c>/api</c>. Call after authentication and authorization.</summary>
    public static IApplicationBuilder UseClassBookAntiforgery(this IApplicationBuilder app) =>
        app.UseMiddleware<AntiforgeryMiddleware>();
}
