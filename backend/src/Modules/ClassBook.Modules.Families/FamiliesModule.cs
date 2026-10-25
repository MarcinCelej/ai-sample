using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Modules.Families;

public static class FamiliesModule
{
    public static IServiceCollection AddFamiliesModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapFamiliesEndpoints(this IEndpointRouteBuilder app) => app;
}
