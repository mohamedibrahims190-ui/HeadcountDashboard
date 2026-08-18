using HeadcountDashboard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HeadcountDashboard.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
        {
            await context.Database.MigrateAsync();

            // Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new[]
                {
                    new Department { Name = "PM1 Process", Code = "PM-1" },
                    new Department { Name = "PM2 Process", Code = "PM-2" },
                    new Department { Name = "Mechanical", Code = "MECH" },
                    new Department { Name = "Electrical", Code = "ELEC" },
                    new Department { Name = "Instrumentation", Code = "INST" },
                    new Department { Name = "WPP", Code = "WPP" },
                    new Department { Name = "ETP", Code = "ETP" },
                    new Department { Name = "Boiler", Code = "BLR" },
                    new Department { Name = "Finishing Warehouse", Code = "FIN-WH" },
                    new Department { Name = "Store", Code = "STORE" },
                    new Department { Name = "HSE", Code = "HSE" },
                    new Department { Name = "HR, Admin, IT", Code = "HR-IT" },
                    new Department { Name = "Security", Code = "SEC" },
                    new Department { Name = "UWW", Code = "UWW" },
                    new Department { Name = "Visitor", Code = "VIS" },
                    new Department { Name = "Contractor", Code = "CONT" }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            var email = configuration["AdminUser:Email"];
            var password = configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Admin user credentials are not configured.");
            }

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create initial user: {errors}");
                }
            }

            const string adminRole = "Admin";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var role = new IdentityRole(adminRole);

                var roleResult = await roleManager.CreateAsync(role);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create role: {errors}");
                }
            }

            existingUser ??= await userManager.FindByEmailAsync(email);

            if (existingUser != null &&
                !await userManager.IsInRoleAsync(existingUser, adminRole))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    existingUser,
                    adminRole);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to assign Admin role: {errors}");
                }
            }
        }
    }
}