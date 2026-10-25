using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Modules.Enrollments;

public static class EnrollmentsModule
{
    public static IServiceCollection AddEnrollmentsModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapEnrollmentsEndpoints(this IEndpointRouteBuilder app) => app;
}
