using ClassBook.Shared.Time;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace ClassBook.UnitTests.Shared;

public class OrganizationClockTests
{
    [Fact]
    public void Today_UsesOrganizationTimeZone()
    {
        // 23:30 UTC on 25 October 2026 is already 26 October in Warsaw (UTC+1 after the DST change).
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 10, 25, 23, 30, 0, TimeSpan.Zero));
        var clock = new OrganizationClock(time, Options.Create(new OrganizationTimeOptions()));

        clock.Today.ShouldBe(new DateOnly(2026, 10, 26));
    }

    [Fact]
    public void ToLocal_ConvertsUtcInstantToOrganizationOffset()
    {
        var clock = new OrganizationClock(new FakeTimeProvider(), Options.Create(new OrganizationTimeOptions()));

        var local = clock.ToLocal(new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero));

        local.Offset.ShouldBe(TimeSpan.FromHours(2));
        local.Hour.ShouldBe(12);
    }
}
