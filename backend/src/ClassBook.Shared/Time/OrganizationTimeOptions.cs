using System.ComponentModel.DataAnnotations;

namespace ClassBook.Shared.Time;

public sealed class OrganizationTimeOptions
{
    public const string SectionName = "Organization";

    /// <summary>IANA time zone of the organization, used for schedules, due dates and emails.</summary>
    [Required]
    public string TimeZoneId { get; set; } = "Europe/Warsaw";
}
