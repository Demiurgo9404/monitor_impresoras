using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using QOPIQ.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración (API + Hub desde appsettings)
builder.Services.Configure<FrontendOptions>(builder.Configuration);

// HttpClient para la API
builder.Services.AddHttpClient("QOPIQ.API", (sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<FrontendOptions>>().Value;
    client.BaseAddress = new Uri(opt.ApiBaseUrl);
});

// Auth / State
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// Servicios de app
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPrinterService, PrinterService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

// Options para inyectar configuración
public class FrontendOptions
{
    public string ApiBaseUrl { get; set; } = "https://localhost:5001/api/";
    public string SignalRHub { get; set; } = "https://localhost:5001/hubs/printers";
}
