using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Domain.Interfaces.Services;
using QOPIQ.Infrastructure.Configuration;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Repositories;
using QOPIQ.Infrastructure.Services;

namespace QOPIQ.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Configuración de opciones
            services.Configure<SnmpOptions>(configuration.GetSection("Snmp"));

            // Base de datos
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Configuración de Identity
            var identityBuilder = services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            });

            identityBuilder.AddRoles<IdentityRole<Guid>>();
            identityBuilder.AddEntityFrameworkStores<ApplicationDbContext>();
            identityBuilder.AddDefaultTokenProviders();
            identityBuilder.AddSignInManager<SignInManager<User>>();
            identityBuilder.AddUserManager<UserManager<User>>();
            identityBuilder.AddRoleManager<RoleManager<IdentityRole<Guid>>>();

            // Repositorios genéricos
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Repositorios específicos
            services.AddScoped<IPrinterRepository, PrinterRepository>();

            // Servicios
            services.AddScoped<ISnmpService, SnmpService>();
            services.AddScoped<IJwtService, JwtService>();

            // Unit of Work - Registrar con resolución manual de dependencias
            services.AddScoped<IUnitOfWork>(provider => 
            {
                var context = provider.GetRequiredService<ApplicationDbContext>();
                var printerRepository = provider.GetRequiredService<IPrinterRepository>();
                return new UnitOfWork(context, printerRepository);
            });

            // Configuración para desarrollo
            if (environment.IsDevelopment())
            {
                // Configuraciones específicas para desarrollo
            }

            return services;
        }
    }
}

