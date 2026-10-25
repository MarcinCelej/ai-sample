using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClassBook.IntegrationTests.Infrastructure;
using ClassBook.Modules.Courses.Domain;
using ClassBook.Modules.Courses.Persistence;
using ClassBook.Shared.Domain;
using ClassBook.Shared.Paging;
using Microsoft.Extensions.DependencyInjection;
using static ClassBook.Modules.Courses.GetCourses.GetCourses;

namespace ClassBook.IntegrationTests.Courses;

public class GetCoursesTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task Anonymous_GetsActiveCoursesOrderedByNameAndPaged()
    {
        await SeedAsync(("Robotics", false), ("Archery", false), ("Chess", false), ("Ballet", true));
        using var client = CreateClient();

        var page1 = await GetPageAsync(client, "/api/courses?page=1&pageSize=2");
        var page2 = await GetPageAsync(client, "/api/courses?page=2&pageSize=2");

        page1.TotalCount.ShouldBe(3);
        page1.Items.Select(c => c.Name).ShouldBe(["Archery", "Chess"]);
        page2.Items.Select(c => c.Name).ShouldBe(["Robotics"]);
        page1.Items[0].MonthlyFee.ShouldBe(99.99m);
    }

    [Fact]
    public async Task CoursesOfAnotherTenant_AreInvisible()
    {
        var otherTenant = Guid.NewGuid();
        await SeedAsync(("Default tenant course", false));
        using (var otherAdmin = await CreateClientWithAntiforgeryAsync(TestUser.Administrator(), otherTenant))
        {
            using var created = await otherAdmin.PostAsJsonAsync(
                new Uri("/api/courses", UriKind.Relative),
                new { name = "Other tenant course", capacity = 5, monthlyFee = 10m },
                TestContext.Current.CancellationToken);
            created.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        using var defaultTenant = CreateClient();
        using var otherTenantClient = CreateClient(tenantId: otherTenant);

        (await GetPageAsync(defaultTenant, "/api/courses")).Items.Select(c => c.Name).ShouldBe(["Default tenant course"]);
        (await GetPageAsync(otherTenantClient, "/api/courses")).Items.Select(c => c.Name).ShouldBe(["Other tenant course"]);
    }

    [Fact]
    public async Task InvalidPaging_Returns400WithErrorCode()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(new Uri("/api/courses?pageSize=1000", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        problem.GetProperty("errorCodes").GetProperty("pageSize")[0].GetString().ShouldBe(Paging.PageSizeInvalidCode);
    }

    private static async Task<PagedResponse<Item>> GetPageAsync(HttpClient client, string uri) =>
        (await client.GetFromJsonAsync<PagedResponse<Item>>(new Uri(uri, UriKind.Relative), TestContext.Current.CancellationToken))!;

    private Task SeedAsync(params (string Name, bool Archived)[] courses) => WithScopeAsync(async services =>
    {
        var db = services.GetRequiredService<CoursesDbContext>();
        foreach (var (name, archived) in courses)
        {
            var course = Course.Create(name, null, 10, Money.Pln(99.99m));
            if (archived)
            {
                course.Archive();
            }

            db.Courses.Add(course);
        }

        await db.SaveChangesAsync();
    });
}
