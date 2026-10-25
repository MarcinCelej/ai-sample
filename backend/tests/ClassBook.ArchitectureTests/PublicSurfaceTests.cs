namespace ClassBook.ArchitectureTests;

public class PublicSurfaceTests
{
    public static TheoryData<string> ModuleNames() => [.. ModuleMap.Assemblies.Keys];

    /// <summary>
    /// Only the module entry class and types in <c>Public/</c> may be visible outside the module.
    /// <see cref="System.Reflection.Assembly.GetExportedTypes"/> returns effectively public types, so public types
    /// nested in internal slice classes (CreateCourse.Request) are correctly not counted.
    /// EF Core scaffolds migrations as public; that generated code is exempt.
    /// </summary>
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_ExposesOnlyEntryClassAndPublicNamespace(string module)
    {
        var publicNamespace = ModuleMap.PublicNamespace(module);
        var entryClass = $"ClassBook.Modules.{module}.{module}Module";
        var migrationsNamespace = $"ClassBook.Modules.{module}.Persistence.Migrations";

        var violations = ModuleMap.Assemblies[module].GetExportedTypes()
            .Where(t => t.FullName != entryClass && t.Namespace != migrationsNamespace)
            .Where(t => t.Namespace != publicNamespace && t.Namespace?.StartsWith(publicNamespace + ".", StringComparison.Ordinal) != true)
            .Select(t => t.FullName)
            .ToList();

        violations.ShouldBeEmpty($"make these types internal or move them to {publicNamespace}");
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Module_HasEntryClass(string module)
    {
        ModuleMap.Assemblies[module].GetType($"ClassBook.Modules.{module}.{module}Module").ShouldNotBeNull();
    }
}
