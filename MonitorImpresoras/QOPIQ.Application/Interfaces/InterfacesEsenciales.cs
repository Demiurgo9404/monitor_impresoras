using QOPIQ.Domain.Entities;
using QOPIQ.Application.DTOs;
using System.Security.Claims;
using QOPIQ.Application.Interfaces.MultiTenancy;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Application.Interfaces
{
    // Servicio de suscripciones
    public interface ISubscriptionService
    {
        /// <summary>
        /// Obtiene la suscripción activa de un usuario
        /// </summary>
        Task<Subscription?> GetActiveSubscriptionAsync(Guid userId);

        /// <summary>
        /// Crea una nueva suscripción para un usuario
        /// </summary>
        Task<Subscription> CreateSubscriptionAsync(Guid userId, SubscriptionPlan plan);

        /// <summary>
        /// Crea una factura para una suscripción
        /// </summary>
        Task<Invoice> CreateInvoiceAsync(Guid subscriptionId, decimal amount);

        /// <summary>
        /// Cancela una suscripción existente
        /// </summary>
        Task CancelSubscriptionAsync(Guid subscriptionId, string? reason = null);

        /// <summary>
        /// Marca una factura como pagada
        /// </summary>
        Task MarkInvoiceAsPaidAsync(Guid invoiceId);

        /// <summary>
        /// Obtiene el historial de facturas de una suscripción
        /// </summary>
        Task<IEnumerable<Invoice>> GetInvoicesForSubscriptionAsync(Guid subscriptionId);
    }

    // Servicio de impresoras Windows
    public interface IWindowsPrinterService
    {
        Task<List<Printer>> GetLocalPrintersAsync();
        Task<bool> IsPrinterOnlineAsync(string printerName);
        Task<PrinterStatusInfo> GetPrinterStatusAsync(string printerName);
        Task<PrinterCounters> GetPrinterCountersAsync(string printerName);
    }

    // Servicio SNMP para impresoras de red
    public interface ISnmpService
    {
        Task<bool> TestConnectionAsync(string ipAddress, string community = "public");
        Task<Dictionary<string, object>> GetPrinterInfoAsync(string ipAddress, string community = "public");
        Task<PrinterCounters> GetPrinterCountersAsync(string ipAddress, string community = "public");
    }

    // Repositorio de impresoras
    public interface IPrinterRepository
    {
        Task<List<Printer>> GetAllAsync();
        Task<Printer?> GetByIdAsync(Guid id);
        Task<Printer> AddAsync(Printer printer);
        Task UpdateAsync(Printer printer);
        Task DeleteAsync(Guid id);
    }

    // Acceso al tenant actual se encuentra en QOPIQ.Application.Interfaces.MultiTenancy.ITenantAccessor

    // Servicio de correo electrónico
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendTemplateEmailAsync(string to, string templateName, object model);
    }

    // Servicio de gestión de tenants
    public interface ITenantService
    {
        Task<Tenant> GetTenantByIdAsync(string tenantId);
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
        Task<bool> CreateTenantAsync(Tenant tenant);
        Task<bool> UpdateTenantAsync(Tenant tenant);
        Task<bool> DeleteTenantAsync(string tenantId);
    }

    // Servicio de gestión de compañías
    public interface ICompanyService
    {
        Task<List<Company>> GetAllAsync(string tenantId);
        Task<Company?> GetByIdAsync(Guid id, string tenantId);
        Task<Company> CreateAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Guid id, string tenantId);
    }

    // Servicio de gestión de proyectos
    public interface IProjectService
    {
        Task<List<Project>> GetAllAsync(string tenantId);
        Task<Project?> GetByIdAsync(Guid id, string tenantId);
        Task<Project> CreateAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Guid id, string tenantId);
    }

    // Servicio de generación de reportes
    public interface IReportService
    {
        Task<byte[]> GenerateReportAsync(string tenantId, string reportType, DateTime startDate, DateTime endDate);
        Task<List<Report>> GetReportsAsync(string tenantId);
        Task<Report?> GetReportByIdAsync(Guid id, string tenantId);
    }

    // Servicio de reportes programados
    public interface IScheduledReportService
    {
        Task<List<ScheduledReport>> GetAllAsync(string tenantId);
        Task<ScheduledReport?> GetByIdAsync(Guid id, string tenantId);
        Task<ScheduledReport> CreateAsync(ScheduledReport scheduledReport);
        Task UpdateAsync(ScheduledReport scheduledReport);
        Task DeleteAsync(Guid id, string tenantId);
        Task ProcessScheduledReportsAsync();
    }

    // Servicio de datos para reportes
    public interface IReportDataService
    {
        Task<ReportDataDto> GetReportDataAsync(string tenantId, DateTime startDate, DateTime endDate);
    }

    // Generador de reportes PDF
    public interface IPdfReportGenerator
    {
        byte[] GeneratePdfReport(ReportDataDto data);
    }

    // Generador de reportes Excel
    public interface IExcelReportGenerator
    {
        byte[] GenerateExcelReport(ReportDataDto data);
    }
}
