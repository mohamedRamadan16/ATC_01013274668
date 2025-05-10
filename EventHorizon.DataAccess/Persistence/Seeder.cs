
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
            // Seeding the roles
            if (!dbContext.Roles.Any())
            {
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

                var createResult = await _userManager.CreateAsync(adminUser, "Admin123*");

                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, RolesConstant.Admin);
                }
                else
                {
                    // Optional: log errors
                    foreach (var error in createResult.Errors)
                    {
                        Console.WriteLine($"Error creating admin user: {error.Description}");
                    }
                }
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

}
