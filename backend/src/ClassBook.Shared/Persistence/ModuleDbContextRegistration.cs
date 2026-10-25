namespace ClassBook.Shared.Persistence;

/// <summary>Marks a registered module <c>DbContext</c> type, so all of them can be migrated together.</summary>
public sealed record ModuleDbContextRegistration(Type ContextType);
