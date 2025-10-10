using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QOPIQ.API.Middleware;
using QOPIQ.Application;
using QOPIQ.Application.Interfaces.MultiTenancy;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure;
using QOPIQ.Infrastructure.Data;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog básico
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar servicios básicos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// Configurar servicios multi-tenant
builder.Services.AddScoped<ITenantAccessor, TenantAccessor>();
builder.Services.AddScoped<ITenantResolver, TenantResolver>();

// Configurar servicios de autenticación
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Configurar servicios de negocio
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// Configurar servicios de reportes
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportDataService, ReportDataService>();
builder.Services.AddScoped<IPdfReportGenerator, PdfReportGenerator>();
builder.Services.AddScoped<IExcelReportGenerator, ExcelReportGenerator>();
builder.Services.AddScoped<IScheduledReportService, ScheduledReportService>();

// Configurar servicio de email
builder.Services.AddScoped<IEmailService, EmailService>();

// Configurar servicio de background para reportes programados
builder.Services.AddHostedService<ReportSchedulerService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "QOPIQ - Sistema de Monitoreo de Impresoras", 
        Version = "v1",
        Description = "API Multi-Tenant para monitoreo de impresoras"
    });
    
    // Agregar header de tenant a Swagger
    c.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
    {
        Description = "Tenant ID header",
        Name = "X-Tenant-Id",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    
    // Agregar JWT a Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Tenant"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar base de datos en memoria para desarrollo
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("QOPIQDb"));

// Configurar JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default-key-for-development-only";
var key = Encoding.ASCII.GetBytes(jwtKey);

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
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Configurar Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configurar servicios de aplicación
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configurar pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Middleware multi-tenant (antes de autenticación)
app.UseTenantResolution();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint de prueba
app.MapGet("/", () => new { 
    message = "Monitor de Impresoras API funcionando correctamente", 
    version = "1.0.0",
    timestamp = DateTime.UtcNow 
});

app.MapGet("/api/status", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
});

app.MapGet("/api/printers/test", () => new { 
    message = "Endpoint de impresoras funcionando",
    printers = new[] {
        new { id = Guid.NewGuid(), name = "Impresora HP LaserJet", status = "Online" },
        new { id = Guid.NewGuid(), name = "Impresora Canon Pixma", status = "Offline" }
    },
});

app.MapControllers();

// Seed de datos iniciales
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    
    await TenantSeeder.SeedAsync(context);
    
    // Seed datos de ejemplo para tenants con usuarios
    await TenantSeeder.SeedTenantDataAsync(context, "demo", passwordHasher);
    await TenantSeeder.SeedTenantDataAsync(context, "contoso", passwordHasher);
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


