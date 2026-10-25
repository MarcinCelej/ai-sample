namespace ClassBook.Shared.Security;

/// <summary>The user of the current request.</summary>
public interface ICurrentUser
{
    /// <summary>The Identity user ID, or <c>null</c> for anonymous requests and background work.</summary>
    Guid? UserId { get; }
}
