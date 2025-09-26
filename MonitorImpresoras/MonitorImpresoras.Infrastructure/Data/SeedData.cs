using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed roles
            string[] roleNames = { "Admin", "User", "Technician" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed admin user
            var adminEmail = "admin@monitorimpresoras.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createAdmin = await userManager.CreateAsync(admin, "Admin@123");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Seed test technician
            var techEmail = "tech@monitorimpresoras.com";
            var techUser = await userManager.FindByEmailAsync(techEmail);
            
            if (techUser == null)
            {
                var tech = new User
                {
                    UserName = "technician",
                    Email = techEmail,
                    FirstName = "Tech",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createTech = await userManager.CreateAsync(tech, "Tech@123");
                if (createTech.Succeeded)
                {
                    await userManager.AddToRoleAsync(tech, "Technician");
                }
            }

            // Seed test regular user
            var userEmail = "user@monitorimpresoras.com";
            var regularUser = await userManager.FindByEmailAsync(userEmail);
            
            if (regularUser == null)
            {
                var user = new User
                {
                    UserName = "user",
                    Email = userEmail,
                    FirstName = "Regular",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createUser = await userManager.CreateAsync(user, "User@123");
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }
    }
}
