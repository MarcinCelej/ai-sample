namespace ClassBook.Shared.Persistence;

/// <summary>
/// An entity with audit fields. <see cref="AuditInterceptor"/> fills them; application code never sets them.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }

    Guid? CreatedBy { get; }

    DateTimeOffset? UpdatedAt { get; }

    Guid? UpdatedBy { get; }
}
