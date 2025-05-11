
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
