using Microsoft.Extensions.Options;

namespace ClassBook.Shared.Time;

/// <summary>Converts UTC instants to the organization's local time. Use only at the edges (UI, emails, due dates).</summary>
public sealed class OrganizationClock(TimeProvider timeProvider, IOptions<OrganizationTimeOptions> options)
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);

    /// <summary>Today's date in the organization time zone.</summary>
    public DateOnly Today => DateOnly.FromDateTime(ToLocal(timeProvider.GetUtcNow()).DateTime);

    public DateTimeOffset ToLocal(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, _timeZone);
}
