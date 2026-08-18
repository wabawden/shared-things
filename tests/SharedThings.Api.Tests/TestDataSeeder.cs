using SharedThings.Api.Data;
using SharedThings.Api.Data.Entities;

namespace SharedThings.Api.Tests;

public static class TestDataSeeder
{
    public static async Task SeedAsync(
        SharedThingsDbContext dbContext)
    {
        var bill = new ApplicationUser
        {
            Id = SeedIds.Bill,
            UserName = "bill@example.test",
            NormalizedUserName = "BILL@EXAMPLE.TEST",
            Email = "bill@example.test",
            NormalizedEmail = "BILL@EXAMPLE.TEST",
            EmailConfirmed = true,
            DisplayName = "Bill"
        };

        var alex = new ApplicationUser
        {
            Id = SeedIds.Alex,
            UserName = "alex@example.test",
            NormalizedUserName = "ALEX@EXAMPLE.TEST",
            Email = "alex@example.test",
            NormalizedEmail = "ALEX@EXAMPLE.TEST",
            EmailConfirmed = true,
            DisplayName = "Alex"
        };

        var casey = new ApplicationUser
        {
            Id = SeedIds.Casey,
            UserName = "casey@example.test",
            NormalizedUserName = "CASEY@EXAMPLE.TEST",
            Email = "casey@example.test",
            NormalizedEmail = "CASEY@EXAMPLE.TEST",
            EmailConfirmed = true,
            DisplayName = "Casey"
        };

        var neighbourhood = new Community(
            SeedIds.Neighbourhood,
            "Our Neighbourhood");

        dbContext.AddRange(
            bill,
            alex,
            casey,
            neighbourhood);

        dbContext.Memberships.AddRange(
            new Membership(
                SeedIds.Bill,
                SeedIds.Neighbourhood),
            new Membership(
                SeedIds.Alex,
                SeedIds.Neighbourhood));

        dbContext.Items.AddRange(
            new Item(
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000001"),
                SeedIds.Bill,
                "Cordless drill",
                "18V drill with charger and a small set of bits.",
                "Good"),
            new Item(
                Guid.Parse(
                    "30000000-0000-0000-0000-000000000002"),
                SeedIds.Alex,
                "Wallpaper steamer",
                "Compact wallpaper steamer.",
                "Well used"));

        await dbContext.SaveChangesAsync();
    }
}