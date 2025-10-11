using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;
using QOPIQ.Frontend.Models;
using QOPIQ.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace QOPIQ.Frontend.Services;

public class PrinterService : IPrinterService
{
    private readonly IHttpClientFactory _factory;
    private readonly ProtectedSessionStorage _storage;
    private readonly FrontendOptions _opt;
    private readonly IAuthService _authService;
    private readonly HttpClient _http;

    public PrinterService(IHttpClientFactory factory,
        ProtectedSessionStorage storage,
        IOptions<FrontendOptions> opt,
        IAuthService authService)
    {
        _factory = factory;
        _storage = storage;
        _opt = opt.Value;
        _authService = authService;
        _http = _factory.CreateClient("QOPIQ.API");
    }

    private async Task<HttpClient> ClientAsync()
    {
        var http = _factory.CreateClient("QOPIQ.API");
        var token = await _authService.GetTokenAsync(_storage);
        if (!string.IsNullOrWhiteSpace(token))
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return http;
    }

    public async Task<List<PrinterDto>> GetAllAsync()
    {
        var http = await ClientAsync();
        return await http.GetFromJsonAsync<List<PrinterDto>>("api/printers") ?? new List<PrinterDto>();
    }

    public async Task<PrinterDto?> GetByIdAsync(Guid id)
    {
        var http = await ClientAsync();
        return await http.GetFromJsonAsync<PrinterDto>($"api/printers/{id}");
    }

    public async Task<PrinterDto> CreateAsync(PrinterCreateDto printer)
    {
        var http = await ClientAsync();
        var response = await http.PostAsJsonAsync("api/printers", printer);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PrinterDto>() ?? 
               throw new Exception("No se pudo crear la impresora");
    }

    public async Task UpdateAsync(PrinterUpdateDto printer)
    {
        var http = await ClientAsync();
        var response = await http.PutAsJsonAsync($"api/printers/{printer.Id}", printer);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id)
    {
        var http = await ClientAsync();
        var response = await http.DeleteAsync($"api/printers/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<PrinterStatusDto?> GetPrinterStatusAsync(Guid printerId)
    {
        var http = await ClientAsync();
        return await http.GetFromJsonAsync<PrinterStatusDto>($"api/printers/{printerId}/status");
    }

    public async Task<IEnumerable<PrinterStatusDto>> GetAllStatusesAsync()
    {
        var http = await ClientAsync();
        return await http.GetFromJsonAsync<List<PrinterStatusDto>>("api/printers/status") ?? 
               new List<PrinterStatusDto>();
    }

    public async Task<PrinterStatsDto> GetStatsAsync()
    {
        var http = await ClientAsync();
        return await http.GetFromJsonAsync<PrinterStatsDto>("api/printers/stats") ?? 
               new PrinterStatsDto();
    }

    public async Task<List<PrinterDto>> SearchAsync(string searchTerm)
    {
        var http = await ClientAsync();
        return await http.GetFromJsonAsync<List<PrinterDto>>($"api/printers/search?query={Uri.EscapeDataString(searchTerm)}") ?? 
               new List<PrinterDto>();
    }

}
