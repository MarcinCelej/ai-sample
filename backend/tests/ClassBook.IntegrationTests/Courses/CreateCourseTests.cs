using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClassBook.IntegrationTests.Infrastructure;
using ClassBook.Modules.Courses.Persistence;
using ClassBook.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static ClassBook.Modules.Courses.CreateCourse.CreateCourse;

namespace ClassBook.IntegrationTests.Courses;

public class CreateCourseTests(ApiFixture api) : IntegrationTest(api)
{
    private static readonly Uri CoursesUri = new("/api/courses", UriKind.Relative);
    private static readonly Request ValidRequest = new("Chess", "Beginners", 12, 150.50m);

    [Fact]
    public async Task Administrator_CreatesCourse_Returns201AndStoresItWithTenantAndAudit()
    {
        var admin = TestUser.Administrator();
        using var client = await CreateClientWithAntiforgeryAsync(admin);

        using var response = await client.PostAsJsonAsync(CoursesUri, ValidRequest, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<Response>(TestContext.Current.CancellationToken))!;
        response.Headers.Location.ShouldBe(new Uri($"/api/courses/{created.Id}", UriKind.Relative));

        await WithScopeAsync(async services =>
        {
            var course = await services.GetRequiredService<CoursesDbContext>().Courses.SingleAsync(c => c.Id == created.Id);
            course.Name.ShouldBe("Chess");
            course.MonthlyFee.ShouldBe(Money.Pln(150.50m));
            course.TenantId.ShouldBe(DefaultTenantId);
            course.CreatedBy.ShouldBe(admin.Id);
            course.CreatedAt.ShouldNotBe(default);
        });
    }

    [Fact]
    public async Task InvalidRequest_Returns400WithFieldErrorCodes()
    {
        using var client = await CreateClientWithAntiforgeryAsync(TestUser.Administrator());

        using var response = await client.PostAsJsonAsync(
            CoursesUri, ValidRequest with { Name = "", Capacity = 0 }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        problem.GetProperty("code").GetString().ShouldBe("validation.failed");
        var codes = problem.GetProperty("errorCodes");
        codes.GetProperty("name")[0].GetString().ShouldBe("course.name.required");
        codes.GetProperty("capacity")[0].GetString().ShouldBe("course.capacity.min");
    }

    [Fact]
    public async Task Guardian_Returns403()
    {
        using var client = await CreateClientWithAntiforgeryAsync(TestUser.Guardian());

        using var response = await client.PostAsJsonAsync(CoursesUri, ValidRequest, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_Returns401()
    {
        using var client = await CreateClientWithAntiforgeryAsync();

        using var response = await client.PostAsJsonAsync(CoursesUri, ValidRequest, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MissingAntiforgeryToken_Returns400()
    {
        using var client = CreateClient(TestUser.Administrator());

        using var response = await client.PostAsJsonAsync(CoursesUri, ValidRequest, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        problem.GetProperty("code").GetString().ShouldBe("antiforgery.invalid");
    }
}
