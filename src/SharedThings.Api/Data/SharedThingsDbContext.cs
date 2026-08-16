using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Data;

public sealed class SharedThingsDbContext(
    DbContextOptions<SharedThingsDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>(options)
{
    
    public DbSet<Community> Communities => Set<Community>();

    public DbSet<Membership> Memberships => Set<Membership>();

    public DbSet<Item> Items => Set<Item>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(x => x.DisplayName)
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Entity<Community>(community =>
        {
            community.HasKey(x => x.Id);

            community.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Entity<Item>(item =>
        {
            item.HasKey(x => x.Id);

            item.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            item.Property(x => x.Description)
                .HasMaxLength(1_000)
                .IsRequired();

            item.Property(x => x.Condition)
                .HasMaxLength(100)
                .IsRequired();

            item.HasOne(x => x.Owner)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Membership>(membership =>
        {
            membership.HasKey(x => new
            {
                x.UserId,
                x.CommunityId
            });

            membership.HasOne(x => x.User)
                .WithMany(x => x.Memberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            membership.HasOne(x => x.Community)
                .WithMany(x => x.Memberships)
                .HasForeignKey(x => x.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}