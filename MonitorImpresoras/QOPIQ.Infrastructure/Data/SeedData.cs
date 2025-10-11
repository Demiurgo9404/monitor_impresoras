using Microsoft.AspNetCore.Identity;
using QOPIQ.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QOPIQ.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            // Crear roles si no existen
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            // Crear usuario administrador si no existe
            var adminEmail = "admin@qopiq.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Agregar datos de prueba para impresoras
            if (!context.Printers.Any())
            {
                var printers = new List<Printer>
                {
                    new Printer
                    {
                        Id = Guid.NewGuid(),
                        Name = "HP LaserJet Pro M404",
                        IpAddress = "192.168.1.100",
                        Model = "HP LaserJet Pro M404",
                        Status = "Online",
                        Location = "Piso 1 - Oficina 101",
                        IsActive = true,
                        LastChecked = DateTime.UtcNow
                    },
                    new Printer
                    {
                        Id = Guid.NewGuid(),
                        Name = "Canon ImageCLASS MF644Cdw",
                        IpAddress = "192.168.1.101",
                        Model = "Canon MF644Cdw",
                        Status = "Online",
                        Location = "Piso 2 - Sala de Reuniones",
                        IsActive = true,
                        LastChecked = DateTime.UtcNow
                    },
                    new Printer
                    {
                        Id = Guid.NewGuid(),
                        Name = "Epson WorkForce Pro WF-4740",
                        IpAddress = "192.168.1.102",
                        Model = "Epson WF-4740",
                        Status = "Offline",
                        Location = "Piso 1 - Recepción",
                        IsActive = true,
                        LastChecked = DateTime.UtcNow.AddHours(-1)
                    }
                };

                await context.Printers.AddRangeAsync(printers);
                await context.SaveChangesAsync();
            }
        }
    }
}
