using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QOPIQ.Application.Interfaces;
using QOPIQ.Application.Services;
using QOPIQ.Infrastructure;
using QOPIQ.API.Hubs;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 🔹 Base de datos
builder.Services.AddInfrastructure(builder.Configuration);

// 🔹 Servicios de aplicación
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 🔹 Configuración JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
builder.Services.AddSignalR();

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

// Seed de datos iniciales (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    
    // Aplicar migraciones pendientes
    await context.Database.MigrateAsync();
    
    // Seed de datos iniciales
    await SeedData.InitializeAsync(context, userManager, roleManager);
}

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


