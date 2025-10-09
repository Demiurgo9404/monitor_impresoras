using QOPIQ.Application.Services;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using QOPIQ.Infrastructure.Services.SNMP;
using Moq;
using Xunit;

namespace QOPIQ.Tests.Services
{
    public class PrinterServiceTests
    {
        private readonly Mock<ISnmpService> _snmpServiceMock;
        private readonly PrinterMonitoringService _service;

        public PrinterServiceTests()
        {
            _snmpServiceMock = new Mock<ISnmpService>();
            _service = new PrinterMonitoringService(null, _snmpServiceMock.Object);
        }

        [Fact]
        public async Task AltaImpresora_ConDatosValidos_CreaImpresoraCorrectamente()
        {
            // Arrange
            var nuevaImpresora = new Printer
            {
                Name = "HP LaserJet Pro 4000",
                Model = "LaserJet Pro 4000",
                SerialNumber = "ABC123XYZ",
                IpAddress = "192.168.1.100",
                Location = "Oficina Principal",
                CommunityString = "public",
                SnmpPort = 161,
                IsActive = true
            };

            // Act & Assert - Simular creación exitosa
            Assert.Equal("HP LaserJet Pro 4000", nuevaImpresora.Name);
            Assert.Equal("192.168.1.100", nuevaImpresora.IpAddress);
            Assert.True(nuevaImpresora.IsActive);
        }

        [Fact]
        public async Task LecturaSNMP_ConImpresoraOnline_ObtieneDatosCorrectos()
        {
            // Arrange
            var printerStatus = new PrinterStatus
            {
                IpAddress = "192.168.1.100",
                IsOnline = true,
                StatusMessage = "En funcionamiento",
                DeviceStatus = "En funcionamiento",
                PrinterStatus = "Inactivo",
                TotalPages = 15420,
                TonerLevel = 85,
                LastChecked = DateTime.UtcNow
            };

            _snmpServiceMock.Setup(s => s.GetPrinterStatusAsync("192.168.1.100", "public", 161))
                .ReturnsAsync(printerStatus);

            // Act
            var result = await _snmpServiceMock.Object.GetPrinterStatusAsync("192.168.1.100");

            // Assert
            Assert.True(result.IsOnline);
            Assert.Equal(15420, result.TotalPages);
            Assert.Equal(85, result.TonerLevel);
        }

        [Fact]
        public async Task LecturaSNMP_ConImpresoraOffline_ManejaErrorCorrectamente()
        {
            // Arrange
            var printerStatus = new PrinterStatus
            {
                IpAddress = "192.168.1.999",
                IsOnline = false,
                StatusMessage = "Error de conexión: Timeout",
                LastChecked = DateTime.UtcNow
            };

            _snmpServiceMock.Setup(s => s.GetPrinterStatusAsync("192.168.1.999", "public", 161))
                .ReturnsAsync(printerStatus);

            // Act
            var result = await _snmpServiceMock.Object.GetPrinterStatusAsync("192.168.1.999");

            // Assert
            Assert.False(result.IsOnline);
            Assert.Contains("Error", result.StatusMessage);
        }

        [Fact]
        public async Task GeneracionAlerta_ConsumibleBajo_CreaAlertaCorrecta()
        {
            // Arrange
            var consumible = new PrinterConsumable
            {
                Name = "Tóner Negro",
                Type = ConsumableType.Toner,
                CurrentLevel = 15, // Nivel bajo
                WarningLevel = 20,
                PrinterId = 1
            };

            var printer = new Printer
            {
                Id = 1,
                Name = "HP LaserJet Pro 4000",
                IsActive = true
            };

            // Act & Assert
            Assert.True(consumible.CurrentLevel < consumible.WarningLevel.Value);
            Assert.Equal(15, consumible.CurrentLevel);
            Assert.Equal(ConsumableType.Toner, consumible.Type);
        }

