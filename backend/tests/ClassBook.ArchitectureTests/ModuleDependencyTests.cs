using System.Text.RegularExpressions;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using ClassBook.Shared;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ClassBook.ArchitectureTests;

public class ModuleDependencyTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies([typeof(SharedExtensions).Assembly, .. ModuleMap.Assemblies.Values])
        .Build();

    public static TheoryData<string> ModuleNames() => [.. ModuleMap.Assemblies.Keys];

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_DoesNotDependOnModulesOutsideTheAllowedGraph(string module)
    {
        var forbidden = ModuleMap.Assemblies.Keys
            .Where(other => other != module && !ModuleMap.AllowedDependencies[module].Contains(other))
            .ToList();
        Assert.SkipWhen(forbidden.Count == 0, "Every other module is allowed.");

        Types().That().ResideInAssembly(ModuleMap.Assemblies[module])
            .Should().NotDependOnAny(Types().That().ResideInAssembly(ModuleMap.Assemblies[forbidden[0]], [.. forbidden.Skip(1).Select(m => ModuleMap.Assemblies[m])]))
            .Because($"{module} may depend only on: {string.Join(", ", ModuleMap.AllowedDependencies[module])}")
            .Check(Architecture);
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_UsesOnlyPublicTypesOfItsDependencies(string module)
    {
        var dependencies = ModuleMap.AllowedDependencies[module];
        Assert.SkipWhen(dependencies.Length == 0, $"{module} has no module dependencies.");

        foreach (var dependency in dependencies)
        {
            Types().That().ResideInAssembly(ModuleMap.Assemblies[module])
                .Should().NotDependOnAny(Types().That().ResideInAssembly(ModuleMap.Assemblies[dependency])
                    .And().DoNotResideInNamespaceMatching($"^{Regex.Escape(ModuleMap.PublicNamespace(dependency))}(\\..*)?$"))
                .Because($"other modules may use only {ModuleMap.PublicNamespace(dependency)}")
                .Check(Architecture);
        }
    }

    [Fact]
    public void Shared_DoesNotDependOnAnyModule()
    {
        var modules = ModuleMap.Assemblies.Values.ToList();

        Types().That().ResideInAssembly(typeof(SharedExtensions).Assembly)
            .Should().NotDependOnAny(Types().That().ResideInAssembly(modules[0], [.. modules.Skip(1)]))
            .Check(Architecture);
    }
}
