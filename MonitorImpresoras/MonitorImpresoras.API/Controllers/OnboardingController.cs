using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    /// <summary>
    /// Controlador para wizard de onboarding
    /// </summary>
    [ApiController]
    [Route("api/onboarding")]
    public class OnboardingController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public OnboardingController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Inicia el proceso de onboarding
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartOnboarding([FromBody] StartOnboardingRequest request)
        {
            try
            {
                // Crear tenant básico
                var createTenantRequest = new CreateTenantRequest
                {
                    TenantKey = GenerateTenantKey(request.CompanyName),
                    Name = request.CompanyName,
                    CompanyName = request.CompanyName,
                    AdminEmail = request.AdminEmail,
                    SubscriptionTier = SubscriptionTier.Free,
                    Timezone = request.Timezone ?? "America/Mexico_City",
                    Currency = request.Currency ?? "MXN"
                };

                var tenant = await _tenantService.CreateTenantAsync(createTenantRequest);

                var response = new
                {
                    TenantId = tenant.Id,
                    TenantKey = tenant.TenantKey,
                    Message = "Onboarding iniciado correctamente",
                    NextStep = "company-info",
                    Steps = new[]
                    {
                        new { Id = "company-info", Name = "Información de la empresa", Completed = true },
                        new { Id = "printers", Name = "Registrar impresoras", Completed = false },
                        new { Id = "alerts", Name = "Configurar alertas", Completed = false },
                        new { Id = "users", Name = "Invitar usuarios", Completed = false },
                        new { Id = "complete", Name = "Finalizar", Completed = false }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Registra la primera impresora
        /// </summary>
        [HttpPost("printers")]
        public async Task<IActionResult> RegisterFirstPrinter([FromBody] RegisterPrinterRequest request)
        {
            try
            {
                // Validar que el tenant existe
                var tenant = await _tenantService.GetTenantByIdAsync(request.TenantId);

                // Crear impresora de ejemplo (en producción esto vendría de un servicio)
                var printer = new Printer
                {
                    Name = request.PrinterName,
                    Model = "Auto-detectado",
                    SerialNumber = "PENDING",
                    IpAddress = request.IpAddress,
                    Location = request.Location ?? "Oficina Principal",
                    CommunityString = request.CommunityString ?? "public",
                    SnmpPort = request.SnmpPort ?? 161,
                    IsActive = true,
                    IsLocalPrinter = false
                };

                var response = new
                {
                    Printer = printer,
                    Message = "Impresora registrada correctamente",
                    NextStep = "alerts",
                    Steps = new[]
                    {
                        new { Id = "company-info", Name = "Información de la empresa", Completed = true },
                        new { Id = "printers", Name = "Registrar impresoras", Completed = true },
                        new { Id = "alerts", Name = "Configurar alertas", Completed = false },
                        new { Id = "users", Name = "Invitar usuarios", Completed = false },
                        new { Id = "complete", Name = "Finalizar", Completed = false }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Configura reglas de alerta básicas
        /// </summary>
        [HttpPost("alerts")]
        public async Task<IActionResult> ConfigureBasicAlerts([FromBody] ConfigureAlertsRequest request)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdAsync(request.TenantId);

                // Crear reglas de alerta básicas (simuladas)
                var basicRules = new List<object>
                {
                    new
                    {
                        Id = 1,
                        Name = "Tóner bajo",
                        Description = "Alerta cuando el tóner esté por debajo del 20%",
                        Type = "ThresholdExceeded",
                        FieldName = "CurrentLevel",
                        Operator = "LessThan",
                        ThresholdValue = 20.0,
                        AlertType = "Warning",
                        Severity = "Medium",
                        IsActive = true
                    },
                    new
                    {
                        Id = 2,
                        Name = "Impresora fuera de línea",
                        Description = "Alerta cuando una impresora no responda por más de 1 hora",
                        Type = "ThresholdExceeded",
                        FieldName = "OfflineTime",
                        Operator = "GreaterThan",
                        ThresholdValue = 1.0,
                        AlertType = "Error",
                        Severity = "High",
                        IsActive = true
                    },
                    new
                    {
                        Id = 3,
                        Name = "Mantenimiento requerido",
                        Description = "Alerta cuando sea tiempo de mantenimiento",
                        Type = "MaintenanceRequired",
                        AlertType = "Information",
                        Severity = "Low",
                        IsActive = true
                    }
                };

                var response = new
                {
                    Rules = basicRules,
                    Message = "Alertas básicas configuradas",
                    NextStep = "users",
                    Steps = new[]
                    {
                        new { Id = "company-info", Name = "Información de la empresa", Completed = true },
                        new { Id = "printers", Name = "Registrar impresoras", Completed = true },
                        new { Id = "alerts", Name = "Configurar alertas", Completed = true },
                        new { Id = "users", Name = "Invitar usuarios", Completed = false },
                        new { Id = "complete", Name = "Finalizar", Completed = false }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Completa el onboarding
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingRequest request)
        {
            try
            {
                var tenant = await _tenantService.GetTenantByIdAsync(request.TenantId);

                var response = new
                {
                    Message = "¡Onboarding completado exitosamente!",
                    Tenant = new
                    {
                        Id = tenant.Id,
                        Name = tenant.Name,
                        TenantKey = tenant.TenantKey
                    },
                    NextActions = new[]
                    {
                        "Revisa tu email para confirmar la cuenta",
                        "Explora el dashboard para ver el estado de tus impresoras",
                        "Configura reglas de alerta personalizadas",
                        "Invita a más usuarios de tu equipo"
                    },
                    GettingStarted = new[]
                    {
                        new { Title = "Dashboard", Description = "Monitorea todas tus impresoras en tiempo real", Url = "/dashboard" },
                        new { Title = "Alertas", Description = "Configura notificaciones inteligentes", Url = "/alerts" },
                        new { Title = "Reportes", Description = "Genera reportes de uso y costos", Url = "/reports" },
                        new { Title = "Configuración", Description = "Personaliza tu experiencia", Url = "/settings" }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Detecta automáticamente impresoras en la red
        /// </summary>
        [HttpGet("discover-printers")]
        public async Task<IActionResult> DiscoverPrinters([FromQuery] string subnet = "192.168.1.0/24")
        {
            try
            {
                // Simular descubrimiento de impresoras
                var discoveredPrinters = new List<object>
                {
                    new
                    {
                        IpAddress = "192.168.1.100",
                        Name = "HP LaserJet Pro 4000",
                        Model = "LaserJet Pro 4000",
                        SerialNumber = "ABC123XYZ",
                        MacAddress = "00:11:22:33:44:55",
                        Status = "Online",
                        Location = "Oficina Principal"
                    },
                    new
                    {
                        IpAddress = "192.168.1.101",
                        Name = "Epson WorkForce WF-7720",
                        Model = "WorkForce Pro WF-7720",
                        SerialNumber = "DEF456ABC",
                        MacAddress = "AA:BB:CC:DD:EE:FF",
                        Status = "Online",
                        Location = "Sala de Juntas"
                    }
                };

                return Ok(new
                {
                    DiscoveredPrinters = discoveredPrinters,
                    TotalFound = discoveredPrinters.Count,
                    Subnet = subnet
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        private string GenerateTenantKey(string companyName)
        {
            var baseKey = companyName.ToLower()
                .Replace(" ", "-")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");

            // Agregar sufijo único si es necesario
            return $"{baseKey}-{DateTime.UtcNow.Ticks % 1000}";
        }
    }

    /// <summary>
    /// Solicitud para iniciar onboarding
    /// </summary>
    public class StartOnboardingRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string? Timezone { get; set; }
        public string? Currency { get; set; }
    }

    /// <summary>
    /// Solicitud para registrar impresora
    /// </summary>
    public class RegisterPrinterRequest
    {
        public int TenantId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? CommunityString { get; set; }
        public int? SnmpPort { get; set; }
    }

    /// <summary>
    /// Solicitud para configurar alertas
    /// </summary>
    public class ConfigureAlertsRequest
    {
        public int TenantId { get; set; }
    }

    /// <summary>
    /// Solicitud para completar onboarding
    /// </summary>
    public class CompleteOnboardingRequest
    {
        public int TenantId { get; set; }
    }
}
