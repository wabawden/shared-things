using Microsoft.AspNetCore.Identity;

namespace SharedThings.Api.Data.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<Item> Items { get; set; } = [];
    public ICollection<Membership> Memberships { get; set; } = [];
}