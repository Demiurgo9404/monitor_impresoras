using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MonitorImpresoras.API.Filters;
using MonitorImpresoras.API.Middleware;
using MonitorImpresoras.API.Models;
using MonitorImpresoras.Application;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure;
using MonitorImpresoras.Infrastructure.Data;
using Serilog;
using Serilog.Events;

// Configurar el constructor de la aplicación
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// Configurar Serilog para logging estructurado
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando la aplicación...");
    
    // Configurar el host para usar Serilog
    builder.Host.UseSerilog();
    
    // Configurar el contexto de la base de datos
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
    
    // Configurar CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
    });
    
    // Configurar controladores con opciones personalizadas
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
        options.SuppressAsyncSuffixInActionNames = false;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
    
    // Configurar autenticación JWT
    var jwtSettings = configuration.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
    
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });
        
    // Configurar Identity
    builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
    // Configurar AutoMapper
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    
    // Configurar servicios de aplicación e infraestructura
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(configuration);
    
    // Configurar Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");
    
    // Configurar Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "Monitor de Impresoras API", 
            Version = "v1",
            Description = "API para el monitoreo y gestión de Impresoras",
            Contact = new OpenApiContact
            {
                Name = "Soporte Técnico",
                Email = "soporte@monitorimpresoras.com"
            }
        });
        
        // Configurar autenticación JWT en Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                          "Ingrese 'Bearer' [espacio] y luego su token en el campo de texto.\r\n\r\n" +
                          "Ejemplo: \"Bearer 12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
        c.OperationFilter<SecurityRequirementsOperationFilter>();
    });
}
catch (Exception ex)
{
    Console.WriteLine($"Error al configurar los servicios: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
    throw;
}

var app = builder.Build();

// Configurar el manejador global de excepciones
// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitor de Impresoras API V1");
        c.RoutePrefix = "api/docs";
    });
    
    // Solo en desarrollo, aplicar migraciones automáticamente
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            Log.Information("Migraciones aplicadas correctamente.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al aplicar migraciones");
        }
    }
}
else
{
    app.UseExceptionHandler("/error");
}

// Crear roles y usuario administrador por defecto
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Crear roles si no existen
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        
        // Crear usuario administrador por defecto si no existe
        var adminUser = await userManager.FindByEmailAsync("admin@monitorimpresoras.com");
        if (adminUser == null)
        {
            var admin = new User
            {
                UserName = "admin@monitorimpresoras.com",
                Email = "admin@monitorimpresoras.com",
                FirstName = "Administrador",
                LastName = "Sistema",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(admin, "Admin123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos con los roles y usuarios por defecto.");
    }
}

// Configurar el pipeline de solicitud HTTP
app.UseHttpsRedirection();
app.UseRouting();

// Habilitar CORS
app.UseCors("AllowSpecificOrigins");

// Configurar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Configurar endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    
    // Health checks
    endpoints.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    // Ruta de verificación de estado detallada (solo en desarrollo)
    if (app.Environment.IsDevelopment())
    {
        endpoints.MapHealthChecks("/health/detailed", new HealthCheckOptions
        {
            Predicate = _ => true,
            AllowCachingResponses = false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }
});

// Configurar el middleware de manejo de errores personalizado
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configurar el manejador de errores para entornos de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitor de Impresoras API V1");
        c.RoutePrefix = "swagger";
    });
}

// Configurar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Configurar endpoints
app.MapControllers();

// Configurar health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

Log.Information("Aplicación iniciada correctamente");

// Ejecutar la aplicación
try
{{ ... }}
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error al iniciar la aplicación");
    throw;
}
finally
{
    Log.Information("La aplicación se está cerrando...");
    Log.CloseAndFlush();
}
