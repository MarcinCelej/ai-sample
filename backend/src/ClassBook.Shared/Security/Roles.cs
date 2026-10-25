namespace ClassBook.Shared.Security;

/// <summary>Identity role names. Endpoints never use them directly; they use <see cref="Policies"/>.</summary>
public static class Roles
{
    public const string Administrator = nameof(Administrator);
    public const string Instructor = nameof(Instructor);
    public const string Guardian = nameof(Guardian);
    public const string Participant = nameof(Participant);
}
