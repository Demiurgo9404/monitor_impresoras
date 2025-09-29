using Microsoft.Extensions.Diagnostics.HealthChecks;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para health checks de la aplicación
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ApplicationDbContext _context;
        private readonly IScheduledReportService _scheduledReportService;
        private readonly IMetricsService _metricsService;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(
            ApplicationDbContext context,
            IScheduledReportService scheduledReportService,
            IMetricsService metricsService,
            ILogger<HealthCheckService> logger)
        {
            _context = context;
            _scheduledReportService = scheduledReportService;
            _metricsService = metricsService;
            _logger = logger;
        }

        public async Task<HealthCheckDto> GetBasicHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var health = new HealthCheckDto();
            var checks = new Dictionary<string, object>();

            try
            {
                // Health check básico de la aplicación
                checks["Application"] = new
                {
                    Status = "Healthy",
                    Version = typeof(HealthCheckService).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
                };

                // Health check de memoria
                var memoryInfo = GC.GetGCMemoryInfo();
                checks["Memory"] = new
                {
                    Status = "Healthy",
                    HeapSizeMB = memoryInfo.HeapSizeBytes / 1024 / 1024,
                    TotalAllocatedMB = GC.GetTotalAllocatedBytes() / 1024 / 1024,
                    Generation0Collections = GC.CollectionCount(0),
                    Generation1Collections = GC.CollectionCount(1),
                    Generation2Collections = GC.CollectionCount(2)
                };

                health.Status = "Healthy";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en health check básico");
                health.Status = "Unhealthy";
                checks["Application"] = new { Status = "Unhealthy", Error = ex.Message };
            }

            health.Checks = checks;
            health.TotalDuration = stopwatch.Elapsed;

            return health;
        }

        public async Task<ExtendedHealthCheckDto> GetExtendedHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var basicHealth = await GetBasicHealthAsync();

            var extendedHealth = new ExtendedHealthCheckDto
            {
                Status = basicHealth.Status,
                Checks = basicHealth.Checks,
                TotalDuration = basicHealth.TotalDuration
            };

            // Database health check
            extendedHealth.Database = await CheckDatabaseHealthAsync();

            // Scheduled reports health check
            extendedHealth.ScheduledReports = await CheckScheduledReportsHealthAsync();

            // System health check
            extendedHealth.System = await CheckSystemHealthAsync();

            // Metrics health check
            extendedHealth.Metrics = await CheckMetricsHealthAsync();

            return extendedHealth;
        }

        private async Task<DatabaseHealthDto> CheckDatabaseHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return new DatabaseHealthDto
                    {
                        Status = "Unhealthy",
                        QueryTime = stopwatch.Elapsed,
                        ConnectionString = "Connection failed"
                    };
                }

                // Get table record counts
                var recordCounts = new Dictionary<string, long>();
                recordCounts["Users"] = await _context.Users.CountAsync();
                recordCounts["ReportTemplates"] = await _context.ReportTemplates.CountAsync();
                recordCounts["ReportExecutions"] = await _context.ReportExecutions.CountAsync();
                recordCounts["ScheduledReports"] = await _context.ScheduledReports.CountAsync();
                recordCounts["SystemEvents"] = await _context.SystemEvents.CountAsync();

                return new DatabaseHealthDto
                {
                    Status = "Healthy",
                    ConnectionCount = 1, // We don't have a way to get active connections easily
                    TableRecordCounts = recordCounts,
                    QueryTime = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en database health check");
                return new DatabaseHealthDto
                {
                    Status = "Unhealthy",
                    QueryTime = stopwatch.Elapsed
                };
            }
        }

        private async Task<ScheduledReportsHealthDto> CheckScheduledReportsHealthAsync()
        {
            try
            {
                var activeReports = await _context.ScheduledReports.CountAsync(sr => sr.IsActive);
                var pendingExecutions = await _context.ReportExecutions.CountAsync(re => re.Status == "pending");
                var failedLast24h = await _context.ReportExecutions
                    .Where(re => re.Status == "failed" && re.StartedAtUtc > DateTime.UtcNow.AddHours(-24))
                    .CountAsync();
                var lastExecution = await _context.ReportExecutions
                    .OrderByDescending(re => re.StartedAtUtc)
                    .Select(re => (DateTime?)re.StartedAtUtc)
                    .FirstOrDefaultAsync();

                return new ScheduledReportsHealthDto
                {
                    Status = "Healthy",
                    ActiveScheduledReports = activeReports,
                    PendingExecutions = pendingExecutions,
                    FailedExecutionsLast24h = failedLast24h,
                    LastExecutionTime = lastExecution
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en scheduled reports health check");
                return new ScheduledReportsHealthDto { Status = "Unhealthy" };
            }
        }

        private async Task<SystemHealthDto> CheckSystemHealthAsync()
        {
            try
            {
                var process = Process.GetCurrentProcess();

                // Get CPU usage (this is approximate)
                var cpuUsage = await GetCpuUsageAsync();

                // Get memory usage
                var memoryUsage = process.WorkingSet64 / (double)GetTotalMemoryBytes();

                // Get disk usage
                var diskUsage = GetDiskUsage();

                return new SystemHealthDto
                {
                    Status = "Healthy",
                    CpuUsage = cpuUsage,
                    MemoryUsage = memoryUsage,
                    DiskUsage = diskUsage,
                    ActiveConnections = 0, // We don't track this easily
                    ApplicationVersion = typeof(HealthCheckService).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en system health check");
                return new SystemHealthDto { Status = "Unhealthy" };
            }
        }

        private async Task<MetricsHealthDto> CheckMetricsHealthAsync()
        {
            try
            {
                // For now, just return healthy since we don't have complex metrics logic
                return new MetricsHealthDto { Status = "Healthy" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en metrics health check");
                return new MetricsHealthDto { Status = "Unhealthy" };
            }
        }

        private async Task<double> GetCpuUsageAsync()
        {
            // This is a simplified CPU usage calculation
            // In a real application, you might want to use PerformanceCounter or similar
            var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;

            await Task.Delay(1000); // Wait 1 second

            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return Math.Min(cpuUsageTotal * 100, 100); // Cap at 100%
        }

        private static long GetTotalMemoryBytes()
        {
            try
            {
                // Try to get total physical memory
                var memoryStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memoryStatus))
                {
                    return memoryStatus.ullTotalPhys;
                }
            }
            catch
            {
                // Fallback to process memory limit
            }

            return 8L * 1024 * 1024 * 1024; // 8GB fallback
        }

        private static double GetDiskUsage()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory)!);
                return (double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize * 100;
            }
            catch
            {
                return 0; // Can't determine disk usage
            }
        }

        // P/Invoke for memory status
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, uint size);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_MEMORY_COUNTERS
        {
            public uint cb;
            public uint PageFaultCount;
            public IntPtr PeakWorkingSetSize;
            public IntPtr WorkingSetSize;
            public IntPtr QuotaPeakPagedPoolUsage;
            public IntPtr QuotaPagedPoolUsage;
            public IntPtr QuotaPeakNonPagedPoolUsage;
            public IntPtr QuotaNonPagedPoolUsage;
            public IntPtr PagefileUsage;
            public IntPtr PeakPagefileUsage;
        }
    }
}
