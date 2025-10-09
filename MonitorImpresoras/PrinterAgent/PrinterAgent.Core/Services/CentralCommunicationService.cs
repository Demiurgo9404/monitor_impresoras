using PrinterAgent.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PrinterAgent.Core.Services
{
    public class CentralCommunicationService : ICentralCommunicationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CentralCommunicationService> _logger;
        private readonly AgentConfiguration _config;
        private readonly JsonSerializerOptions _jsonOptions;

        public CentralCommunicationService(
            HttpClient httpClient,
            ILogger<CentralCommunicationService> logger,
            IOptions<AgentConfiguration> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_config.CentralApiUrl);
            _httpClient.DefaultRequestHeaders.Add("X-Agent-Id", _config.AgentId);
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", $"PrinterAgent/{_config.AgentId}");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> RegisterAgentAsync(AgentConfiguration config, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Registrando agente {AgentId} en el sistema central", config.AgentId);

                var registrationData = new
                {
                    agentId = config.AgentId,
                    agentName = config.AgentName,
                    location = config.Location,
                    version = "1.0.0",
                    capabilities = new
                    {
                        supportsSnmp = true,
                        supportsWmi = true,
                        maxConcurrentScans = config.Network.MaxConcurrentScans
                    },
                    networkRanges = config.Network.ScanRanges
                };

                var response = await _httpClient.PostAsJsonAsync("/agents/register", registrationData, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Agente registrado exitosamente");
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Error registrando agente: {StatusCode} - {Error}", response.StatusCode, error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción registrando agente");
                return false;
            }
        }

        public async Task<bool> SendReportAsync(AgentReport report, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Enviando reporte con {PrinterCount} impresoras", report.Printers.Count);

                var response = await _httpClient.PostAsJsonAsync("/agents/reports", report, _jsonOptions, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Reporte enviado exitosamente");
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Error enviando reporte: {StatusCode} - {Error}", response.StatusCode, error);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error de red enviando reporte");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando reporte");
                return false;
            }
        }

        public async Task<bool> SendAlertAsync(PrinterAlert alert, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Enviando alerta {Severity}: {Message}", alert.Severity, alert.Message);

                var alertData = new
                {
                    agentId = _config.AgentId,
                    alert = alert
                };

                var response = await _httpClient.PostAsJsonAsync("/agents/alerts", alertData, _jsonOptions, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Alerta enviada exitosamente");
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Error enviando alerta: {StatusCode} - {Error}", response.StatusCode, error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando alerta");
                return false;
            }
        }

        public async Task<List<AgentCommand>> GetPendingCommandsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/agents/{_config.AgentId}/commands", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var commands = await response.Content.ReadFromJsonAsync<List<AgentCommand>>(_jsonOptions, cancellationToken);
                    _logger.LogDebug("Obtenidos {CommandCount} comandos pendientes", commands?.Count ?? 0);
                    return commands ?? new List<AgentCommand>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<AgentCommand>();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Error obteniendo comandos: {StatusCode} - {Error}", response.StatusCode, error);
                    return new List<AgentCommand>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Excepción obteniendo comandos pendientes");
                return new List<AgentCommand>();
            }
        }

        public async Task<bool> SendCommandResponseAsync(AgentCommandResponse response, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Enviando respuesta para comando {CommandId}: {Success}", response.CommandId, response.Success);

                var httpResponse = await _httpClient.PostAsJsonAsync("/agents/commands/responses", response, _jsonOptions, cancellationToken);
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Respuesta de comando enviada exitosamente");
                    return true;
                }
                else
                {
                    var error = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Error enviando respuesta de comando: {StatusCode} - {Error}", httpResponse.StatusCode, error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando respuesta de comando");
                return false;
            }
        }

        public async Task<AgentConfiguration?> GetUpdatedConfigurationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/agents/{_config.AgentId}/configuration", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var config = await response.Content.ReadFromJsonAsync<AgentConfiguration>(_jsonOptions, cancellationToken);
                    _logger.LogDebug("Configuración actualizada obtenida");
                    return config;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                {
                    _logger.LogDebug("Configuración no ha cambiado");
                    return null;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Error obteniendo configuración: {StatusCode} - {Error}", response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Excepción obteniendo configuración actualizada");
                return null;
            }
        }

        public async Task<bool> SendHeartbeatAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var heartbeat = new
                {
                    agentId = _config.AgentId,
                    timestamp = DateTime.UtcNow,
                    status = "healthy"
                };

                var response = await _httpClient.PostAsJsonAsync("/agents/heartbeat", heartbeat, _jsonOptions, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogTrace("Heartbeat enviado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogDebug("Error enviando heartbeat: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Excepción enviando heartbeat");
                return false;
            }
        }

        public async Task<bool> TestConnectivityAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/health", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}

