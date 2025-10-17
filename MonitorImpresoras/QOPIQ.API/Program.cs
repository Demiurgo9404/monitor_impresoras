using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QOPIQ.Application.Interfaces;
using QOPIQ.Application.Services;
using QOPIQ.Infrastructure;
using QOPIQ.API.Hubs;
using QOPIQ.API.Middleware;
using QOPIQ.Domain.Entities;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 🔹 Base de datos y configuración de Identity
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// 🔹 Servicios de aplicación - AuthService ya está registrado en Infrastructure
// Otros servicios se registrarán cuando sean necesarios

// 🔹 Configuración JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default-key-for-development-only"))
        };

        // 🔹 Habilitar JWT para SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && 
                    path.StartsWithSegments("/hubs/printers"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
        b.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod()
         .WithExposedHeaders("X-Pagination"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "QOPIQ - Sistema de Monitoreo de Impresoras", 
        Version = "v1"
    });

    // Agregar JWT a Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
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

// 🔹 SignalR
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    });

// Registrar IPrinterHub
builder.Services.AddScoped<IPrinterHubContext, PrinterHubContext>();

// Configurar CORS para SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(host => true)
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configurar el pipeline de la aplicación
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QOPIQ API V1");
        c.RoutePrefix = string.Empty; // Para servir Swagger en la raíz
    });
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Middleware de manejo de errores personalizado
app.UseMiddleware<ErrorHandlingMiddleware>();

// Mapear controladores y hub SignalR
app.MapControllers();
app.MapHub<PrinterHub>("/hubs/printers");

// Database initialization (commented out until DbContext is properly configured)
// if (app.Environment.IsDevelopment())
// {
//     using var scope = app.Services.CreateScope();
//     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     await context.Database.MigrateAsync();
// }

Log.Information("QOPIQ API iniciada correctamente");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error al iniciar la aplicación");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


