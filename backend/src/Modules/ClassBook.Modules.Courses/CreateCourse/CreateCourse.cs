using ClassBook.Modules.Courses.Domain;
using ClassBook.Modules.Courses.Persistence;
using ClassBook.Shared.Domain;
using ClassBook.Shared.Errors;
using ClassBook.Shared.Security;
using ClassBook.Shared.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ClassBook.Modules.Courses.CreateCourse;

internal static class CreateCourse
{
    public sealed record Request(string Name, string? Description, int Capacity, decimal MonthlyFee);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("course.name.required")
                .MaximumLength(Course.NameMaxLength).WithErrorCode("course.name.tooLong");
            RuleFor(x => x.Description).MaximumLength(Course.DescriptionMaxLength).WithErrorCode("course.description.tooLong");
            RuleFor(x => x.Capacity).GreaterThan(0).WithErrorCode("course.capacity.min");
            RuleFor(x => x.MonthlyFee).GreaterThanOrEqualTo(0).WithErrorCode("course.monthlyFee.min")
                .PrecisionScale(18, 2, true).WithErrorCode("course.monthlyFee.precision");
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
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
}
