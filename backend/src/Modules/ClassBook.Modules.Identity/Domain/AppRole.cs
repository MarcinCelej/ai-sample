using Microsoft.AspNetCore.Identity;

namespace ClassBook.Modules.Identity.Domain;

/// <summary>An application role; names are defined in <c>ClassBook.Shared.Security.Roles</c>.</summary>
internal sealed class AppRole : IdentityRole<Guid>
{
}
