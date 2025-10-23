using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Data.Repositories;
using QOPIQ.Infrastructure.Services;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Domain.Settings;
using QOPIQ.Infrastructure.Configuration;

// Use fully qualified names to avoid ambiguity
using IPrinterRepository = QOPIQ.Domain.Interfaces.Repositories.IPrinterRepository;
using ISnmpService = QOPIQ.Domain.Interfaces.Services.ISnmpService;
using IPrinterMonitoringService = QOPIQ.Application.Interfaces.IPrinterMonitoringService;
using IUnitOfWork = QOPIQ.Domain.Interfaces.IUnitOfWork;
using IAuthService = QOPIQ.Application.Interfaces.IAuthService;

namespace QOPIQ.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // 🔧 Configuración de Base de Datos
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (connectionString?.Contains(".db") == true)
            {
                // SQLite para desarrollo
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(connectionString, 
                        b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
            }
            else
            {
                // PostgreSQL para producción
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString,
                        b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                              .EnableRetryOnFailure(
                                  maxRetryCount: 5,
                                  maxRetryDelay: TimeSpan.FromSeconds(30),
                                  errorCodesToAdd: null)));
            }

            // 🔄 Configuración de Redis para caching (opcional)
            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "QOPIQ_";
                });

                // SignalR con Redis backplane para escalabilidad
                services.AddSignalR().AddStackExchangeRedis(redisConnection, options =>
                {
                    options.Configuration.ChannelPrefix = "QOPIQ_SignalR_";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
                services.AddSignalR();
            }

            // 📦 Repositorios y Unit of Work
            services.AddScoped<Domain.Interfaces.Repositories.IUserRepository, UserRepository>();
            services.AddScoped<IPrinterRepository, PrinterRepository>();
            services.AddScoped<Domain.Interfaces.Repositories.ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<Domain.Interfaces.Repositories.IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUnitOfWork, Data.UnitOfWork>();

            // 🌐 Configuración de SNMP v3 Seguro
            services.Configure<SnmpOptions>(configuration.GetSection("Snmp"));
            services.AddScoped<ISnmpService, SnmpService>();
            
            // 🖨️ Servicios de aplicación
            services.AddScoped<IPrinterMonitoringService>(provider =>
                new PrinterMonitoringService(
                    provider.GetRequiredService<IUnitOfWork>(),
                    provider.GetRequiredService<ISnmpService>(),
                    provider.GetRequiredService<IOptions<SnmpOptions>>()
                ));

            // 🔐 Configuración de JWT con validación estricta
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings?.SecretKey != null)
            {
                var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

                services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = environment.EnvironmentName == "Production";
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = true
                    };
                });
            }

            // ⚡ Rate Limiting - Se configurará en el API layer
            // TODO: Implementar Rate Limiting en Program.cs del API

            // 🏥 Health Checks para monitoreo
            var healthChecks = services.AddHealthChecks()
                .AddNpgSql(
                    configuration.GetConnectionString("DefaultConnection"),
                    name: "database",
                    tags: new[] { "ready" });

            if (!string.IsNullOrEmpty(redisConnection))
            {
                healthChecks.AddRedis(redisConnection, "redis", tags: new[] { "ready" });
            }

            // 🔧 Servicios de infraestructura
            services.AddHttpContextAccessor();
            services.AddScoped<Application.Interfaces.ICurrentUserService, Services.CurrentUserService>();
            services.AddScoped<IAuthService, AuthService>();
            
            // 📝 Logging configurado
            services.AddLogging();

            // 🌐 CORS configuración segura
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    var allowedOrigins = configuration["AllowedOrigins"]?.Split(';') ?? new[] { "https://localhost:5001" };
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }
    }
}
