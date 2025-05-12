
using EventHorizon.DataAccess.Data;
using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventHorizon.DataAccess.Persistence;

public class Seeder(ApplicationDbContext dbContext,
    UserManager<ApplicationUser> _userManager) : ISeeder
{
    public async Task Seed()
    {
        if (dbContext.Database.GetPendingMigrations().Any())
            await dbContext.Database.MigrateAsync();

        if (await dbContext.Database.CanConnectAsync())
        {
            // Seeding the roles && users
            if (!dbContext.Roles.Any())
            {
                // Seeding the roles
                var roles = GetRoles();
                await dbContext.Roles.AddRangeAsync(roles);
                await dbContext.SaveChangesAsync();

                // Create the admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "admin123",
                    Email = "admin@eventhorizon.com",
                    PhoneNumber = "01013274668",
                    EmailConfirmed = true
                };

                // Create owner of event user
                var ownerUser = new ApplicationUser
                {
                    UserName = "owner123",
                    Email = "owner@eventhorizon.com",
                    PhoneNumber = "01013274668",
                    EmailConfirmed = true
                };

                // Create usual user
                var usualUser = new ApplicationUser
                {
                    UserName = "test123",
                    Email = "test@eventhorizon.com",
                    PhoneNumber = "01013274668",
                    EmailConfirmed = true
                };

                var createAdminResult = await _userManager.CreateAsync(adminUser, "Admin123*");
                var createOwnerResult = await _userManager.CreateAsync(ownerUser, "Owner123*");
                var createUserResult = await _userManager.CreateAsync(usualUser, "Test123*");

                // Adding the rules
                if (createAdminResult.Succeeded && createOwnerResult.Succeeded && createUserResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, RolesConstant.Admin);
                    await _userManager.AddToRoleAsync(ownerUser, RolesConstant.Owner);
                    await _userManager.AddToRoleAsync(usualUser, RolesConstant.User);

                }
                else
                {
                    // Optional: log errors
                    foreach (var error in createAdminResult.Errors)
                        Console.WriteLine($"Error creating admin user: {error.Description}");
                    foreach (var error in createOwnerResult.Errors)
                        Console.WriteLine($"Error creating owner user: {error.Description}");
                    foreach (var error in createUserResult.Errors)
                        Console.WriteLine($"Error creating usual user: {error.Description}");

                }
            }


            // Seeding the categories
            if (!dbContext.Categories.Any())
            {
                var categories = GetCategories();
                await dbContext.Categories.AddRangeAsync(categories);
                await dbContext.SaveChangesAsync();
            }

            // Seeding the events
            if (!dbContext.Events.Any())
            {
                var events = GetEvents();
                await dbContext.Events.AddRangeAsync(events);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private IEnumerable<IdentityRole> GetRoles()
    {
        List<IdentityRole> roles = [
                new(RolesConstant.User){
                        NormalizedName = RolesConstant.User.ToUpper()
                    },
                    new(RolesConstant.Admin){
                        NormalizedName = RolesConstant.Admin.ToUpper()
                    },
                    new(RolesConstant.Owner)
                    {
                        NormalizedName = RolesConstant.Owner.ToUpper()
                    }
            ];
        return roles;
    }
    private IEnumerable<Category> GetCategories()
    {
        List<Category> categories = [
                new Category { Name = "Tech" },
                new Category { Name = "Music" },
                new Category { Name = "Health" }
            ];
        return categories;
    }
    private IEnumerable<Event> GetEvents()
    {
        var techCategoryId = dbContext.Categories.First(c => c.Name == "Tech").Id;
        var adminUser = dbContext.Users.First(u => u.Email == "admin@eventhorizon.com");

        var events = new List<Event>();
        for (int i = 1; i <= 50; i++)
        {
            events.Add(new Event
            {
                Name = $"Tech Summit {2025 + (i % 3)} - Edition {i}",
                Description = $"Annual tech event number {i}.",
                Venue = $"Conference Hall {i % 10 + 1}, Cairo",
                Price = 100 + i,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                EventDate = DateTime.Now.AddDays(i * 2),
                CategoryId = techCategoryId,
                OwnerId = adminUser.Id
            });
        }

        return events;
    }

}
