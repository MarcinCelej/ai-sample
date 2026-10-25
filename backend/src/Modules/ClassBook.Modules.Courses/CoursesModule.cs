using ClassBook.Modules.Courses.Persistence;
using ClassBook.Shared.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Modules.Courses;

public static class CoursesModule
{
    public static IServiceCollection AddCoursesModule(this IServiceCollection services) => services
        .AddModuleDbContext<CoursesDbContext>()
        .AddScoped<CreateCourse.CreateCourse.Handler>()
        .AddScoped<IValidator<CreateCourse.CreateCourse.Request>, CreateCourse.CreateCourse.Validator>()
        .AddScoped<GetCourses.GetCourses.Handler>()
        .AddScoped<IValidator<GetCourses.GetCourses.Request>, GetCourses.GetCourses.Validator>();

    public static IEndpointRouteBuilder MapCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");
        CreateCourse.CreateCourse.MapEndpoint(group);
        GetCourses.GetCourses.MapEndpoint(group);
        return app;
    }
}
