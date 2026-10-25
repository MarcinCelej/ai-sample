using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Modules.Courses;

public static class CoursesModule
{
    public static IServiceCollection AddCoursesModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapCoursesEndpoints(this IEndpointRouteBuilder app) => app;
}
