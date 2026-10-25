using ClassBook.Shared.Persistence;
using ClassBook.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.ArchitectureTests;

/// <summary>
/// Builds the EF Core model of every module <c>DbContext</c> without a database (no connection is opened)
/// and checks the persistence rules of the spec.
/// </summary>
public class PersistenceTests
{
    public static TheoryData<string> ContextNames() => [.. ModuleContextTypes().Select(t => t.FullName!)];

    [Fact]
    public void EveryModuleWithEntities_HasOneContext_NamedAfterTheModule()
    {
        foreach (var context in ModuleContextTypes())
        {
            var module = context.Assembly.GetName().Name!["ClassBook.Modules.".Length..];
            context.Name.ShouldBe($"{module}DbContext");
            context.IsNotPublic.ShouldBeTrue($"{context.Name} must be internal");
        }
    }

    [Theory]
    [MemberData(nameof(ContextNames))]
    public void EveryTenantOwnedEntity_HasTheTenantFilter(string contextName)
    {
        using var context = CreateContext(contextName);

        foreach (var entity in TenantOwnedEntities(context.Model))
        {
            entity.FindDeclaredQueryFilter(ModuleDbContext.TenantFilter)
                .ShouldNotBeNull($"{entity.DisplayName()} is tenant-owned but has no '{ModuleDbContext.TenantFilter}' query filter");
        }
    }

    [Theory]
    [MemberData(nameof(ContextNames))]
    public void EveryBusinessEntity_IsTenantOwned(string contextName)
    {
        using var context = CreateContext(contextName);
        if (context is not ModuleDbContext)
        {
            return; // The Identity context holds framework tables, which are not tenant-owned.
        }

        foreach (var entity in context.Model.GetEntityTypes().Where(e => !e.IsOwned()))
        {
            typeof(ITenantOwned).IsAssignableFrom(entity.ClrType)
                .ShouldBeTrue($"{entity.DisplayName()} must implement {nameof(ITenantOwned)}");
        }
    }

    private static IEnumerable<Type> ModuleContextTypes() =>
        ModuleMap.Assemblies.Values
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t) && typeof(IModuleDbContext).IsAssignableFrom(t));

    private static IEnumerable<IEntityType> TenantOwnedEntities(IModel model) =>
        model.GetEntityTypes().Where(e => typeof(ITenantOwned).IsAssignableFrom(e.ClrType));

    private static DbContext CreateContext(string contextName)
    {
        var contextType = ModuleContextTypes().Single(t => t.FullName == contextName);
        var optionsBuilder = (DbContextOptionsBuilder)Activator.CreateInstance(typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType))!;
        optionsBuilder.UseSqlServer("Server=unused");

        var services = new ServiceCollection()
            .AddSingleton(optionsBuilder.Options.GetType(), optionsBuilder.Options)
            .AddSingleton<ITenantContext>(new FixedTenant())
            .BuildServiceProvider();
        return (DbContext)ActivatorUtilities.CreateInstance(services, contextType);
    }

    private sealed class FixedTenant : ITenantContext
    {
        public Guid TenantId { get; } = Guid.NewGuid();
    }
}
