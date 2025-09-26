-- Script SQL para crear la base de datos MonitorImpresoras
-- Ejecutar este script en SQL Server Management Studio o desde la línea de comandos

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MonitorImpresoras')
BEGIN
    CREATE DATABASE [MonitorImpresoras];
    PRINT 'Base de datos MonitorImpresoras creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos MonitorImpresoras ya existe.';
END
GO

USE [MonitorImpresoras];
GO

-- Crear tabla AspNetRoles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoles' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla AspNetRoles creada exitosamente.';
END
GO

-- Crear tabla AspNetUsers
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUsers' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Department] nvarchar(max) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpiryTime] datetime2 NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla AspNetUsers creada exitosamente.';
END
GO

-- Crear tabla AspNetRoleClaims
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetRoleClaims' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetRoleClaims creada exitosamente.';
END
GO

-- Crear tabla AspNetUserClaims
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserClaims' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserClaims creada exitosamente.';
END
GO

-- Crear tabla AspNetUserLogins
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserLogins' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserLogins creada exitosamente.';
END
GO

-- Crear tabla AspNetUserRoles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserRoles creada exitosamente.';
END
GO

-- Crear tabla AspNetUserTokens
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AspNetUserTokens' AND xtype='U')
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Tabla AspNetUserTokens creada exitosamente.';
END
GO

-- Crear tabla Printers
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Printers' AND xtype='U')
BEGIN
    CREATE TABLE [Printers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Model] nvarchar(100) NOT NULL,
        [SerialNumber] nvarchar(100) NOT NULL,
        [IpAddress] nvarchar(50) NOT NULL,
        [Location] nvarchar(200) NULL,
        [Status] nvarchar(50) NULL DEFAULT 'Unknown',
        [IsOnline] bit NOT NULL DEFAULT 0,
        [IsLocalPrinter] bit NOT NULL DEFAULT 0,
        [CommunityString] nvarchar(50) NULL DEFAULT 'public',
        [SnmpPort] int NULL DEFAULT 161,
        [PageCount] int NULL,
        [LastMaintenance] datetime2 NULL,
        [MaintenanceIntervalDays] int NULL DEFAULT 90,
        [Notes] nvarchar(500) NULL,
        [LastError] nvarchar(1000) NULL,
        [LastChecked] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Printers] PRIMARY KEY ([Id])
    );

    -- Crear índices únicos
    CREATE UNIQUE INDEX [IX_Printers_IpAddress] ON [Printers] ([IpAddress]);
    CREATE UNIQUE INDEX [IX_Printers_SerialNumber] ON [Printers] ([SerialNumber]);
    CREATE INDEX [IX_Printers_IsOnline] ON [Printers] ([IsOnline]);
    CREATE INDEX [IX_Printers_Status] ON [Printers] ([Status]);

    PRINT 'Tabla Printers creada exitosamente.';
END
GO

-- Crear tabla PrinterConsumables
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PrinterConsumables' AND xtype='U')
BEGIN
    CREATE TABLE [PrinterConsumables] (
        [Id] int NOT NULL IDENTITY,
        [PrinterId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [PartNumber] nvarchar(50) NULL,
        [MaxCapacity] int NULL,
        [CurrentLevel] int NULL,
        [Unit] nvarchar(20) NULL,
        [WarningLevel] int NULL,
        [CriticalLevel] int NULL,
        [LastUpdated] datetime2 NULL,
        [Status] nvarchar(max) NULL,
        [RemainingPages] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_PrinterConsumables] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrinterConsumables_Printers_PrinterId] FOREIGN KEY ([PrinterId]) REFERENCES [Printers] ([Id]) ON DELETE CASCADE
    );

    -- Crear índices
    CREATE INDEX [IX_PrinterConsumables_PrinterId] ON [PrinterConsumables] ([PrinterId]);
    CREATE INDEX [IX_PrinterConsumables_Type] ON [PrinterConsumables] ([Type]);
    CREATE INDEX [IX_PrinterConsumables_Status] ON [PrinterConsumables] ([Status]);

    PRINT 'Tabla PrinterConsumables creada exitosamente.';
