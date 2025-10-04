using PrinterAgent.Core.Models;
using PrinterAgent.Core.Services;
using Microsoft.Extensions.Options;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/agent-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog();

// Configurar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PrinterAgent API", Version = "v1" });
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar configuración del agente
builder.Services.Configure<AgentConfiguration>(
    builder.Configuration.GetSection("Agent"));

// Registrar servicios del agente
builder.Services.AddHttpClient<ICentralCommunicationService, CentralCommunicationService>();
builder.Services.AddSingleton<INetworkScannerService, NetworkScannerService>();
builder.Services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configurar pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

// Endpoints básicos
app.MapGet("/", () => new
{
    service = "PrinterAgent",
    version = "1.0.0",
    status = "running",
    timestamp = DateTime.UtcNow
});

// Iniciar el agente
var orchestrator = app.Services.GetRequiredService<IAgentOrchestrator>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await orchestrator.StartAsync();
        Log.Information("PrinterAgent iniciado exitosamente");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Error crítico iniciando PrinterAgent");
        Environment.Exit(1);
    }
});

lifetime.ApplicationStopping.Register(async () =>
{
    try
    {
        await orchestrator.StopAsync();
        Log.Information("PrinterAgent detenido correctamente");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error deteniendo PrinterAgent");
    }
});

Log.Information("PrinterAgent API iniciando...");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error crítico en PrinterAgent API");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
