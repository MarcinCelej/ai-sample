using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Modules.Payments;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder app) => app;
}
