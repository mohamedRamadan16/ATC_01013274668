
using EventHorizon.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventHorizon.DataAccess.Persistence;

public class Seeder(ApplicationDbContext dbContext) : ISeeder
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