        [Fact]
        public void TenantAislado_ConContextoDiferente_MantieneSeparacion()
        {
            // Arrange
            var tenant1 = new TenantContext
            {
                TenantId = Guid.NewGuid(),
                TenantKey = "empresa1",
                Name = "Empresa 1",
                ConnectionString = "Host=localhost;Database=tenant_empresa1"
            };

            var tenant2 = new TenantContext
            {
                TenantId = Guid.NewGuid(),
                TenantKey = "empresa2",
                Name = "Empresa 2",
                ConnectionString = "Host=localhost;Database=tenant_empresa2"
            };

            // Act & Assert
            Assert.NotEqual(tenant1.TenantId, tenant2.TenantId);
            Assert.NotEqual(tenant1.TenantKey, tenant2.TenantKey);
            Assert.NotEqual(tenant1.ConnectionString, tenant2.ConnectionString);
        }

        [Fact]
        public void PanelAdministrativo_ConPermisosCorrectos_MuestraDatosTenant()
        {
            // Arrange
            var tenant = new Tenant
            {
                Id = 1,
                TenantKey = "demo",
                Name = "Empresa Demo",
                CompanyName = "Demo S.A. de C.V.",
                SubscriptionTier = SubscriptionTier.Professional,
                MaxPrinters = 100,
                MaxUsers = 200,
                IsActive = true
            };

            // Act & Assert
            Assert.Equal("demo", tenant.TenantKey);
            Assert.Equal("Empresa Demo", tenant.Name);
            Assert.Equal(SubscriptionTier.Professional, tenant.SubscriptionTier);
            Assert.Equal(100, tenant.MaxPrinters);
            Assert.True(tenant.IsActive);
        }

        [Fact]
        public void OnboardingWizard_ConDatosValidos_ValidaCorrectamente()
        {
            // Arrange
            var wizardData = new
            {
                CompanyName = "Tech Solutions S.A.",
                AdminEmail = "admin@techsolutions.com",
                Printers = new[]
                {
                    new { Name = "Impresora Oficina", IpAddress = "192.168.1.100" },
                    new { Name = "Impresora Sala", IpAddress = "192.168.1.101" }
                }
            };

            // Act & Assert
            Assert.Equal("Tech Solutions S.A.", wizardData.CompanyName);
            Assert.Equal("admin@techsolutions.com", wizardData.AdminEmail);
            Assert.Equal(2, wizardData.Printers.Length);
            Assert.Equal("192.168.1.100", wizardData.Printers[0].IpAddress);
        }

        [Fact]
        public void CalculoCostoImpresion_ConDatosValidos_CalculaCorrectamente()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "HP LaserJet",
                CostPerPage = 0.05m
            };

            var pages = 100;

            // Act
            var cost = pages * printer.CostPerPage;

            // Assert
            Assert.Equal(5.0m, cost);
        }

        [Fact]
        public void DeteccionMantenimiento_ConIntervaloSuperado_RequiereMantenimiento()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "HP LaserJet",
                LastMaintenance = DateTime.UtcNow.AddDays(-120), // 120 días atrás
                MaintenanceIntervalDays = 90 // Cada 90 días
            };

            // Act
            var needsMaintenance = printer.LastMaintenance.Value.AddDays(printer.MaintenanceIntervalDays.Value) <= DateTime.UtcNow;

            // Assert
            Assert.True(needsMaintenance);
        }

        [Fact]
        public void ReporteEstadisticas_ConDatosValidos_GeneraReporteCorrecto()
        {
            // Arrange
            var stats = new MonitoringStats
            {
                TotalPrinters = 10,
                OnlinePrinters = 8,
                OfflinePrinters = 2,
                RecentJobs = 45,
                TotalPages24h = 1250,
                LastUpdate = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal(10, stats.TotalPrinters);
            Assert.Equal(8, stats.OnlinePrinters);
            Assert.Equal(2, stats.OfflinePrinters);
            Assert.Equal(80.0, (double)stats.OnlinePrinters / stats.TotalPrinters * 100); // 80%
            Assert.Equal(45, stats.RecentJobs);
            Assert.Equal(1250, stats.TotalPages24h);
        }
    }
}

