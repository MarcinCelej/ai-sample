using Microsoft.AspNetCore.Identity;

namespace ClassBook.Modules.Identity.Domain;

/// <summary>A login account. Personal data of family members lives in the Families module.</summary>
internal sealed class AppUser : IdentityUser<Guid>
{
}
