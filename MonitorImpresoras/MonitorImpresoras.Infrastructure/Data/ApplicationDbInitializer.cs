using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MonitorImpresoras.Infrastructure.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task SeedAsync(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IConfiguration configuration,
            ILogger logger)
        {
            try
            {
                logger.LogInformation("Iniciando inicialización de la base de datos...");

                // Obtener configuración del administrador
                var adminConfig = configuration.GetSection("AdminUser");
                var adminEmail = adminConfig["Email"] ?? "admin@monitorimpresoras.com";
                var adminPassword = adminConfig["Password"] ?? "Admin123!";

                // Crear roles si no existen
                string[] roles = { "Admin", "User" };
                string[] roleDescriptions =
                {
                    "Administrador del sistema con acceso completo",
                    "Usuario estándar con acceso limitado"
                };

                for (int i = 0; i < roles.Length; i++)
                {
                    var roleName = roles[i];
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        role = new Role
                        {
                            Name = roleName,
                            Description = roleDescriptions[i],
                            IsActive = true
                        };
                        var result = await roleManager.CreateAsync(role);
                        if (result.Succeeded)
                        {
                            logger.LogInformation($"Rol {roleName} creado exitosamente");
                        }
                        else
                        {
                            logger.LogError($"Error al crear el rol {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                // Crear usuario administrador si no existe
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "Administrador",
                        LastName = "Sistema",
                        IsActive = true,
                        PhoneNumber = "+1234567890",
                        PhoneNumberConfirmed = true
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                        logger.LogInformation("Usuario administrador creado exitosamente");
                    }
                    else
                    {
                        logger.LogError($"Error al crear el usuario administrador: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                logger.LogInformation("Inicialización de la base de datos completada");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error durante la inicialización de la base de datos");
                throw; // Relanzar para que la aplicación no inicie si hay un error crítico
            }
        }

        /// <summary>
        /// Inicialización mínima de datos funcionales (Printers) para desarrollo.
        /// Aplica migraciones y agrega datos si la tabla está vacía.
        /// </summary>
        public static async Task InitializeAsync(ApplicationDbContext db, CancellationToken ct = default)
        {
            await db.Database.MigrateAsync(ct);

            if (!await db.Set<Printer>().AsNoTracking().AnyAsync(ct))
            {
                db.Set<Printer>().AddRange(
                    new Printer { Name = "HP LaserJet 402dn", IpAddress = "192.168.1.10", Location = "Front Desk" },
                    new Printer { Name = "Kyocera ECOSYS M2540", IpAddress = "192.168.1.11", Location = "Accounting" }
                );
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
