using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MonitorImpresoras.API.Models;
using MonitorImpresoras.Application;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure;
using MonitorImpresoras.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar el logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Admin"));
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Monitor de Impresoras API", 
        Version = "v1",
        Description = "API para el monitoreo y gestión de impresoras en red",
        Contact = new OpenApiContact
        {
            Name = "Soporte Técnico",
            Email = "soporte@monitorimpresoras.com"
        }
    });

    // Configuración de autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                     "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                     "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    
    // Configurar la documentación XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Configure AutoMapper
try
{
    // Configurar AutoMapper para buscar perfiles en todos los ensamblados
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    
    // Add application services
    builder.Services.AddApplicationServices(builder.Configuration);
    
    // Add infrastructure services
    builder.Services.AddInfrastructureServices(builder.Configuration);
    
    Console.WriteLine("Servicios configurados correctamente.");
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
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error-development");
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
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitor de Impresoras API V1");
        c.RoutePrefix = string.Empty; // Hacer que Swagger UI esté disponible en la raíz
    });
    
    Console.WriteLine("Modo desarrollo activado.");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Configurar endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    
    // Ruta de verificación de estado
    endpoints.MapHealthChecks("/health");
    
    // Ruta de verificación de estado detallada (solo en desarrollo)
    if (app.Environment.IsDevelopment())
    {
        endpoints.MapHealthChecks("/health/detailed");
    }
});

// Configurar el middleware de manejo de errores
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new 
        { 
            error = "Ha ocurrido un error en el servidor.",
            message = ex.Message,
            stackTrace = app.Environment.IsDevelopment() ? ex.StackTrace : null
        });
    }
});

Console.WriteLine("Iniciando la aplicación...");
try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Error al iniciar la aplicación: {ex.Message}");
    throw;
}
finally
{
    Console.WriteLine("La aplicación se está cerrando...");
}
