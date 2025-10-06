using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data
{
    /// <summary>
    /// Seeder para datos iniciales de tenants
    /// </summary>
    public static class TenantSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Verificar si ya existen tenants
            if (await context.Tenants.AnyAsync())
            {
                return; // Ya hay datos
            }

            var tenants = new[]
            {
                new Tenant
                {
                    Id = Guid.NewGuid(),
                    TenantKey = "contoso",
                    Name = "Contoso Printer Solutions",
                    CompanyName = "Contoso Ltd.",
                    AdminEmail = "admin@contoso.com",
                    Timezone = "America/Mexico_City",
                    Currency = "MXN",
                    IsActive = true,
                    Tier = SubscriptionTier.Professional,
                    MaxPrinters = 100,
                    MaxUsers = 50,
                    MaxPolicies = 20,
                    MaxStorageMB = 5000,
                    EmailNotificationsEnabled = true,
                    SmsNotificationsEnabled = true,
                    LowConsumableThreshold = 15,
                    CriticalConsumableThreshold = 5,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Tenant
                {
                    Id = Guid.NewGuid(),
                    TenantKey = "acme",
                    Name = "ACME Printing Services",
                    CompanyName = "ACME Corp.",
                    AdminEmail = "admin@acme.com",
                    Timezone = "America/New_York",
                    Currency = "USD",
                    IsActive = true,
                    Tier = SubscriptionTier.Enterprise,
                    MaxPrinters = 500,
                    MaxUsers = 200,
                    MaxPolicies = 100,
                    MaxStorageMB = 20000,
                    EmailNotificationsEnabled = true,
                    SmsNotificationsEnabled = false,
                    LowConsumableThreshold = 20,
                    CriticalConsumableThreshold = 10,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Tenant
                {
                    Id = Guid.NewGuid(),
                    TenantKey = "demo",
                    Name = "Demo Company",
                    CompanyName = "Demo Rentals Inc.",
                    AdminEmail = "demo@qopiq.com",
                    Timezone = "America/Mexico_City",
                    Currency = "MXN",
                    IsActive = true,
                    Tier = SubscriptionTier.Free,
                    MaxPrinters = 5,
                    MaxUsers = 3,
                    MaxPolicies = 2,
                    MaxStorageMB = 100,
                    EmailNotificationsEnabled = true,
                    SmsNotificationsEnabled = false,
                    LowConsumableThreshold = 25,
                    CriticalConsumableThreshold = 15,
                    ExpiresAt = DateTime.UtcNow.AddDays(30), // Demo por 30 días
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Tenants.AddRangeAsync(tenants);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Crea datos de ejemplo para un tenant específico
        /// </summary>
        public static async Task SeedTenantDataAsync(ApplicationDbContext context, string tenantKey, IPasswordHasher<User>? passwordHasher = null)
        {
            var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.TenantKey == tenantKey);
            if (tenant == null)
            {
                throw new ArgumentException($"Tenant {tenantKey} not found");
            }

            // Crear empresa de ejemplo
            var company = new Company
            {
                Id = Guid.NewGuid(),
                TenantId = tenantKey,
                Name = $"{tenant.CompanyName} - Main Office",
                TaxId = "RFC123456789",
                Address = "123 Main Street",
                City = "Mexico City",
                State = "CDMX",
                PostalCode = "01000",
                Country = "Mexico",
                Phone = "+52-55-1234-5678",
                Email = tenant.AdminEmail,
                ContactPerson = "John Doe",
                IsActive = true,
                ContractStartDate = DateTime.UtcNow.AddDays(-30),
                ContractEndDate = DateTime.UtcNow.AddYears(1),
                SubscriptionPlan = tenant.Tier.ToString(),
                MaxPrinters = tenant.MaxPrinters,
                MaxProjects = 10,
                MaxUsers = tenant.MaxUsers,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Companies.AddAsync(company);

            // Crear proyecto de ejemplo
            var project = new Project
            {
                Id = Guid.NewGuid(),
                TenantId = tenantKey,
                CompanyId = company.Id,
                Name = "Proyecto Oficina Principal",
                Description = "Monitoreo de impresoras en oficina principal",
                ClientName = "Cliente Demo",
                Address = "456 Client Street",
                City = "Mexico City",
                State = "CDMX",
                PostalCode = "02000",
                ContactPerson = "Jane Smith",
                ContactPhone = "+52-55-9876-5432",
                ContactEmail = "jane@cliente.com",
                StartDate = DateTime.UtcNow.AddDays(-15),
                Status = "Active",
                IsActive = true,
                MonitoringIntervalMinutes = 5,
                EnableRealTimeAlerts = true,
                EnableAutomaticReports = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Projects.AddAsync(project);

            // Crear usuarios de ejemplo
            if (passwordHasher != null)
            {
                var users = new[]
                {
                    new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        TenantId = tenantKey,
                        CompanyId = company.Id,
                        Email = $"admin@{tenantKey}.com",
                        UserName = $"admin@{tenantKey}.com",
                        FirstName = "Admin",
                        LastName = "User",
                        Department = "IT",
                        Role = QopiqRoles.CompanyAdmin,
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        TenantId = tenantKey,
                        CompanyId = company.Id,
                        Email = $"user@{tenantKey}.com",
                        UserName = $"user@{tenantKey}.com",
                        FirstName = "Regular",
                        LastName = "User",
                        Department = "Operations",
                        Role = QopiqRoles.Viewer,
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                foreach (var user in users)
                {
                    user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");
                }

                await context.Users.AddRangeAsync(users);
            }

            // Crear impresora de ejemplo
            var printer = new Printer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantKey,
                ProjectId = project.Id.ToString(),
                Name = "HP LaserJet Pro M404n",
                Model = "HP LaserJet Pro M404n",
                SerialNumber = "DEMO123456",
                IpAddress = "192.168.1.100",
                Location = "Oficina Principal - Piso 1",
                Status = "Online",
                IsOnline = true,
                IsLocalPrinter = false,
                CommunityString = "public",
                SnmpPort = 161,
                BlackTonerLevel = 85,
                PageCount = 15420,
                TotalPagesPrinted = 15420,
                TotalPrintsBlack = 14200,
                TotalPrintsColor = 0,
                TotalCopies = 890,
                TotalScans = 340,
                LastChecked = DateTime.UtcNow.AddMinutes(-2),
                LastSeen = DateTime.UtcNow.AddMinutes(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow
            };

            await context.Printers.AddAsync(printer);
            await context.SaveChangesAsync();
        }
    }
}
