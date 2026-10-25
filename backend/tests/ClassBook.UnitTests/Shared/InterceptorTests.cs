using ClassBook.Modules.Courses.Domain;
using ClassBook.Modules.Courses.Persistence;
using ClassBook.Shared.Domain;
using ClassBook.Shared.Persistence;
using ClassBook.Shared.Security;
using ClassBook.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace ClassBook.UnitTests.Shared;

/// <summary>
/// The interceptors only touch the change tracker, so they are tested without a database:
/// SQL Server is configured but no connection is ever opened.
/// </summary>
public sealed class InterceptorTests : IDisposable
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly ITenantContext _tenant = Substitute.For<ITenantContext>();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 25, 9, 0, 0, TimeSpan.Zero));
    private readonly CoursesDbContext _db;

    public InterceptorTests()
    {
        _tenant.TenantId.Returns(TenantA);
        var options = new DbContextOptionsBuilder<CoursesDbContext>().UseSqlServer("Server=unused").Options;
        _db = new CoursesDbContext(options, _tenant);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void TenantInterceptor_SetsTenantOnAddedEntities()
    {
        var course = AddCourse();

        new TenantInterceptor(_tenant).Apply(_db);

        course.TenantId.ShouldBe(TenantA);
    }

    [Fact]
    public void TenantInterceptor_ChangedTenant_Throws()
    {
        var course = AttachCourse();
        _db.Entry(course).Property(c => c.TenantId).CurrentValue = Guid.NewGuid();

        Should.Throw<InvalidOperationException>(() => new TenantInterceptor(_tenant).Apply(_db));
    }

    [Fact]
    public void TenantInterceptor_UnchangedTenantOnModifiedEntity_IsAllowed()
    {
        var course = AttachCourse();
        course.Archive();

        Should.NotThrow(() => new TenantInterceptor(_tenant).Apply(_db));
    }

    [Fact]
    public void AuditInterceptor_SetsCreatedFieldsOnAddedEntities()
    {
        var course = AddCourse();

        new AuditInterceptor(_time, CurrentUser(UserId)).Apply(_db);

        course.CreatedAt.ShouldBe(_time.GetUtcNow());
        course.CreatedBy.ShouldBe(UserId);
        course.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void AuditInterceptor_SetsUpdatedFieldsOnModifiedEntities()
    {
        var course = AttachCourse();
        course.Archive();

        new AuditInterceptor(_time, CurrentUser(null)).Apply(_db);

        course.UpdatedAt.ShouldBe(_time.GetUtcNow());
        course.UpdatedBy.ShouldBeNull();
    }

    [Fact]
    public void Interceptors_IgnoreMissingContext()
    {
        Should.NotThrow(() => new TenantInterceptor(_tenant).Apply(null));
        Should.NotThrow(() => new AuditInterceptor(_time, CurrentUser(null)).Apply(null));
    }

    [Fact]
    public void TenantFilter_ScopesQueriesToCurrentTenant()
    {
        var sql = _db.Courses.ToQueryString();

        sql.ShouldContain("[c].[TenantId] = @");
        sql.ShouldContain(TenantA.ToString(), Case.Insensitive);
    }

    private static ICurrentUser CurrentUser(Guid? userId)
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(userId);
        return user;
    }

    private Course AddCourse()
    {
        var course = Course.Create("Chess", null, 10, Money.Pln(100m));
        _db.Courses.Add(course);
        return course;
    }

    private Course AttachCourse()
    {
        var course = Course.Create("Chess", null, 10, Money.Pln(100m));
        _db.Entry(course).Property(c => c.Id).CurrentValue = Guid.NewGuid();
        _db.Entry(course).Property(c => c.TenantId).CurrentValue = TenantA;
        _db.Courses.Attach(course);
        return course;
    }
}
