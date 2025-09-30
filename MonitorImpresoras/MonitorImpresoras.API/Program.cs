using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Serilog;
using Serilog.Events;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.API.Filters;
using MonitorImpresoras.API.Middleware;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MonitorImpresoras.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MonitorImpresoras")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/app_log.txt",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10 * 1024 * 1024) // 10MB por archivo
    .WriteTo.File("logs/errors-.txt",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuración de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración de Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuración de JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT Key no configurada"));

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configuración de autorización
builder.Services.AddAuthorization(options =>
{
    // Políticas basadas en roles
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireManager", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("RequireUser", policy => policy.RequireRole("Admin", "Manager", "User"));

    // Políticas basadas en claims
    options.AddPolicy("CanManagePrinters", policy => policy.RequireClaim("printers.manage", "true"));
    options.AddPolicy("CanViewReports", policy => policy.RequireClaim("reports.view", "true"));
    options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("users.manage", "true"));
    options.AddPolicy("CanViewAuditLogs", policy => policy.RequireClaim("audit.view", "true"));

    // Política para usuarios activos
    options.AddPolicy("ActiveUser", policy => policy.RequireClaim("IsActive", "true"));

    // Política para lectura de impresoras (Admin o usuarios con permiso)
    options.AddPolicy("CanReadPrinters", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.HasClaim("ManagePrinters", "true") ||
            context.User.HasClaim("ReadPrinters", "true")));

    // Política para escritura de impresoras (solo Admin o usuarios con permiso específico)
    options.AddPolicy("CanWritePrinters", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.HasClaim("ManagePrinters", "true")));
});

// Configuración de autorización personalizada (omitida temporalmente en baseline)

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",    // React dev server
            "http://localhost:5173",   // Vite dev server
            "http://localhost:4200",   // Angular dev server
            "https://localhost:3000",  // HTTPS React
            "https://localhost:5173",  // HTTPS Vite
            "https://localhost:4200"   // HTTPS Angular
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });

    // Política más permisiva para desarrollo
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Explorers y Swagger (bloque único y bien formado)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "MonitorImpresoras API",
        Description = "API para monitoreo y gestión de impresoras"
    });

    var jwt = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };

    c.AddSecurityDefinition("Bearer", jwt);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwt, Array.Empty<string>() } });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configuración de controladores
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

// Registro mínimo de servicios para baseline (evitar dependencias no esenciales)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MonitorImpresoras.Application.Interfaces.IPrinterQueryService,
    MonitorImpresoras.Infrastructure.Services.PrinterQueryService>();

// AutoMapper y FluentValidation deshabilitados temporalmente para baseline
// builder.Services.AddAutoMapper(typeof(PrinterProfile));
// builder.Services.AddControllers().AddFluentValidation(...);

// Quartz deshabilitado temporalmente para baseline
// builder.Services.AddQuartz(...);
// builder.Services.AddQuartzHostedService(...);

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitor Impresoras API V1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
        c.OAuthClientId("swagger-ui");
        c.OAuthClientSecret("swagger-ui-secret");
        c.OAuthUsePkce();
    });
}

// Middleware de manejo global de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de salud
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// Inicialización de la base de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();

        // Aplicar migraciones
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }

        // Inicializar datos
        await ApplicationDbInitializer.SeedAsync(userManager, roleManager,
            app.Configuration, app.Logger);

        // Seed mínimo de impresoras en entorno de desarrollo
        var env = services.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            await ApplicationDbInitializer.InitializeAsync(context);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error al inicializar la base de datos");
    }
}

try
{
    Log.Information("Aplicación iniciándose");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error fatal al iniciar la aplicación");
}
finally
{
    Log.CloseAndFlush();
}
