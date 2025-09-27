using FluentValidation.AspNetCore;
using MonitorImpresoras.API.Mapping;
using MonitorImpresoras.API.Middleware;
using MonitorImpresoras.API.Validations;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Application.Services.Interfaces;
using MonitorImpresoras.Domain.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Repositories;
using MonitorImpresoras.Infrastructure.Services.SNMP;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure DbContext - SQLite for development
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors());

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Register Application Services
builder.Services.AddScoped<IAuthService, JwtAuthService>();
builder.Services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();
builder.Services.AddScoped<ISnmpService, SnmpService>();
builder.Services.AddScoped<IAlertEngineService, AlertEngineService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Register new simplified services
builder.Services.AddScoped<MonitorImpresoras.Application.Services.Interfaces.IAlertEngineService, AlertEngineService>();
builder.Services.AddScoped<MonitorImpresoras.Application.Services.Interfaces.IAlertService, AlertService>();
builder.Services.AddScoped<MonitorImpresoras.Application.Services.Interfaces.IPrinterService, PrinterService>();
builder.Services.AddScoped<MonitorImpresoras.Application.Services.Interfaces.IConsumableService, ConsumableService>();

// Register Infrastructure Services
builder.Services.AddScoped<IPrinterRepository, PrinterRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? "default-secret-key"))
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    options.AddPolicy("TechnicianOnly", policy => policy.RequireRole("Technician"));
});

// Configure SignalR
builder.Services.AddSignalR();

// Configure HttpClient for notifications
builder.Services.AddHttpClient();

// Configure SMTP for emails
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register infrastructure services
builder.Services.AddScoped<ISnmpService, SnmpService>();
builder.Services.AddScoped<IWindowsPrinterService, WindowsPrinterService>();
builder.Services.AddScoped<IPrinterMonitoringService, PrinterMonitoringService>();

// Register application services
builder.Services.AddScoped<IPrintJobService, PrintJobService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICostCalculationService, CostCalculationService>();
builder.Services.AddScoped<IPrintingPolicyService, PrintingPolicyService>();

// ========== MULTI-TENANT AND SCHEDULER SERVICES ==========
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
builder.Services.AddScoped<IScheduledReportService, ScheduledReportService>();
builder.Services.AddScoped<IReportExecutionService, ReportExecutionService>();
builder.Services.AddScoped<IReportSchedulerService, ReportSchedulerService>();
builder.Services.AddScoped<IReportTemplateService, ReportTemplateService>();
builder.Services.AddScoped<IIntelligenceEngineService, IntelligenceEngineService>();
builder.Services.AddScoped<IAdvancedNotificationService, AdvancedNotificationService>();

// Register scheduler service as HostedService
builder.Services.AddHostedService<ReportSchedulerService>();

// Register authentication services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePrinterDTOValidator>();

// Add global exception handling
builder.Services.AddTransient<GlobalExceptionHandler>();

// Add controllers
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Monitor de Impresoras API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before other middleware
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Add tenant middleware
app.UseMiddleware<TenantPlanMiddleware>();

// Add global exception handling
app.UseMiddleware<GlobalExceptionHandler>();

// Map SignalR hub
app.MapHub<PrinterHub>("/printerhub");

// Map controllers
app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Apply migrations and seed the database
        await context.Database.MigrateAsync();
        await SeedData.SeedAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
