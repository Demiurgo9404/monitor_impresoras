using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;
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

    public PrinterService(IHttpClientFactory factory,
        ProtectedSessionStorage storage,
        IOptions<FrontendOptions> opt)
    {
        _factory = factory;
        _storage = storage;
        _opt = opt.Value;
    }

    private async Task<HttpClient> ClientAsync()
    {
        var http = _factory.CreateClient("QOPIQ.API");

        if (_storage != null)
        {
            var result = await _storage.GetAsync<string>("authToken");
            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Value);
            }
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
               throw new InvalidOperationException("No se pudo crear la impresora");
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
               new PrinterStatsDto { TotalPrinters = 0, OnlinePrinters = 0, OfflinePrinters = 0 };
    }

    public async Task<List<PrinterDto>> SearchAsync(string searchTerm)
    {
        var http = await ClientAsync();
        return await http.GetFromJsonAsync<List<PrinterDto>>($"api/printers/search?query={Uri.EscapeDataString(searchTerm)}") ?? 
               new List<PrinterDto>();
    }

}