END
GO

-- Crear tabla PrintJobs
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PrintJobs' AND xtype='U')
BEGIN
    CREATE TABLE [PrintJobs] (
        [Id] int NOT NULL IDENTITY,
        [PrinterId] int NOT NULL,
        [UserId] nvarchar(450) NULL,
        [DocumentName] nvarchar(100) NOT NULL,
        [Pages] int NOT NULL,
        [IsColor] bit NOT NULL,
        [IsDuplex] bit NOT NULL,
        [PrintedAt] datetime2 NOT NULL,
        [JobStatus] nvarchar(20) NULL,
        [ErrorMessage] nvarchar(500) NULL,
        [Cost] decimal(18,2) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_PrintJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrintJobs_Printers_PrinterId] FOREIGN KEY ([PrinterId]) REFERENCES [Printers] ([Id]),
        CONSTRAINT [FK_PrintJobs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
    );

    -- Crear índices
    CREATE INDEX [IX_PrintJobs_PrinterId] ON [PrintJobs] ([PrinterId]);
    CREATE INDEX [IX_PrintJobs_UserId] ON [PrintJobs] ([UserId]);
    CREATE INDEX [IX_PrintJobs_PrintedAt] ON [PrintJobs] ([PrintedAt]);

    PRINT 'Tabla PrintJobs creada exitosamente.';
END
GO

-- Crear tabla Alerts
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Alerts' AND xtype='U')
BEGIN
    CREATE TABLE [Alerts] (
        [Id] int NOT NULL IDENTITY,
        [Type] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [Title] nvarchar(max) NULL,
        [Message] nvarchar(max) NULL,
        [PrinterId] int NULL,
        [AcknowledgedBy] nvarchar(max) NULL,
        [AcknowledgedAt] datetime2 NULL,
        [ResolutionNotes] nvarchar(max) NULL,
        [Source] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [Metadata] nvarchar(max) NULL,
        CONSTRAINT [PK_Alerts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Alerts_Printers_PrinterId] FOREIGN KEY ([PrinterId]) REFERENCES [Printers] ([Id])
    );

    -- Crear índices
    CREATE INDEX [IX_Alerts_PrinterId] ON [Alerts] ([PrinterId]);
    CREATE INDEX [IX_Alerts_Status] ON [Alerts] ([Status]);
    CREATE INDEX [IX_Alerts_Type] ON [Alerts] ([Type]);
    CREATE INDEX [IX_Alerts_CreatedAt] ON [Alerts] ([CreatedAt]);

    PRINT 'Tabla Alerts creada exitosamente.';
END
GO

-- Crear tabla Reports
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reports' AND xtype='U')
BEGIN
    CREATE TABLE [Reports] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NULL,
        [Type] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [UserId] nvarchar(450) NULL,
        [Status] nvarchar(max) NULL,
        [FilterParameters] nvarchar(max) NULL,
        [FileUrl] nvarchar(max) NULL,
        [FileSize] bigint NULL,
        [CreatedAt] datetime2 NOT NULL,
        [GeneratedAt] datetime2 NULL,
        [UpdatedAt] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reports_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
    );

    -- Crear índices
    CREATE INDEX [IX_Reports_UserId] ON [Reports] ([UserId]);
    CREATE INDEX [IX_Reports_Type] ON [Reports] ([Type]);
    CREATE INDEX [IX_Reports_Status] ON [Reports] ([Status]);
    CREATE INDEX [IX_Reports_CreatedAt] ON [Reports] ([CreatedAt]);

    PRINT 'Tabla Reports creada exitosamente.';
END
GO

-- Insertar datos iniciales
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [Description]) VALUES
('1', 'Admin', 'ADMIN', 'Administrador del sistema'),
('2', 'Technician', 'TECHNICIAN', 'Técnico de mantenimiento'),
('3', 'User', 'USER', 'Usuario estándar');

PRINT 'Datos iniciales insertados exitosamente.';
PRINT 'Configuración de base de datos completada.';
GO
