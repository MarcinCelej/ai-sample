namespace ClassBook.Shared.Persistence;

/// <summary>A module's <c>DbContext</c>. Each module owns one database schema.</summary>
public interface IModuleDbContext
{
    /// <summary>The database schema of the module, e.g. <c>courses</c>.</summary>
    static abstract string Schema { get; }
}
