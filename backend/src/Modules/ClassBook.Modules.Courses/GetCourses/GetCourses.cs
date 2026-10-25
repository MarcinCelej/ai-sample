using ClassBook.Modules.Courses.Domain;
using ClassBook.Modules.Courses.Persistence;
using ClassBook.Shared.Paging;
using ClassBook.Shared.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ClassBook.Modules.Courses.GetCourses;

/// <summary>The public course catalog: active courses, ordered by name, paged.</summary>
internal static class GetCourses
{
    public sealed record Request(
        [FromQuery(Name = "page")] int Page = 1,
        [FromQuery(Name = "pageSize")] int PageSize = Paging.DefaultPageSize);

    public sealed record Item(Guid Id, string Name, string? Description, int Capacity, decimal MonthlyFee);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Page).ValidPage();
            RuleFor(x => x.PageSize).ValidPageSize();
        }
    }

    public sealed class Handler(CoursesDbContext db)
    {
        public Task<PagedResponse<Item>> HandleAsync(Request request, CancellationToken ct) =>
            db.Courses
                .AsNoTracking()
                .Where(c => c.Status == CourseStatus.Active)
                .OrderBy(c => c.Name).ThenBy(c => c.Id)
                .Select(c => new Item(c.Id, c.Name, c.Description, c.Capacity, c.MonthlyFee.Amount))
                .ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public static void MapEndpoint(RouteGroupBuilder group) =>
        group.MapGet("/", async ([AsParameters] Request request, Handler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(request, ct)))
            .WithName(nameof(GetCourses))
            .WithValidation<Request>()
            .AllowAnonymous();
}
