namespace ClassBook.IntegrationTests.Infrastructure;

public sealed record TestUser(Guid Id, params string[] Roles)
{
    public static TestUser Administrator() => new(Guid.NewGuid(), Shared.Security.Roles.Administrator);

    public static TestUser Guardian() => new(Guid.NewGuid(), Shared.Security.Roles.Guardian);
}
