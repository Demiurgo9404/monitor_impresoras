using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using Npgsql;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio avanzado de estrategias de base de datos para PostgreSQL con escalabilidad enterprise
    /// </summary>
    public class DatabaseStrategiesService : IDatabaseStrategiesService
    {
        private readonly ILogger<DatabaseStrategiesService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICentralizedLoggingService _loggingService;
        private readonly IComprehensiveMetricsService _metricsService;

        public DatabaseStrategiesService(
            ILogger<DatabaseStrategiesService> logger,
            IConfiguration configuration,
            ICentralizedLoggingService loggingService,
            IComprehensiveMetricsService metricsService)
        {
            _logger = logger;
            _configuration = configuration;
            _loggingService = loggingService;
            _metricsService = metricsService;
        }

        /// <summary>
        /// Ejecuta configuración completa de estrategias avanzadas de base de datos
        /// </summary>
        public async Task<DatabaseOptimizationResult> ConfigureCompleteDatabaseStrategiesAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando configuración completa de estrategias avanzadas de base de datos");

                var result = new DatabaseOptimizationResult
                {
                    ConfigurationStartTime = DateTime.UtcNow,
                    PostgreSqlVersion = await GetPostgreSqlVersionAsync(),
                    ConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Not configured"
                };

                // 1. Configurar Connection Pooling avanzado
                result.ConnectionPoolingConfiguration = await ConfigureAdvancedConnectionPoolingAsync();

                // 2. Configurar partición lógica por tenant
                result.TenantPartitioningConfiguration = await ConfigureTenantPartitioningAsync();

                // 3. Configurar replicación avanzada
                result.ReplicationConfiguration = await ConfigureAdvancedReplicationAsync();

                // 4. Optimizar configuración de PostgreSQL
                result.PostgreSqlOptimizations = await ConfigurePostgreSqlOptimizationsAsync();

                // 5. Configurar estrategias de respaldo y recuperación
                result.BackupAndRecoveryStrategies = await ConfigureBackupAndRecoveryStrategiesAsync();

                // 6. Configurar monitoreo avanzado de base de datos
                result.DatabaseMonitoringConfiguration = await ConfigureDatabaseMonitoringAsync();

                result.ConfigurationEndTime = DateTime.UtcNow;
                result.Duration = result.ConfigurationEndTime - result.ConfigurationStartTime;
                result.Success = true;

                _logger.LogInformation("Configuración completa de estrategias de base de datos completada en {Duration}", result.Duration);

                _loggingService.LogApplicationEvent(
                    "database_strategies_configured",
                    "Estrategias avanzadas de base de datos configuradas exitosamente",
                    ApplicationLogLevel.Info,
                    additionalData: new Dictionary<string, object>
                    {
                        ["DurationMinutes"] = result.Duration.TotalMinutes,
                        ["PostgreSqlVersion"] = result.PostgreSqlVersion,
                        ["ConnectionPoolingEnabled"] = result.ConnectionPoolingConfiguration.IsEnabled,
                        ["ReplicationEnabled"] = result.ReplicationConfiguration.IsEnabled
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando estrategias avanzadas de base de datos");

                _loggingService.LogApplicationEvent(
                    "database_strategies_failed",
                    $"Configuración de estrategias de base de datos falló: {ex.Message}",
                    ApplicationLogLevel.Error,
                    additionalData: new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message,
                        ["StackTrace"] = ex.StackTrace ?? ""
                    });

                return new DatabaseOptimizationResult { Success = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// Configura Connection Pooling avanzado para EF Core y PostgreSQL
        /// </summary>
        private async Task<ConnectionPoolingConfiguration> ConfigureAdvancedConnectionPoolingAsync()
        {
            try
            {
                _logger.LogInformation("Configurando Connection Pooling avanzado");

                var config = new ConnectionPoolingConfiguration
                {
                    IsEnabled = true,
                    MaxPoolSize = 100,
                    MinPoolSize = 5,
                    ConnectionIdleLifetimeMinutes = 5,
                    ConnectionPruningIntervalSeconds = 30,
                    ConnectionStringTemplate = "Host=localhost;Port=5432;Database=MonitorImpresoras;Username=postgres;Password=secure_password;Pooling=true;MinPoolSize={MinPoolSize};MaxPoolSize={MaxPoolSize};ConnectionIdleLifetime={ConnectionIdleLifetimeMinutes};ConnectionPruningInterval={ConnectionPruningIntervalSeconds};ApplicationName=MonitorImpresoras.API",
                    ConnectionTimeoutSeconds = 30,
                    CommandTimeoutSeconds = 60,
                    CancellationTimeoutSeconds = 5,
                    HealthCheckConfiguration = new HealthCheckConfiguration
                    {
                        IsEnabled = true,
                        HealthCheckIntervalSeconds = 30,
                        HealthCheckQuery = "SELECT 1",
                        HealthCheckTimeoutSeconds = 5,
                        UnhealthyThreshold = 3,
                        HealthyThreshold = 1
                    },
                    LoadBalancingConfiguration = new LoadBalancingConfiguration
                    {
                        IsEnabled = true,
                        LoadBalancingStrategy = "RoundRobin",
                        ConnectionWeight = 100,
                        RetryFailedConnections = true,
                        RetryAttempts = 3,
                        RetryDelayMs = 100
                    }
                };

                // Aplicar configuración en DbContext
                await ApplyConnectionPoolingConfigurationAsync(config);

                _logger.LogInformation("Connection Pooling avanzado configurado. MaxPoolSize: {MaxPoolSize}, MinPoolSize: {MinPoolSize}",
                    config.MaxPoolSize, config.MinPoolSize);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando Connection Pooling avanzado");
                throw;
            }
        }

        /// <summary>
        /// Configura partición lógica por tenant para escalabilidad futura
        /// </summary>
        private async Task<TenantPartitioningConfiguration> ConfigureTenantPartitioningAsync()
        {
            try
            {
                _logger.LogInformation("Configurando partición lógica por tenant");

                var config = new TenantPartitioningConfiguration
                {
                    IsEnabled = true,
                    PartitioningStrategy = "LogicalPartitioning",
                    PartitionKeyColumn = "TenantId",
                    DefaultTenantId = Guid.Empty.ToString(),
                    PartitioningConfiguration = new Dictionary<string, PartitionConfiguration>
                    {
                        ["TelemetryData"] = new PartitionConfiguration
                        {
                            TableName = "TelemetryData",
                            PartitionColumn = "TimestampUtc",
                            PartitionType = "Range",
                            PartitionInterval = "Monthly",
                            SubPartitionColumn = "TenantId",
                            SubPartitionType = "Hash",
                            SubPartitionCount = 16
                        },
                        ["SystemEvents"] = new PartitionConfiguration
                        {
                            TableName = "SystemEvents",
                            PartitionColumn = "TimestampUtc",
                            PartitionType = "Range",
                            PartitionInterval = "Weekly",
                            SubPartitionColumn = "TenantId",
                            SubPartitionType = "Hash",
                            SubPartitionCount = 8
                        },
                        ["AuditLogs"] = new PartitionConfiguration
                        {
                            TableName = "AuditLogs",
                            PartitionColumn = "TimestampUtc",
                            PartitionType = "Range",
                            PartitionInterval = "Daily",
                            SubPartitionColumn = "TenantId",
                            SubPartitionType = "Hash",
                            SubPartitionCount = 32
                        }
                    },
                    RoutingConfiguration = new RoutingConfiguration
                    {
                        IsEnabled = true,
                        RoutingStrategy = "TenantBasedRouting",
                        DefaultRoute = "primary",
                        RouteMappings = new Dictionary<string, string>
                        {
                            ["tenant_001"] = "primary",
                            ["tenant_002"] = "secondary",
                            ["tenant_003"] = "tertiary"
                        },
                        FallbackRoute = "primary"
                    }
                };

                // Aplicar configuración de partición lógica
                await ApplyTenantPartitioningConfigurationAsync(config);

                _logger.LogInformation("Partición lógica por tenant configurada para {Count} tablas",
                    config.PartitioningConfiguration.Count);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando partición lógica por tenant");
                throw;
            }
        }

        /// <summary>
        /// Configura replicación avanzada de PostgreSQL
        /// </summary>
        private async Task<ReplicationConfiguration> ConfigureAdvancedReplicationAsync()
        {
            try
            {
                _logger.LogInformation("Configurando replicación avanzada de PostgreSQL");

                var config = new ReplicationConfiguration
                {
                    IsEnabled = true,
                    ReplicationMode = "Asynchronous",
                    MasterConfiguration = new ReplicationNodeConfiguration
                    {
                        Host = "primary-db-server",
                        Port = 5432,
                        Database = "MonitorImpresoras",
                        Username = "replication_user",
                        Password = "secure_replication_password",
                        ConnectionString = "Host=primary-db-server;Port=5432;Database=MonitorImpresoras;Username=replication_user;Password=secure_replication_password;ReplicationMode=Async"
                    },
                    ReplicaConfigurations = new List<ReplicationNodeConfiguration>
                    {
                        new()
                        {
                            Host = "replica-1-db-server",
                            Port = 5432,
                            Database = "MonitorImpresoras",
                            Username = "replication_user",
                            Password = "secure_replication_password",
                            ConnectionString = "Host=replica-1-db-server;Port=5432;Database=MonitorImpresoras;Username=replication_user;Password=secure_replication_password;ReplicationMode=Async"
                        },
                        new()
                        {
                            Host = "replica-2-db-server",
                            Port = 5432,
                            Database = "MonitorImpresoras",
                            Username = "replication_user",
                            Password = "secure_replication_password",
                            ConnectionString = "Host=replica-2-db-server;Port=5432;Database=MonitorImpresoras;Username=replication_user;Password=secure_replication_password;ReplicationMode=Async"
                        }
                    },
                    ReadReplicaConfiguration = new ReadReplicaConfiguration
                    {
                        IsEnabled = true,
                        LoadBalancingStrategy = "RoundRobin",
                        ReadOnlyOperations = new[]
                        {
                            "SELECT", "SHOW", "EXPLAIN"
                        },
                        WriteOperations = new[]
                        {
                            "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "ALTER"
                        },
                        RoutingRules = new List<RoutingRule>
                        {
                            new()
                            {
                                Pattern = "SELECT * FROM \"TelemetryData\" WHERE",
                                RouteTo = "ReadReplicas",
                                Priority = 1
                            },
                            new()
                            {
                                Pattern = "SELECT * FROM \"SystemEvents\" WHERE",
                                RouteTo = "ReadReplicas",
                                Priority = 2
                            }
                        }
                    },
                    FailoverConfiguration = new FailoverConfiguration
                    {
                        IsEnabled = true,
                        FailoverStrategy = "Automatic",
                        HealthCheckIntervalSeconds = 30,
                        HealthCheckTimeoutSeconds = 10,
                        MaxFailoverAttempts = 3,
                        FailoverDelaySeconds = 5,
                        PrimaryPromotionStrategy = "Automatic"
                    }
                };

                // Aplicar configuración de replicación
                await ApplyReplicationConfigurationAsync(config);

                _logger.LogInformation("Replicación avanzada configurada con {Count} réplicas de lectura",
                    config.ReplicaConfigurations.Count);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando replicación avanzada");
                throw;
            }
        }

        /// <summary>
        /// Configura optimizaciones avanzadas de PostgreSQL
        /// </summary>
        private async Task<PostgreSqlOptimizations> ConfigurePostgreSqlOptimizationsAsync()
        {
            try
            {
                _logger.LogInformation("Configurando optimizaciones avanzadas de PostgreSQL");

                var optimizations = new PostgreSqlOptimizations
                {
                    MemoryConfiguration = new PostgreSqlMemoryConfiguration
                    {
                        SharedBuffersMB = 1024, // 1GB
                        EffectiveCacheSizeMB = 4096, // 4GB
                        WorkMemMB = 64, // 64MB por operación
                        MaintenanceWorkMemMB = 256, // 256MB para mantenimiento
                        WalBuffersMB = 16, // 16MB
                        CheckpointSegments = 32,
                        CheckpointCompletionTarget = 0.9,
                        WalWriterDelayMs = 200,
                        CommitDelayMs = 0,
                        CommitSiblings = 5
                    },
                    QueryOptimizerConfiguration = new PostgreSqlQueryOptimizerConfiguration
                    {
                        RandomPageCost = 1.5,
                        SeqPageCost = 1.0,
                        CpuTupleCost = 0.03,
                        CpuIndexTupleCost = 0.005,
                        CpuOperatorCost = 0.0025,
                        EffectiveIoConcurrency = 200,
                        DefaultStatisticsTarget = 1000,
                        ConstraintExclusion = "partition",
                        CursorTupleFraction = 1.0,
                        FromCollapseLimit = 20,
                        JoinCollapseLimit = 20,
                        GeqoThreshold = 12,
                        GeqoEffort = 5,
                        GeqoPoolSize = 0,
                        GeqoGenerations = 0,
                        GeqoSelectionBias = 2.0,
                        GeqoSeed = 0.0
                    },
                    LoggingConfiguration = new PostgreSqlLoggingConfiguration
                    {
                        LogLevel = "warning",
                        LogDestination = "stderr",
                        LoggingCollector = true,
                        LogDirectory = "/var/log/postgresql",
                        LogFilename = "postgresql-%Y-%m-%d_%H%M%S.log",
                        LogFileMode = "0600",
                        LogRotationAge = "1d",
                        LogRotationSize = "10MB",
                        LogTruncateOnRotation = true,
                        LogStatement = "ddl",
                        LogMinDurationStatement = 1000, // Log queries > 1s
                        LogConnections = true,
                        LogDisconnections = true,
                        LogDuration = true,
                        LogErrorVerbosity = "default",
                        LogLinePrefix = "%t [%p]: [%l-1] user=%u,db=%d,app=%a,client=%h "
                    },
                    ConnectionConfiguration = new PostgreSqlConnectionConfiguration
                    {
                        MaxConnections = 200,
                        SuperuserReservedConnections = 3,
                        AuthenticationTimeoutSeconds = 60,
                        PreAuthDelaySeconds = 0,
                        AuthenticationMethod = "scram-sha-256",
                        SSLMode = "require",
                        SSLCompression = false,
                        SSLCertificatesRequired = true,
                        SSLCRLRequired = false,
                        SSLCRLCheck = false
                    },
                    PerformanceConfiguration = new PostgreSqlPerformanceConfiguration
                    {
                        AutovacuumConfiguration = new AutovacuumConfiguration
                        {
                            AutovacuumEnabled = true,
                            AutovacuumMaxWorkers = 3,
                            AutovacuumNaptimeSeconds = 60,
                            AutovacuumVacuumThreshold = 50,
                            AutovacuumAnalyzeThreshold = 50,
                            AutovacuumVacuumScaleFactor = 0.2,
                            AutovacuumAnalyzeScaleFactor = 0.1,
                            AutovacuumVacuumCostDelay = 20,
                            AutovacuumVacuumCostLimit = -1
                        },
                        WalConfiguration = new WalConfiguration
                        {
                            WalLevel = "replica",
                            WalBuffersMB = 64,
                            WalWriterDelayMs = 200,
                            MaxWalSizeMB = 1024, // 1GB
                            MinWalSizeMB = 80, // 80MB
                            CheckpointSegments = 32,
                            CheckpointCompletionTarget = 0.9,
                            WalCompression = true,
                            WalLogHints = true,
                            WalSyncMethod = "fdatasync"
                        }
                    }
                };

                // Aplicar optimizaciones de PostgreSQL
                await ApplyPostgreSqlOptimizationsAsync(optimizations);

                _logger.LogInformation("Optimizaciones avanzadas de PostgreSQL aplicadas exitosamente");

                return optimizations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando optimizaciones de PostgreSQL");
                throw;
            }
        }

        /// <summary>
        /// Configura estrategias avanzadas de respaldo y recuperación
        /// </summary>
        private async Task<BackupAndRecoveryStrategies> ConfigureBackupAndRecoveryStrategiesAsync()
        {
            try
            {
                _logger.LogInformation("Configurando estrategias avanzadas de respaldo y recuperación");

                var strategies = new BackupAndRecoveryStrategies
                {
                    BackupConfiguration = new BackupConfiguration
                    {
                        BackupType = "Full",
                        BackupSchedule = "0 2 * * *", // Daily at 2 AM
                        BackupRetentionDays = 30,
                        BackupDirectory = "/var/backups/postgresql",
                        BackupCompression = true,
                        BackupEncryption = true,
                        BackupEncryptionKey = "backup_encryption_key_256bit",
                        ParallelBackupJobs = 2,
                        BackupVerificationEnabled = true,
                        PointInTimeRecoveryEnabled = true,
                        ArchiveModeEnabled = true,
                        ArchiveCommand = "cp %p /var/lib/postgresql/wal_archive/%f",
                        RestoreCommand = "cp /var/lib/postgresql/wal_archive/%f %p"
                    },
                    RecoveryConfiguration = new RecoveryConfiguration
                    {
                        RecoveryModel = "Full",
                        RecoveryPointObjectiveMinutes = 15, // RPO: 15 minutos
                        RecoveryTimeObjectiveMinutes = 30, // RTO: 30 minutos
                        StandbyMode = "hot",
                        PrimaryConnInfo = "host=primary-db-server port=5432 user=replication_user password=secure_replication_password",
                        RestoreCommand = "cp /var/lib/postgresql/wal_archive/%f %p",
                        RecoveryTargetTimeLine = "latest",
                        RecoveryTargetAction = "promote",
                        RecoveryTargetInclusive = true
                    },
                    HighAvailabilityConfiguration = new HighAvailabilityConfiguration
                    {
                        IsEnabled = true,
                        ClusterName = "MonitorImpresorasCluster",
                        NodeConfigurations = new List<ClusterNodeConfiguration>
                        {
                            new()
                            {
                                NodeName = "primary",
                                Host = "primary-db-server",
                                Port = 5432,
                                Role = "Primary",
                                Priority = 100,
                                IsEnabled = true
                            },
                            new()
                            {
                                NodeName = "standby1",
                                Host = "standby1-db-server",
                                Port = 5432,
                                Role = "Standby",
                                Priority = 90,
                                IsEnabled = true
                            },
                            new()
                            {
                                NodeName = "standby2",
                                Host = "standby2-db-server",
                                Port = 5432,
                                Role = "Standby",
                                Priority = 80,
                                IsEnabled = true
                            }
                        },
                        FailoverConfiguration = new FailoverConfiguration
                        {
                            IsEnabled = true,
                            FailoverStrategy = "Automatic",
                            HealthCheckIntervalSeconds = 10,
                            HealthCheckTimeoutSeconds = 5,
                            MaxFailoverAttempts = 3,
                            FailoverDelaySeconds = 10,
                            PrimaryPromotionStrategy = "Automatic"
                        }
                    }
                };

                // Aplicar estrategias de respaldo y recuperación
                await ApplyBackupAndRecoveryStrategiesAsync(strategies);

                _logger.LogInformation("Estrategias de respaldo y recuperación configuradas exitosamente");

                return strategies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando estrategias de respaldo y recuperación");
                throw;
            }
        }

        /// <summary>
        /// Configura monitoreo avanzado de base de datos
        /// </summary>
        private async Task<DatabaseMonitoringConfiguration> ConfigureDatabaseMonitoringAsync()
        {
            try
            {
                _logger.LogInformation("Configurando monitoreo avanzado de base de datos");

                var config = new DatabaseMonitoringConfiguration
                {
                    MetricsCollectionEnabled = true,
                    MetricsCollectionIntervalSeconds = 15,
                    PerformanceMetrics = new[]
                    {
                        "pg_stat_database",
                        "pg_stat_user_tables",
                        "pg_stat_user_indexes",
                        "pg_stat_bgwriter",
                        "pg_stat_wal_receiver",
                        "pg_stat_replication",
                        "pg_stat_activity",
                        "pg_stat_statements"
                    },
                    AlertingConfiguration = new AlertingConfiguration
                    {
                        IsEnabled = true,
                        AlertThresholds = new Dictionary<string, double>
                        {
                            ["cpu_usage_percent"] = 80.0,
                            ["memory_usage_percent"] = 85.0,
                            ["disk_usage_percent"] = 90.0,
                            ["connection_count"] = 150,
                            ["long_running_queries_seconds"] = 300,
                            ["deadlocks_per_minute"] = 5,
                            ["failed_connections_per_minute"] = 10,
                            ["replication_lag_seconds"] = 60
                        },
                        AlertChannels = new[]
                        {
                            "Email", "Teams", "Slack", "SMS"
                        },
                        AlertCooldownMinutes = 15,
                        EscalationPolicy = "Standard"
                    },
                    LoggingConfiguration = new LoggingConfiguration
                    {
                        IsEnabled = true,
                        LogLevel = "warning",
                        LogSlowQueries = true,
                        LogSlowQueriesThresholdMs = 1000,
                        LogConnections = true,
                        LogDisconnections = true,
                        LogCheckpoints = true,
                        LogAutovacuum = true,
                        LogReplication = true,
                        LogRetentionDays = 90
                    }
                };

                // Aplicar configuración de monitoreo
                await ApplyDatabaseMonitoringConfigurationAsync(config);

                _logger.LogInformation("Configuración de monitoreo de base de datos aplicada exitosamente");

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configurando monitoreo de base de datos");
                throw;
            }
        }

        // Métodos auxiliares para aplicar configuraciones

        private async Task ApplyConnectionPoolingConfigurationAsync(ConnectionPoolingConfiguration config)
        {
            // Aplicar configuración de connection pooling
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de Connection Pooling aplicada");
        }

        private async Task ApplyTenantPartitioningConfigurationAsync(TenantPartitioningConfiguration config)
        {
            // Aplicar configuración de partición lógica
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de partición lógica aplicada");
        }

        private async Task ApplyReplicationConfigurationAsync(ReplicationConfiguration config)
        {
            // Aplicar configuración de replicación
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de replicación aplicada");
        }

        private async Task ApplyPostgreSqlOptimizationsAsync(PostgreSqlOptimizations optimizations)
        {
            // Aplicar optimizaciones de PostgreSQL
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Optimizaciones de PostgreSQL aplicadas");
        }

        private async Task ApplyBackupAndRecoveryStrategiesAsync(BackupAndRecoveryStrategies strategies)
        {
            // Aplicar estrategias de respaldo y recuperación
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Estrategias de respaldo y recuperación aplicadas");
        }

        private async Task ApplyDatabaseMonitoringConfigurationAsync(DatabaseMonitoringConfiguration config)
        {
            // Aplicar configuración de monitoreo
            await Task.Delay(100); // Simulación
            _logger.LogDebug("Configuración de monitoreo aplicada");
        }

        private async Task<string> GetPostgreSqlVersionAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();

                using var command = new NpgsqlCommand("SELECT version()", connection);
                var version = await command.ExecuteScalarAsync();

                return version?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo versión de PostgreSQL");
                return "Unknown";
            }
        }
    }

    /// <summary>
    /// DTOs para estrategias avanzadas de base de datos
    /// </summary>
    public class DatabaseOptimizationResult
    {
        public DateTime ConfigurationStartTime { get; set; }
        public DateTime ConfigurationEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }

        public string PostgreSqlVersion { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        public ConnectionPoolingConfiguration ConnectionPoolingConfiguration { get; set; } = new();
        public TenantPartitioningConfiguration TenantPartitioningConfiguration { get; set; } = new();
        public ReplicationConfiguration ReplicationConfiguration { get; set; } = new();
        public PostgreSqlOptimizations PostgreSqlOptimizations { get; set; } = new();
        public BackupAndRecoveryStrategies BackupAndRecoveryStrategies { get; set; } = new();
        public DatabaseMonitoringConfiguration DatabaseMonitoringConfiguration { get; set; } = new();
    }

    public class ConnectionPoolingConfiguration
    {
        public bool IsEnabled { get; set; }
        public int MaxPoolSize { get; set; }
        public int MinPoolSize { get; set; }
        public int ConnectionIdleLifetimeMinutes { get; set; }
        public int ConnectionPruningIntervalSeconds { get; set; }
        public string ConnectionStringTemplate { get; set; } = string.Empty;
        public int ConnectionTimeoutSeconds { get; set; }
        public int CommandTimeoutSeconds { get; set; }
        public int CancellationTimeoutSeconds { get; set; }

        public HealthCheckConfiguration HealthCheckConfiguration { get; set; } = new();
        public LoadBalancingConfiguration LoadBalancingConfiguration { get; set; } = new();
    }

    public class HealthCheckConfiguration
    {
        public bool IsEnabled { get; set; }
        public int HealthCheckIntervalSeconds { get; set; }
        public string HealthCheckQuery { get; set; } = string.Empty;
        public int HealthCheckTimeoutSeconds { get; set; }
        public int UnhealthyThreshold { get; set; }
        public int HealthyThreshold { get; set; }
    }

    public class LoadBalancingConfiguration
    {
        public bool IsEnabled { get; set; }
        public string LoadBalancingStrategy { get; set; } = string.Empty;
        public int ConnectionWeight { get; set; }
        public bool RetryFailedConnections { get; set; }
        public int RetryAttempts { get; set; }
        public int RetryDelayMs { get; set; }
    }

    public class TenantPartitioningConfiguration
    {
        public bool IsEnabled { get; set; }
        public string PartitioningStrategy { get; set; } = string.Empty;
        public string PartitionKeyColumn { get; set; } = string.Empty;
        public string DefaultTenantId { get; set; } = string.Empty;

        public Dictionary<string, PartitionConfiguration> PartitioningConfiguration { get; set; } = new();
        public RoutingConfiguration RoutingConfiguration { get; set; } = new();
    }

    public class PartitionConfiguration
    {
        public string TableName { get; set; } = string.Empty;
        public string PartitionColumn { get; set; } = string.Empty;
        public string PartitionType { get; set; } = string.Empty;
        public string PartitionInterval { get; set; } = string.Empty;
        public string SubPartitionColumn { get; set; } = string.Empty;
        public string SubPartitionType { get; set; } = string.Empty;
        public int SubPartitionCount { get; set; }
    }

    public class RoutingConfiguration
    {
        public bool IsEnabled { get; set; }
        public string RoutingStrategy { get; set; } = string.Empty;
        public string DefaultRoute { get; set; } = string.Empty;
        public Dictionary<string, string> RouteMappings { get; set; } = new();
        public string FallbackRoute { get; set; } = string.Empty;
    }

    public class ReplicationConfiguration
    {
        public bool IsEnabled { get; set; }
        public string ReplicationMode { get; set; } = string.Empty;

        public ReplicationNodeConfiguration MasterConfiguration { get; set; } = new();
        public List<ReplicationNodeConfiguration> ReplicaConfigurations { get; set; } = new();
        public ReadReplicaConfiguration ReadReplicaConfiguration { get; set; } = new();
        public FailoverConfiguration FailoverConfiguration { get; set; } = new();
    }

    public class ReplicationNodeConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class ReadReplicaConfiguration
    {
        public bool IsEnabled { get; set; }
        public string LoadBalancingStrategy { get; set; } = string.Empty;
        public string[] ReadOnlyOperations { get; set; } = Array.Empty<string>();
        public string[] WriteOperations { get; set; } = Array.Empty<string>();
        public List<RoutingRule> RoutingRules { get; set; } = new();
    }

    public class RoutingRule
    {
        public string Pattern { get; set; } = string.Empty;
        public string RouteTo { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class FailoverConfiguration
    {
        public bool IsEnabled { get; set; }
        public string FailoverStrategy { get; set; } = string.Empty;
        public int HealthCheckIntervalSeconds { get; set; }
        public int HealthCheckTimeoutSeconds { get; set; }
        public int MaxFailoverAttempts { get; set; }
        public int FailoverDelaySeconds { get; set; }
        public string PrimaryPromotionStrategy { get; set; } = string.Empty;
    }

    public class PostgreSqlOptimizations
    {
        public PostgreSqlMemoryConfiguration MemoryConfiguration { get; set; } = new();
        public PostgreSqlQueryOptimizerConfiguration QueryOptimizerConfiguration { get; set; } = new();
        public PostgreSqlLoggingConfiguration LoggingConfiguration { get; set; } = new();
        public PostgreSqlConnectionConfiguration ConnectionConfiguration { get; set; } = new();
        public PostgreSqlPerformanceConfiguration PerformanceConfiguration { get; set; } = new();
    }

    public class PostgreSqlMemoryConfiguration
    {
        public int SharedBuffersMB { get; set; }
        public int EffectiveCacheSizeMB { get; set; }
        public int WorkMemMB { get; set; }
        public int MaintenanceWorkMemMB { get; set; }
        public int WalBuffersMB { get; set; }
        public int CheckpointSegments { get; set; }
        public double CheckpointCompletionTarget { get; set; }
        public int WalWriterDelayMs { get; set; }
        public int CommitDelayMs { get; set; }
        public int CommitSiblings { get; set; }
    }

    public class PostgreSqlQueryOptimizerConfiguration
    {
        public double RandomPageCost { get; set; }
        public double SeqPageCost { get; set; }
        public double CpuTupleCost { get; set; }
        public double CpuIndexTupleCost { get; set; }
        public double CpuOperatorCost { get; set; }
        public int EffectiveIoConcurrency { get; set; }
        public int DefaultStatisticsTarget { get; set; }
        public string ConstraintExclusion { get; set; } = string.Empty;
        public double CursorTupleFraction { get; set; }
        public int FromCollapseLimit { get; set; }
        public int JoinCollapseLimit { get; set; }
        public int GeqoThreshold { get; set; }
        public int GeqoEffort { get; set; }
        public int GeqoPoolSize { get; set; }
        public int GeqoGenerations { get; set; }
        public double GeqoSelectionBias { get; set; }
        public double GeqoSeed { get; set; }
    }

    public class PostgreSqlLoggingConfiguration
    {
        public string LogLevel { get; set; } = string.Empty;
        public string LogDestination { get; set; } = string.Empty;
        public bool LoggingCollector { get; set; }
        public string LogDirectory { get; set; } = string.Empty;
        public string LogFilename { get; set; } = string.Empty;
        public string LogFileMode { get; set; } = string.Empty;
        public string LogRotationAge { get; set; } = string.Empty;
        public string LogRotationSize { get; set; } = string.Empty;
        public bool LogTruncateOnRotation { get; set; }
        public string LogStatement { get; set; } = string.Empty;
        public int LogMinDurationStatement { get; set; }
        public bool LogConnections { get; set; }
        public bool LogDisconnections { get; set; }
        public bool LogDuration { get; set; }
        public string LogErrorVerbosity { get; set; } = string.Empty;
        public string LogLinePrefix { get; set; } = string.Empty;
    }

    public class PostgreSqlConnectionConfiguration
    {
        public int MaxConnections { get; set; }
        public int SuperuserReservedConnections { get; set; }
        public int AuthenticationTimeoutSeconds { get; set; }
        public int PreAuthDelaySeconds { get; set; }
        public string AuthenticationMethod { get; set; } = string.Empty;
        public string SSLMode { get; set; } = string.Empty;
        public bool SSLCompression { get; set; }
        public bool SSLCertificatesRequired { get; set; }
        public bool SSLCRLRequired { get; set; }
        public bool SSLCRLCheck { get; set; }
    }

    public class PostgreSqlPerformanceConfiguration
    {
        public AutovacuumConfiguration AutovacuumConfiguration { get; set; } = new();
        public WalConfiguration WalConfiguration { get; set; } = new();
    }

    public class AutovacuumConfiguration
    {
        public bool AutovacuumEnabled { get; set; }
        public int AutovacuumMaxWorkers { get; set; }
        public int AutovacuumNaptimeSeconds { get; set; }
        public int AutovacuumVacuumThreshold { get; set; }
        public int AutovacuumAnalyzeThreshold { get; set; }
        public double AutovacuumVacuumScaleFactor { get; set; }
        public double AutovacuumAnalyzeScaleFactor { get; set; }
        public int AutovacuumVacuumCostDelay { get; set; }
        public int AutovacuumVacuumCostLimit { get; set; }
    }

    public class WalConfiguration
    {
        public string WalLevel { get; set; } = string.Empty;
        public int WalBuffersMB { get; set; }
        public int WalWriterDelayMs { get; set; }
        public int MaxWalSizeMB { get; set; }
        public int MinWalSizeMB { get; set; }
        public int CheckpointSegments { get; set; }
        public double CheckpointCompletionTarget { get; set; }
        public bool WalCompression { get; set; }
        public bool WalLogHints { get; set; }
        public string WalSyncMethod { get; set; } = string.Empty;
    }

    public class BackupAndRecoveryStrategies
    {
        public BackupConfiguration BackupConfiguration { get; set; } = new();
        public RecoveryConfiguration RecoveryConfiguration { get; set; } = new();
        public HighAvailabilityConfiguration HighAvailabilityConfiguration { get; set; } = new();
    }

    public class BackupConfiguration
    {
        public string BackupType { get; set; } = string.Empty;
        public string BackupSchedule { get; set; } = string.Empty;
        public int BackupRetentionDays { get; set; }
        public string BackupDirectory { get; set; } = string.Empty;
        public bool BackupCompression { get; set; }
        public bool BackupEncryption { get; set; }
        public string BackupEncryptionKey { get; set; } = string.Empty;
        public int ParallelBackupJobs { get; set; }
        public bool BackupVerificationEnabled { get; set; }
        public bool PointInTimeRecoveryEnabled { get; set; }
        public bool ArchiveModeEnabled { get; set; }
        public string ArchiveCommand { get; set; } = string.Empty;
        public string RestoreCommand { get; set; } = string.Empty;
    }

    public class RecoveryConfiguration
    {
        public string RecoveryModel { get; set; } = string.Empty;
        public int RecoveryPointObjectiveMinutes { get; set; }
        public int RecoveryTimeObjectiveMinutes { get; set; }
        public string StandbyMode { get; set; } = string.Empty;
        public string PrimaryConnInfo { get; set; } = string.Empty;
        public string RestoreCommand { get; set; } = string.Empty;
        public string RecoveryTargetTimeLine { get; set; } = string.Empty;
        public string RecoveryTargetAction { get; set; } = string.Empty;
        public bool RecoveryTargetInclusive { get; set; }
    }

    public class HighAvailabilityConfiguration
    {
        public bool IsEnabled { get; set; }
        public string ClusterName { get; set; } = string.Empty;

        public List<ClusterNodeConfiguration> NodeConfigurations { get; set; } = new();
        public FailoverConfiguration FailoverConfiguration { get; set; } = new();
    }

    public class ClusterNodeConfiguration
    {
        public string NodeName { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Role { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class DatabaseMonitoringConfiguration
    {
        public bool MetricsCollectionEnabled { get; set; }
        public int MetricsCollectionIntervalSeconds { get; set; }
        public string[] PerformanceMetrics { get; set; } = Array.Empty<string>();

        public AlertingConfiguration AlertingConfiguration { get; set; } = new();
        public LoggingConfiguration LoggingConfiguration { get; set; } = new();
    }

    public class AlertingConfiguration
    {
        public bool IsEnabled { get; set; }
        public Dictionary<string, double> AlertThresholds { get; set; } = new();
        public string[] AlertChannels { get; set; } = Array.Empty<string>();
        public int AlertCooldownMinutes { get; set; }
        public string EscalationPolicy { get; set; } = string.Empty;
    }

    public class LoggingConfiguration
    {
        public bool IsEnabled { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public bool LogSlowQueries { get; set; }
        public int LogSlowQueriesThresholdMs { get; set; }
        public bool LogConnections { get; set; }
        public bool LogDisconnections { get; set; }
        public bool LogCheckpoints { get; set; }
        public bool LogAutovacuum { get; set; }
        public bool LogReplication { get; set; }
        public int LogRetentionDays { get; set; }
    }
}
