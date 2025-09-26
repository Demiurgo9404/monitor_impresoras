namespace MonitorImpresoras.Domain.Enums
{
    public enum ReportType
    {
        // Reportes de uso
        UsageByUser,            // Uso por usuario
        UsageByDepartment,      // Uso por departamento
        UsageByPrinter,         // Uso por impresora
        UsageTrend,             // Tendencias de uso
        
        // Reportes de costos
        CostByUser,             // Costos por usuario
        CostByDepartment,       // Costos por departamento
        CostByPrinter,          // Costos por impresora
        CostTrend,              // Tendencias de costos
        
        // Reportes de mantenimiento
        Maintenance,            // Historial de mantenimiento
        ConsumableUsage,        // Uso de consumibles
        AlertHistory,           // Historial de alertas
        
        // Reportes de rendimiento
        PrinterUtilization,     // Tasa de utilización
        Downtime,               // Tiempo de inactividad
        
        // Reportes de inventario
        InventoryStatus,        // Estado del inventario
        SupplyOrders,           // Órdenes de suministro
        
        // Reportes personalizados
        Custom                  // Informe personalizado
    }
    
    public enum ReportFormat
    {
        PDF,
        Excel,
        CSV,
        HTML
    }
    
    public enum ReportStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Expired
    }
}
