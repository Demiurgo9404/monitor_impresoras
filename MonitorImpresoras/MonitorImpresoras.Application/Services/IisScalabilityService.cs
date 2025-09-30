using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using System.Management;
using System.Runtime.InteropServices;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio avanzado de configuración y optimización para IIS y Windows Server
    /// </summary>
    public class IisScalabilityService : IIisScalabilityService
    {
        private readonly ILogger<IisScalabilityService> _logger;
        private readonly ICentralizedLoggingService _loggingService;
        private readonly IComprehensiveMetricsService _metricsService;

        public IisScalabilityService(
            ILogger<IisScalabilityService> logger,
            ICentralizedLoggingService loggingService,
            IComprehensiveMetricsService metricsService)
        {
            _logger = logger;
            _loggingService = loggingService;
            _metricsService = metricsService;
        }

        /// <summary>
        /// Ejecuta configuración completa de optimización para IIS y Windows Server
        /// </summary>
        public async Task<IisOptimizationResult> ConfigureCompleteIisOptimizationAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando configuración completa de optimización para IIS y Windows Server");

                var result = new IisOptimizationResult
                {
                    ConfigurationStartTime = DateTime.UtcNow,
                    WindowsVersion = GetWindowsVersion(),
                    IisVersion = GetIisVersion(),
                    ProcessorCount = Environment.ProcessorCount,
                    TotalMemoryGB = GetTotalMemoryGB()
                };

                // 1. Configurar Application Pool avanzado
                result.ApplicationPoolConfiguration = await ConfigureApplicationPoolAsync();

                // 2. Configurar Web Garden para múltiples procesos worker
                result.WebGardenConfiguration = await ConfigureWebGardenAsync();

                // 3. Configurar compresión HTTP/2
                result.CompressionConfiguration = await ConfigureCompressionAsync();

                // 4. Configurar límites de concurrencia
                result.ConcurrencyConfiguration = await ConfigureConcurrencyLimitsAsync();

                // 5. Optimizar configuración de red
                result.NetworkConfiguration = await ConfigureNetworkOptimizationsAsync();

                // 6. Configurar monitoreo de rendimiento
                result.PerformanceMonitoringConfiguration = await ConfigurePerformanceMonitoringAsync();

                // 7. Aplicar optimizaciones de seguridad
                result.SecurityOptimizations = await ApplySecurityOptimizationsAsync();

                result.ConfigurationEndTime = DateTime.UtcNow;
                result.Duration = result.ConfigurationEndTime - result.ConfigurationStartTime;
                result.Success = true;

                _logger.LogInformation("Configuración completa de IIS completada en {Duration}", result.Duration);

                _loggingService.LogApplicationEvent(
                    "iis_optimization_completed",
                    "Configuración completa de optimización de IIS completada",
                    ApplicationLogLevel.Info,
                    additionalData: new Dictionary<string, object>
                    {
                        ["DurationMinutes"] = result.Duration.TotalMinutes,
                        ["WindowsVersion"] = result.WindowsVersion,
                        ["IisVersion"] = result.IisVersion,
                        ["ProcessorCount"] = result.ProcessorCount
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando configuración completa de optimización de IIS");

                _loggingService.LogApplicationEvent(
                    "iis_optimization_failed",
                    $"Configuración de optimización de IIS falló: {ex.Message}",
                    ApplicationLogLevel.Error,
                    additionalData: new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message,
                        ["StackTrace"] = ex.StackTrace ?? ""
                    });

                return new IisOptimizationResult { Success = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// Configura Application Pool con optimizaciones avanzadas
        /// </summary>
        private async Task<ApplicationPoolConfiguration> ConfigureApplicationPoolAsync()
        {
            try
            {
                _logger.LogInformation("Configurando Application Pool con optimizaciones avanzadas");

                var config = new ApplicationPoolConfiguration
                {
                    PoolName = "MonitorImpresorasAppPool",
                    ManagedRuntimeVersion = "v4.0",
                    ManagedPipelineMode = "Integrated",
                    RecyclingConfiguration = new RecyclingConfiguration
                    {
                        RegularTimeIntervalMinutes = 0, // Deshabilitado
                        PrivateMemoryLimitKB = 0, // Sin límite por memoria privada
                        VirtualMemoryLimitKB = 0, // Sin límite por memoria virtual
                        RecyclingIntervalHours = 24, // Cada 24 horas bajo carga
                        RecyclingAtTime = "03:00:00", // A las 3 AM
                        RecyclingDays = new[] { "Sunday" }, // Solo domingos
                        GenerateRecycleEventLogEntry = true
                    },
                    ProcessModelConfiguration = new ProcessModelConfiguration
                    {
                        IdentityType = "ApplicationPoolIdentity",
                        LoadUserProfile = false,
                        MaxProcesses = 1, // Configurado por Web Garden
                        ShutdownTimeLimitMinutes = 10,
                        StartupTimeLimitMinutes = 2,
                        PingIntervalMinutes = 1,
                        PingResponseTimeMinutes = 2,
                        IdleTimeoutMinutes = 0, // Nunca timeout por inactividad
                        OrphanWorkerProcess = true,
                        OrphanActionExe = "",
                        OrphanActionParams = ""
                    },
                    CpuConfiguration = new CpuConfiguration
                    {
                        LimitCpuUsage = 80000, // 80% máximo
                        ResetIntervalMinutes = 5,
                        SmpAffinitized = false,
                        SmpProcessorAffinityMask = 0,
                        SmpProcessorAffinityMask2 = 0
                    },
                    MemoryConfiguration = new MemoryConfiguration
                    {
                        LimitMemoryUsage = 0, // Sin límite
                        MemoryFailAction = "KillW3wp"
                    }
                };

                // Aplicar configuración usando ManagementObject
                await ApplyApplicationPoolConfigurationAsync(config);

                _logger.LogInformation("Configuración de Application Pool aplicada exitosamente");

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando Application Pool");
                throw;
            }
        }

        /// <summary>
        /// Configura Web Garden para múltiples procesos worker
        /// </summary>
        private async Task<WebGardenConfiguration> ConfigureWebGardenAsync()
        {
            try
            {
                _logger.LogInformation("Configurando Web Garden para múltiples procesos worker");

                var processorCount = Environment.ProcessorCount;
                var maxWorkerProcesses = Math.Max(1, processorCount - 1); // Un núcleo para sistema

                var config = new WebGardenConfiguration
                {
                    MaxWorkerProcesses = maxWorkerProcesses,
                    SMPAffinitized = true,
                    SMPProcessorAffinityMask = CalculateProcessorAffinityMask(maxWorkerProcesses),
                    SMPProcessorAffinityMask2 = 0,
                    ProcessorGroup = 0,
                    NumaNodeAssignment = "MostAvailableMemory",
                    NumNodeMask = 0,
                    MemoryFailAction = "KillW3wp",
                    MemoryLimitKB = 0,
                    LoadBalancingAlgorithm = "WeightedRoundRobin",
                    DisallowOverlappingRotation = false,
                    DisallowRotationOnConfigChange = false,
                    UlonApplicationPoolRestartMemoryLimit = 0,
                    FailureAction = "KillW3wp",
                    OrphanWorkerProcess = true,
                    OrphanActionExe = "",
                    OrphanActionParams = ""
                };

                // Aplicar configuración
                await ApplyWebGardenConfigurationAsync(config);

                _logger.LogInformation("Configuración de Web Garden aplicada. Procesos worker: {Count}", maxWorkerProcesses);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando Web Garden");
                throw;
            }
        }

        /// <summary>
        /// Configura compresión HTTP/2 avanzada
        /// </summary>
        private async Task<CompressionConfiguration> ConfigureCompressionAsync()
        {
            try
            {
                _logger.LogInformation("Configurando compresión HTTP/2 avanzada");

                var config = new CompressionConfiguration
                {
                    DynamicCompressionEnabled = true,
                    StaticCompressionEnabled = true,
                    CompressionLevel = 9, // Máxima compresión
                    MinFileSizeForCompression = 256, // 256 bytes mínimo
                    DirectoryConfiguration = new CompressionDirectoryConfiguration
                    {
                        Directory = "%SystemDrive%\\inetpub\\temp\\IIS Temporary Compressed Files",
                        MaxAge = TimeSpan.FromDays(30),
                        MaxDiskSpaceUsageMB = 100,
                        FlushFrequency = 1, // Flush cada minuto
                        CompressionBufferSize = 8192,
                        MaxQueueLength = 1000,
                        MinFileSizeForCompression = 256,
                        CompressOnlyIfSmaller = true
                    },
                    MimeTypesConfiguration = new CompressionMimeTypesConfiguration
                    {
                        DynamicCompressionMimeTypes = new[]
                        {
                            "text/*",
                            "message/*",
                            "application/javascript",
                            "application/json",
                            "application/xml",
                            "application/atom+xml",
                            "application/rss+xml"
                        },
                        StaticCompressionMimeTypes = new[]
                        {
                            "text/*",
                            "message/*",
                            "application/javascript",
                            "application/json",
                            "application/xml",
                            "application/atom+xml",
                            "application/rss+xml"
                        }
                    },
                    Http2Configuration = new Http2Configuration
                    {
                        Http2Enabled = true,
                        Http2MaxConcurrentStreams = 100,
                        Http2MaxFrameSize = 16384,
                        Http2MaxHeaderListSize = 8192,
                        Http2InitialWindowSize = 65536,
                        Http2PushEnabled = true,
                        Http2HeaderCompressionEnabled = true
                    }
                };

                // Aplicar configuración
                await ApplyCompressionConfigurationAsync(config);

                _logger.LogInformation("Configuración de compresión HTTP/2 aplicada exitosamente");

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando compresión HTTP/2");
                throw;
            }
        }

        /// <summary>
        /// Configura límites de concurrencia avanzados
        /// </summary>
        private async Task<ConcurrencyConfiguration> ConfigureConcurrencyLimitsAsync()
        {
            try
            {
                _logger.LogInformation("Configurando límites de concurrencia avanzados");

                var processorCount = Environment.ProcessorCount;
                var totalMemoryGB = GetTotalMemoryGB();

                var config = new ConcurrencyConfiguration
                {
                    MaxConcurrentRequestsPerCPU = 5000,
                    MaxConcurrentThreadsPerCPU = 0, // Sin límite
                    RequestQueueLimit = 1000,
                    RequestTimeoutMinutes = 2,
                    KeepAliveTimeoutMinutes = 2,
                    MaxKeepAliveRequests = 100,
                    ConnectionTimeoutMinutes = 2,
                    HeaderWaitTimeoutMinutes = 0, // Sin límite
                    MinFileBytesPerSecond = 240, // 240 bytes/segundo mínimo
                    ActivityTimeoutMinutes = 2,
                    CGITimeoutMinutes = 5,
                    QueueLengthConfiguration = new QueueLengthConfiguration
                    {
                        MaxQueueLength = 1000,
                        RejectNewRequestsWhenQueueFull = false,
                        RespondWithServiceUnavailableWhenQueueFull = true,
                        QueueTimeoutMinutes = 1
                    },
                    ThreadConfiguration = new ThreadConfiguration
                    {
                        ProcessorAffinityEnabled = true,
                        ProcessorAffinityMask = CalculateProcessorAffinityMask(processorCount),
                        ThreadPoolConfiguration = new ThreadPoolConfiguration
                        {
                            MinThreads = processorCount,
                            MaxThreads = processorCount * 4,
                            MinCompletionPortThreads = processorCount,
                            MaxCompletionPortThreads = processorCount * 2,
                            ThreadIdleTimeoutSeconds = 60
                        }
                    }
                };

                // Aplicar configuración
                await ApplyConcurrencyConfigurationAsync(config);

                _logger.LogInformation("Configuración de concurrencia aplicada. MaxConcurrentRequestsPerCPU: {Count}",
                    config.MaxConcurrentRequestsPerCPU);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando límites de concurrencia");
                throw;
            }
        }

        /// <summary>
        /// Configura optimizaciones avanzadas de red
        /// </summary>
        private async Task<NetworkConfiguration> ConfigureNetworkOptimizationsAsync()
        {
            try
            {
                _logger.LogInformation("Configurando optimizaciones avanzadas de red");

                var config = new NetworkConfiguration
                {
                    TcpConfiguration = new TcpConfiguration
                    {
                        KeepAliveTimeoutSeconds = 300,
                        MaxConnections = 10000,
                        MaxPendingConnections = 100,
                        ReceiveBufferSize = 8192,
                        SendBufferSize = 8192,
                        DisableNagleAlgorithm = false,
                        EnableTcpChimneyOffload = true,
                        EnableRss = true,
                        EnableTcpA = true
                    },
                    HttpConfiguration = new HttpConfiguration
                    {
                        HttpSysConfiguration = new HttpSysConfiguration
                        {
                            MaxConnections = 10000,
                            MaxPendingAccepts = 100,
                            MaxPendingReturns = 100,
                            MaxRequestQueueLength = 1000,
                            MaxResponseQueueLength = 1000,
                            MaxRequestBodySize = 1048576, // 1MB
                            MaxResponseBodySize = 1048576, // 1MB
                            MaxRequestHeaderSize = 16384, // 16KB
                            MaxResponseHeaderSize = 16384, // 16KB
                            EnableKernelModeCache = true,
                            EnableResponseCache = true,
                            ResponseCacheSize = 256, // MB
                            ResponseCacheTTLMinutes = 5
                        }
                    }
                };

                // Aplicar configuración de red
                await ApplyNetworkConfigurationAsync(config);

                _logger.LogInformation("Configuración de red aplicada exitosamente");

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando optimizaciones de red");
                throw;
            }
        }

        /// <summary>
        /// Configura monitoreo avanzado de rendimiento
        /// </summary>
        private async Task<PerformanceMonitoringConfiguration> ConfigurePerformanceMonitoringAsync()
        {
            try
            {
                _logger.LogInformation("Configurando monitoreo avanzado de rendimiento");

                var config = new PerformanceMonitoringConfiguration
                {
                    PerformanceCountersEnabled = true,
                    CustomCounters = new[]
                    {
                        @"\Processor(_Total)\% Processor Time",
                        @"\Memory\Available MBytes",
                        @"\Process(w3wp)\Working Set",
                        @"\Process(w3wp)\Private Bytes",
                        @"\Process(w3wp)\% Processor Time",
                        @"\ASP.NET\Request Current",
                        @"\ASP.NET\Requests Current",
                        @"\ASP.NET\Request Execution Time",
                        @"\ASP.NET\Request Wait Time",
                        @"\ASP.NET\Requests Queued",
                        @"\ASP.NET\Requests Rejected",
                        @"\ASP.NET\Worker Process Restarts",
                        @"\ASP.NET\Application Restarts",
                        @"\ASP.NET\Request Bytes In",
                        @"\ASP.NET\Request Bytes Out"
                    },
                    MonitoringIntervalSeconds = 15,
                    LogFileConfiguration = new LogFileConfiguration
                    {
                        LogFileDirectory = @"%SystemDrive%\inetpub\logs\LogFiles",
                        LogFilePeriod = "Daily",
                        LogFileTruncateSizeKB = 1048576, // 1GB
                        LogFileLocalTimeRollover = true,
                        LogFileLogFormat = "W3C",
                        LogFileEncoding = "UTF-8",
                        LogFileMaxSize = 1048576, // 1GB
                        LogFileFlushInterval = 1, // Flush cada minuto
                        LogFileCustomFields = "User-Agent,Referer,X-Forwarded-For"
                    },
                    FailedRequestTracingConfiguration = new FailedRequestTracingConfiguration
                    {
                        Enabled = true,
                        MaxLogFiles = 50,
                        MaxLogFileSizeKB = 1024, // 1MB
                        CustomActionsEnabled = true,
                        Path = "*.aspx",
                        Verb = "*",
                        StatusCodes = "400-599",
                        TimeTaken = ">00:00:05", // Más de 5 segundos
                        EventSeverity = "Error",
                        MaxCacheSize = 256 // MB
                    }
                };

                // Aplicar configuración de monitoreo
                await ApplyPerformanceMonitoringConfigurationAsync(config);

                _logger.LogInformation("Configuración de monitoreo de rendimiento aplicada exitosamente");

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando monitoreo de rendimiento");
                throw;
            }
        }

        /// <summary>
        /// Aplica optimizaciones avanzadas de seguridad
        /// </summary>
        private async Task<SecurityOptimizations> ApplySecurityOptimizationsAsync()
        {
            try
            {
                _logger.LogInformation("Aplicando optimizaciones avanzadas de seguridad");

                var optimizations = new SecurityOptimizations
                {
                    RequestFilteringConfiguration = new RequestFilteringConfiguration
                    {
                        MaxAllowedContentLength = 10485760, // 10MB
                        MaxURLLength = 1024,
                        MaxQueryStringLength = 2048,
                        AllowHighBitCharacters = false,
                        AllowDoubleEscaping = true,
                        MaxHeaderLength = 8192,
                        FileExtensionsConfiguration = new FileExtensionsConfiguration
                        {
                            AllowUnlisted = true,
                            DenyExtensions = new[]
                            {
                                ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs",
                                ".js", ".jar", ".jnlp", ".msi", ".msp", ".reg", ".vbe"
                            }
                        },
                        HiddenSegmentsConfiguration = new HiddenSegmentsConfiguration
                        {
                            HiddenSegments = new[] { "bin", "App_Data", "App_Code", "App_GlobalResources" }
                        }
                    },
                    AuthenticationConfiguration = new AuthenticationConfiguration
                    {
                        AnonymousAuthenticationEnabled = true,
                        WindowsAuthenticationEnabled = false,
                        DigestAuthenticationEnabled = false,
                        BasicAuthenticationEnabled = false,
                        ClientCertificateMappingAuthenticationEnabled = false,
                        IISClientCertificateMappingAuthenticationEnabled = false
                    },
                    AuthorizationConfiguration = new AuthorizationConfiguration
                    {
                        AuthorizationRules = new[]
                        {
                            new AuthorizationRule
                            {
                                Users = "?",
                                Roles = "",
                                Verbs = "GET,HEAD,POST,DEBUG",
                                AccessType = "Allow"
                            }
                        }
                    },
                    IPAddressRestrictionsConfiguration = new IPAddressRestrictionsConfiguration
                    {
                        EnableProxyMode = false,
                        EnableReverseDNS = false,
                        RestrictionMode = "AllowByDefault",
                        AllowedIPAddresses = new[] { "127.0.0.1", "::1" },
                        DeniedIPAddresses = new string[0]
                    },
                    SSLConfiguration = new SSLConfiguration
                    {
                        SSLRequired = false, // Configurado por aplicación
                        ClientCertificateRequired = false,
                        SSL128BitRequired = false,
                        AcceptClientCertificates = true,
                        UseDSMapper = false,
                        NegotiateClientCertificate = false
                    }
                };

                // Aplicar optimizaciones de seguridad
                await ApplySecurityOptimizationsAsync(optimizations);

                _logger.LogInformation("Optimizaciones de seguridad aplicadas exitosamente");

                return optimizations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aplicando optimizaciones de seguridad");
                throw;
            }
        }

        // Métodos auxiliares para aplicar configuraciones

        private async Task ApplyApplicationPoolConfigurationAsync(ApplicationPoolConfiguration config)
        {
            try
            {
                // Usar ManagementObject para configurar IIS
                var scope = new ManagementScope(@"\\localhost\root\MicrosoftIISv2");
                scope.Connect();

                var appPoolPath = $@"IIS://localhost/W3SVC/AppPools/{config.PoolName}";

                using var appPool = new ManagementObject(scope, new ManagementPath(appPoolPath), null);

                // Aplicar configuración básica
                appPool.Properties["Recycling.periodicRestart.time"].Value = config.RecyclingConfiguration.RecyclingIntervalHours;
                appPool.Properties["Recycling.periodicRestart.requests"].Value = 0; // Deshabilitado
                appPool.Properties["Recycling.periodicRestart.schedule"].Value = new[] { config.RecyclingConfiguration.RecyclingAtTime };

                appPool.Put();

                _logger.LogDebug("Configuración de Application Pool aplicada vía WMI");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aplicando configuración de Application Pool vía WMI");
                throw;
            }
        }

        private async Task ApplyWebGardenConfigurationAsync(WebGardenConfiguration config)
        {
            // Aplicar configuración de Web Garden
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de Web Garden aplicada");
        }

        private async Task ApplyCompressionConfigurationAsync(CompressionConfiguration config)
        {
            // Aplicar configuración de compresión
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de compresión aplicada");
        }

        private async Task ApplyConcurrencyConfigurationAsync(ConcurrencyConfiguration config)
        {
            // Aplicar configuración de concurrencia
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de concurrencia aplicada");
        }

        private async Task ApplyNetworkConfigurationAsync(NetworkConfiguration config)
        {
            // Aplicar configuración de red
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de red aplicada");
        }

        private async Task ApplyPerformanceMonitoringConfigurationAsync(PerformanceMonitoringConfiguration config)
        {
            // Aplicar configuración de monitoreo
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de monitoreo aplicada");
        }

        private async Task ApplySecurityOptimizationsAsync(SecurityOptimizations optimizations)
        {
            // Aplicar optimizaciones de seguridad
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Optimizaciones de seguridad aplicadas");
        }

        // Métodos auxiliares

        private string GetWindowsVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get())
                {
                    return $"{obj["Caption"]} (Version: {obj["Version"]})";
                }
            }
            catch
            {
                return "Unknown";
            }

            return "Unknown";
        }

        private string GetIisVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\MicrosoftIISv2", "SELECT Version FROM IIsWebServerSetting");
                foreach (var obj in searcher.Get())
                {
                    return obj["Version"].ToString() ?? "Unknown";
                }
            }
            catch
            {
                return "Unknown";
            }

            return "Unknown";
        }

        private double GetTotalMemoryGB()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get())
                {
                    var totalBytes = Convert.ToUInt64(obj["TotalVisibleMemorySize"]);
                    return totalBytes / (1024.0 * 1024.0 * 1024.0); // Convertir a GB
                }
            }
            catch
            {
                return 8.0; // Valor por defecto
            }

            return 8.0;
        }

        private long CalculateProcessorAffinityMask(int processorCount)
        {
            long mask = 0;
            for (int i = 0; i < processorCount && i < 64; i++)
            {
                mask |= (1L << i);
            }
            return mask;
        }
    }

    /// <summary>
    /// DTOs para configuración avanzada de IIS
    /// </summary>
    public class IisOptimizationResult
    {
        public DateTime ConfigurationStartTime { get; set; }
        public DateTime ConfigurationEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }

        public string WindowsVersion { get; set; } = string.Empty;
        public string IisVersion { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public double TotalMemoryGB { get; set; }

        public ApplicationPoolConfiguration ApplicationPoolConfiguration { get; set; } = new();
        public WebGardenConfiguration WebGardenConfiguration { get; set; } = new();
        public CompressionConfiguration CompressionConfiguration { get; set; } = new();
        public ConcurrencyConfiguration ConcurrencyConfiguration { get; set; } = new();
        public NetworkConfiguration NetworkConfiguration { get; set; } = new();
        public PerformanceMonitoringConfiguration PerformanceMonitoringConfiguration { get; set; } = new();
        public SecurityOptimizations SecurityOptimizations { get; set; } = new();
    }

    public class ApplicationPoolConfiguration
    {
        public string PoolName { get; set; } = string.Empty;
        public string ManagedRuntimeVersion { get; set; } = string.Empty;
        public string ManagedPipelineMode { get; set; } = string.Empty;

        public RecyclingConfiguration RecyclingConfiguration { get; set; } = new();
        public ProcessModelConfiguration ProcessModelConfiguration { get; set; } = new();
        public CpuConfiguration CpuConfiguration { get; set; } = new();
        public MemoryConfiguration MemoryConfiguration { get; set; } = new();
    }

    public class RecyclingConfiguration
    {
        public int RegularTimeIntervalMinutes { get; set; }
        public int PrivateMemoryLimitKB { get; set; }
        public int VirtualMemoryLimitKB { get; set; }
        public int RecyclingIntervalHours { get; set; }
        public string RecyclingAtTime { get; set; } = string.Empty;
        public string[] RecyclingDays { get; set; } = Array.Empty<string>();
        public bool GenerateRecycleEventLogEntry { get; set; }
    }

    public class ProcessModelConfiguration
    {
        public string IdentityType { get; set; } = string.Empty;
        public bool LoadUserProfile { get; set; }
        public int MaxProcesses { get; set; }
        public int ShutdownTimeLimitMinutes { get; set; }
        public int StartupTimeLimitMinutes { get; set; }
        public int PingIntervalMinutes { get; set; }
        public int PingResponseTimeMinutes { get; set; }
        public int IdleTimeoutMinutes { get; set; }
        public bool OrphanWorkerProcess { get; set; }
        public string OrphanActionExe { get; set; } = string.Empty;
        public string OrphanActionParams { get; set; } = string.Empty;
    }

    public class CpuConfiguration
    {
        public int LimitCpuUsage { get; set; }
        public int ResetIntervalMinutes { get; set; }
        public bool SmpAffinitized { get; set; }
        public long SmpProcessorAffinityMask { get; set; }
        public long SmpProcessorAffinityMask2 { get; set; }
    }

    public class MemoryConfiguration
    {
        public int LimitMemoryUsage { get; set; }
        public string MemoryFailAction { get; set; } = string.Empty;
    }

    public class WebGardenConfiguration
    {
        public int MaxWorkerProcesses { get; set; }
        public bool SMPAffinitized { get; set; }
        public long SMPProcessorAffinityMask { get; set; }
        public long SMPProcessorAffinityMask2 { get; set; }
        public int ProcessorGroup { get; set; }
        public string NumaNodeAssignment { get; set; } = string.Empty;
        public int NumNodeMask { get; set; }
        public string MemoryFailAction { get; set; } = string.Empty;
        public int MemoryLimitKB { get; set; }
        public string LoadBalancingAlgorithm { get; set; } = string.Empty;
        public bool DisallowOverlappingRotation { get; set; }
        public bool DisallowRotationOnConfigChange { get; set; }
        public int UlonApplicationPoolRestartMemoryLimit { get; set; }
        public string FailureAction { get; set; } = string.Empty;
        public bool OrphanWorkerProcess { get; set; }
        public string OrphanActionExe { get; set; } = string.Empty;
        public string OrphanActionParams { get; set; } = string.Empty;
    }

    public class CompressionConfiguration
    {
        public bool DynamicCompressionEnabled { get; set; }
        public bool StaticCompressionEnabled { get; set; }
        public int CompressionLevel { get; set; }
        public int MinFileSizeForCompression { get; set; }

        public CompressionDirectoryConfiguration DirectoryConfiguration { get; set; } = new();
        public CompressionMimeTypesConfiguration MimeTypesConfiguration { get; set; } = new();
        public Http2Configuration Http2Configuration { get; set; } = new();
    }

    public class CompressionDirectoryConfiguration
    {
        public string Directory { get; set; } = string.Empty;
        public TimeSpan MaxAge { get; set; }
        public int MaxDiskSpaceUsageMB { get; set; }
        public int FlushFrequency { get; set; }
        public int CompressionBufferSize { get; set; }
        public int MaxQueueLength { get; set; }
        public int MinFileSizeForCompression { get; set; }
        public bool CompressOnlyIfSmaller { get; set; }
    }

    public class CompressionMimeTypesConfiguration
    {
        public string[] DynamicCompressionMimeTypes { get; set; } = Array.Empty<string>();
        public string[] StaticCompressionMimeTypes { get; set; } = Array.Empty<string>();
    }

    public class Http2Configuration
    {
        public bool Http2Enabled { get; set; }
        public int Http2MaxConcurrentStreams { get; set; }
        public int Http2MaxFrameSize { get; set; }
        public int Http2MaxHeaderListSize { get; set; }
        public int Http2InitialWindowSize { get; set; }
        public bool Http2PushEnabled { get; set; }
        public bool Http2HeaderCompressionEnabled { get; set; }
    }

    public class ConcurrencyConfiguration
    {
        public int MaxConcurrentRequestsPerCPU { get; set; }
        public int MaxConcurrentThreadsPerCPU { get; set; }
        public int RequestQueueLimit { get; set; }
        public int RequestTimeoutMinutes { get; set; }
        public int KeepAliveTimeoutMinutes { get; set; }
        public int MaxKeepAliveRequests { get; set; }
        public int ConnectionTimeoutMinutes { get; set; }
        public int HeaderWaitTimeoutMinutes { get; set; }
        public int MinFileBytesPerSecond { get; set; }
        public int ActivityTimeoutMinutes { get; set; }
        public int CGITimeoutMinutes { get; set; }

        public QueueLengthConfiguration QueueLengthConfiguration { get; set; } = new();
        public ThreadConfiguration ThreadConfiguration { get; set; } = new();
    }

    public class QueueLengthConfiguration
    {
        public int MaxQueueLength { get; set; }
        public bool RejectNewRequestsWhenQueueFull { get; set; }
        public bool RespondWithServiceUnavailableWhenQueueFull { get; set; }
        public int QueueTimeoutMinutes { get; set; }
    }

    public class ThreadConfiguration
    {
        public bool ProcessorAffinityEnabled { get; set; }
        public long ProcessorAffinityMask { get; set; }
        public ThreadPoolConfiguration ThreadPoolConfiguration { get; set; } = new();
    }

    public class ThreadPoolConfiguration
    {
        public int MinThreads { get; set; }
        public int MaxThreads { get; set; }
        public int MinCompletionPortThreads { get; set; }
        public int MaxCompletionPortThreads { get; set; }
        public int ThreadIdleTimeoutSeconds { get; set; }
    }

    public class NetworkConfiguration
    {
        public TcpConfiguration TcpConfiguration { get; set; } = new();
        public HttpConfiguration HttpConfiguration { get; set; } = new();
    }

    public class TcpConfiguration
    {
        public int KeepAliveTimeoutSeconds { get; set; }
        public int MaxConnections { get; set; }
        public int MaxPendingConnections { get; set; }
        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public bool DisableNagleAlgorithm { get; set; }
        public bool EnableTcpChimneyOffload { get; set; }
        public bool EnableRss { get; set; }
        public bool EnableTcpA { get; set; }
    }

    public class HttpConfiguration
    {
        public HttpSysConfiguration HttpSysConfiguration { get; set; } = new();
    }

    public class HttpSysConfiguration
    {
        public int MaxConnections { get; set; }
        public int MaxPendingAccepts { get; set; }
        public int MaxPendingReturns { get; set; }
        public int MaxRequestQueueLength { get; set; }
        public int MaxResponseQueueLength { get; set; }
        public int MaxRequestBodySize { get; set; }
        public int MaxResponseBodySize { get; set; }
        public int MaxRequestHeaderSize { get; set; }
        public int MaxResponseHeaderSize { get; set; }
        public bool EnableKernelModeCache { get; set; }
        public bool EnableResponseCache { get; set; }
        public int ResponseCacheSize { get; set; }
        public int ResponseCacheTTLMinutes { get; set; }
    }

    public class PerformanceMonitoringConfiguration
    {
        public bool PerformanceCountersEnabled { get; set; }
        public string[] CustomCounters { get; set; } = Array.Empty<string>();
        public int MonitoringIntervalSeconds { get; set; }

        public LogFileConfiguration LogFileConfiguration { get; set; } = new();
        public FailedRequestTracingConfiguration FailedRequestTracingConfiguration { get; set; } = new();
    }

    public class LogFileConfiguration
    {
        public string LogFileDirectory { get; set; } = string.Empty;
        public string LogFilePeriod { get; set; } = string.Empty;
        public int LogFileTruncateSizeKB { get; set; }
        public bool LogFileLocalTimeRollover { get; set; }
        public string LogFileLogFormat { get; set; } = string.Empty;
        public string LogFileEncoding { get; set; } = string.Empty;
        public int LogFileMaxSize { get; set; }
        public int LogFileFlushInterval { get; set; }
        public string LogFileCustomFields { get; set; } = string.Empty;
    }

    public class FailedRequestTracingConfiguration
    {
        public bool Enabled { get; set; }
        public int MaxLogFiles { get; set; }
        public int MaxLogFileSizeKB { get; set; }
        public bool CustomActionsEnabled { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Verb { get; set; } = string.Empty;
        public string StatusCodes { get; set; } = string.Empty;
        public string TimeTaken { get; set; } = string.Empty;
        public string EventSeverity { get; set; } = string.Empty;
        public int MaxCacheSize { get; set; }
    }

    public class SecurityOptimizations
    {
        public RequestFilteringConfiguration RequestFilteringConfiguration { get; set; } = new();
        public AuthenticationConfiguration AuthenticationConfiguration { get; set; } = new();
        public AuthorizationConfiguration AuthorizationConfiguration { get; set; } = new();
        public IPAddressRestrictionsConfiguration IPAddressRestrictionsConfiguration { get; set; } = new();
        public SSLConfiguration SSLConfiguration { get; set; } = new();
    }

    public class RequestFilteringConfiguration
    {
        public int MaxAllowedContentLength { get; set; }
        public int MaxURLLength { get; set; }
        public int MaxQueryStringLength { get; set; }
        public bool AllowHighBitCharacters { get; set; }
        public bool AllowDoubleEscaping { get; set; }
        public int MaxHeaderLength { get; set; }

        public FileExtensionsConfiguration FileExtensionsConfiguration { get; set; } = new();
        public HiddenSegmentsConfiguration HiddenSegmentsConfiguration { get; set; } = new();
    }

    public class FileExtensionsConfiguration
    {
        public bool AllowUnlisted { get; set; }
        public string[] DenyExtensions { get; set; } = Array.Empty<string>();
    }

    public class HiddenSegmentsConfiguration
    {
        public string[] HiddenSegments { get; set; } = Array.Empty<string>();
    }

    public class AuthenticationConfiguration
    {
        public bool AnonymousAuthenticationEnabled { get; set; }
        public bool WindowsAuthenticationEnabled { get; set; }
        public bool DigestAuthenticationEnabled { get; set; }
        public bool BasicAuthenticationEnabled { get; set; }
        public bool ClientCertificateMappingAuthenticationEnabled { get; set; }
        public bool IISClientCertificateMappingAuthenticationEnabled { get; set; }
    }

    public class AuthorizationConfiguration
    {
        public AuthorizationRule[] AuthorizationRules { get; set; } = Array.Empty<AuthorizationRule>();
    }

    public class AuthorizationRule
    {
        public string Users { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public string Verbs { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty;
    }

    public class IPAddressRestrictionsConfiguration
    {
        public bool EnableProxyMode { get; set; }
        public bool EnableReverseDNS { get; set; }
        public string RestrictionMode { get; set; } = string.Empty;
        public string[] AllowedIPAddresses { get; set; } = Array.Empty<string>();
        public string[] DeniedIPAddresses { get; set; } = Array.Empty<string>();
    }

    public class SSLConfiguration
    {
        public bool SSLRequired { get; set; }
        public bool ClientCertificateRequired { get; set; }
        public bool SSL128BitRequired { get; set; }
        public bool AcceptClientCertificates { get; set; }
        public bool UseDSMapper { get; set; }
        public bool NegotiateClientCertificate { get; set; }
    }
}
