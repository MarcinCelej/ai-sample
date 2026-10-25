using System.Reflection;
using ClassBook.Modules.Courses;
using ClassBook.Modules.Enrollments;
using ClassBook.Modules.Families;
using ClassBook.Modules.Identity;
using ClassBook.Modules.Payments;

namespace ClassBook.ArchitectureTests;

/// <summary>The modules and their allowed dependencies, as defined in the engineering standards spec.</summary>
internal static class ModuleMap
{
    public static readonly IReadOnlyDictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>
    {
        ["Identity"] = typeof(IdentityModule).Assembly,
        ["Courses"] = typeof(CoursesModule).Assembly,
        ["Families"] = typeof(FamiliesModule).Assembly,
        ["Enrollments"] = typeof(EnrollmentsModule).Assembly,
        ["Payments"] = typeof(PaymentsModule).Assembly,
    };

    public static readonly IReadOnlyDictionary<string, string[]> AllowedDependencies = new Dictionary<string, string[]>
    {
        ["Identity"] = [],
        ["Courses"] = [],
        ["Families"] = ["Identity"],
        ["Enrollments"] = ["Courses", "Families"],
        ["Payments"] = ["Families", "Enrollments"],
    };

    public static string PublicNamespace(string module) => $"ClassBook.Modules.{module}.Public";
}
