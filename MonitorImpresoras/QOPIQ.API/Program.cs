using Microsoft.EntityFrameworkCore;
using QOPIQ.Infrastructure;
using QOPIQ.API.Hubs;
using QOPIQ.API.Middleware;
using QOPIQ.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configurar Serilog desde configuración
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// 🔧 Configuración de infraestructura (incluye JWT, Rate Limiting, Health Checks, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// 🎯 Controladores y API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 📚 Swagger con autenticación JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "QOPIQ - Sistema de Monitoreo de Impresoras", 
        Version = "v1.0",
        Description = "API REST para monitoreo de impresoras con autenticación JWT",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "QOPIQ Support",
            Email = "support@qopiq.com"
        }
    });

    // JWT Security para Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 🔄 SignalR Hub Context
// TODO: Implementar IPrinterHubContext correctamente
// builder.Services.AddScoped<IPrinterHubContext, PrinterHubContext>();

var app = builder.Build();

// 🔧 Pipeline de middleware configurado para producción
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QOPIQ API V1.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "QOPIQ API Documentation";
    });
}
else
{
    app.UseHsts(); // HTTPS Strict Transport Security
}

// 🔒 Seguridad y headers
app.UseHttpsRedirection();

// ⚡ Rate Limiting personalizado
app.UseMiddleware<RateLimitingMiddleware>();

// 🌐 CORS
app.UseCors("CorsPolicy");

// 🔐 Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 🚨 Middleware de manejo de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

// 🏥 Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// 🎯 Endpoints
app.MapControllers();
app.MapHub<PrinterHub>("/hubs/printers");

// 🗄️ Inicialización de base de datos
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync(); // Crear base de datos si no existe
        Log.Information("Base de datos inicializada correctamente");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Error al inicializar la base de datos: {Message}", ex.Message);
    }
}

Log.Information("🚀 QOPIQ API iniciada correctamente en {Environment}", app.Environment.EnvironmentName);

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Error crítico al iniciar la aplicación");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


