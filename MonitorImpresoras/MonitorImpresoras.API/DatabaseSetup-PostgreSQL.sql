-- Script SQL para PostgreSQL - Base de datos MonitorImpresoras
-- Ejecutar este script en pgAdmin o desde psql

-- Crear la base de datos si no existe
SELECT 'CREATE DATABASE "MonitorImpresoras"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'MonitorImpresoras')\gexec

-- Conectar a la base de datos
\c MonitorImpresoras;

-- Crear tabla AspNetRoles
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" varchar(450) NOT NULL PRIMARY KEY,
    "Name" varchar(256) NULL,
    "NormalizedName" varchar(256) NULL,
    "ConcurrencyStamp" text NULL,
    "Description" text NULL
);

-- Crear tabla AspNetUsers
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" varchar(450) NOT NULL PRIMARY KEY,
    "UserName" varchar(256) NULL,
    "NormalizedUserName" varchar(256) NULL,
    "Email" varchar(256) NULL,
    "NormalizedEmail" varchar(256) NULL,
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text NULL,
    "SecurityStamp" text NULL,
    "ConcurrencyStamp" text NULL,
    "PhoneNumber" text NULL,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamptz NULL,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "FirstName" varchar(100) NOT NULL,
    "LastName" varchar(100) NOT NULL,
    "Department" text NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "RefreshToken" text NULL,
    "RefreshTokenExpiryTime" timestamptz NULL
);

-- Crear tabla AspNetRoleClaims
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" serial NOT NULL PRIMARY KEY,
    "RoleId" varchar(450) NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

-- Crear tabla AspNetUserClaims
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" serial NOT NULL PRIMARY KEY,
    "UserId" varchar(450) NOT NULL,
    "ClaimType" text NULL,
    "ClaimValue" text NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Crear tabla AspNetUserLogins
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" varchar(450) NOT NULL,
    "ProviderKey" varchar(450) NOT NULL,
    "ProviderDisplayName" text NULL,
    "UserId" varchar(450) NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Crear tabla AspNetUserRoles
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" varchar(450) NOT NULL,
    "RoleId" varchar(450) NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Crear tabla AspNetUserTokens
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" varchar(450) NOT NULL,
    "LoginProvider" varchar(450) NOT NULL,
    "Name" varchar(450) NOT NULL,
    "Value" text NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Crear tabla Printers
CREATE TABLE IF NOT EXISTS "Printers" (
    "Id" serial NOT NULL PRIMARY KEY,
    "Name" varchar(100) NOT NULL,
    "Model" varchar(100) NOT NULL,
    "SerialNumber" varchar(100) NOT NULL,
    "IpAddress" varchar(50) NOT NULL,
    "Location" varchar(200) NULL,
    "Status" varchar(50) NULL DEFAULT 'Unknown',
    "IsOnline" boolean NOT NULL DEFAULT false,
    "IsLocalPrinter" boolean NOT NULL DEFAULT false,
    "CommunityString" varchar(50) NULL DEFAULT 'public',
    "SnmpPort" integer NULL DEFAULT 161,
    "PageCount" integer NULL,
    "LastMaintenance" timestamptz NULL,
    "MaintenanceIntervalDays" integer NULL DEFAULT 90,
    "Notes" varchar(500) NULL,
    "LastError" varchar(1000) NULL,
    "LastChecked" timestamptz NULL,
    "CreatedAt" timestamptz NOT NULL,
    "UpdatedAt" timestamptz NULL,
    "IsActive" boolean NOT NULL DEFAULT true
);

-- Crear índices únicos para Printers
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Printers_IpAddress" ON "Printers" ("IpAddress");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Printers_SerialNumber" ON "Printers" ("SerialNumber");
CREATE INDEX IF NOT EXISTS "IX_Printers_IsOnline" ON "Printers" ("IsOnline");
CREATE INDEX IF NOT EXISTS "IX_Printers_Status" ON "Printers" ("Status");

-- Crear tabla PrinterConsumables
CREATE TABLE IF NOT EXISTS "PrinterConsumables" (
    "Id" serial NOT NULL PRIMARY KEY,
    "PrinterId" integer NOT NULL,
    "Name" varchar(100) NOT NULL,
    "Type" varchar(50) NOT NULL,
    "PartNumber" varchar(50) NULL,
    "MaxCapacity" integer NULL,
    "CurrentLevel" integer NULL,
    "Unit" varchar(20) NULL,
    "WarningLevel" integer NULL,
    "CriticalLevel" integer NULL,
    "LastUpdated" timestamptz NULL,
    "Status" text NULL,
    "RemainingPages" integer NULL,
    "CreatedAt" timestamptz NOT NULL,
    "UpdatedAt" timestamptz NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    CONSTRAINT "FK_PrinterConsumables_Printers_PrinterId" FOREIGN KEY ("PrinterId") REFERENCES "Printers" ("Id") ON DELETE CASCADE
);

-- Crear índices para PrinterConsumables
CREATE INDEX IF NOT EXISTS "IX_PrinterConsumables_PrinterId" ON "PrinterConsumables" ("PrinterId");
CREATE INDEX IF NOT EXISTS "IX_PrinterConsumables_Type" ON "PrinterConsumables" ("Type");
CREATE INDEX IF NOT EXISTS "IX_PrinterConsumables_Status" ON "PrinterConsumables" ("Status");

-- Crear tabla PrintJobs
CREATE TABLE IF NOT EXISTS "PrintJobs" (
    "Id" serial NOT NULL PRIMARY KEY,
    "PrinterId" integer NOT NULL,
    "UserId" varchar(450) NULL,
    "DocumentName" varchar(100) NOT NULL,
    "Pages" integer NOT NULL,
    "IsColor" boolean NOT NULL,
    "IsDuplex" boolean NOT NULL,
    "PrintedAt" timestamptz NOT NULL,
    "JobStatus" varchar(20) NULL,
    "ErrorMessage" varchar(500) NULL,
    "Cost" decimal(18,2) NULL,
    "CreatedAt" timestamptz NOT NULL,
    "UpdatedAt" timestamptz NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    CONSTRAINT "FK_PrintJobs_Printers_PrinterId" FOREIGN KEY ("PrinterId") REFERENCES "Printers" ("Id"),
    CONSTRAINT "FK_PrintJobs_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id")
);

-- Crear índices para PrintJobs
CREATE INDEX IF NOT EXISTS "IX_PrintJobs_PrinterId" ON "PrintJobs" ("PrinterId");
CREATE INDEX IF NOT EXISTS "IX_PrintJobs_UserId" ON "PrintJobs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_PrintJobs_PrintedAt" ON "PrintJobs" ("PrintedAt");

-- Crear tabla Alerts
CREATE TABLE IF NOT EXISTS "Alerts" (
    "Id" serial NOT NULL PRIMARY KEY,
    "Type" text NULL,
    "Status" text NULL,
    "Title" text NULL,
    "Message" text NULL,
    "PrinterId" integer NULL,
    "AcknowledgedBy" text NULL,
    "AcknowledgedAt" timestamptz NULL,
    "ResolutionNotes" text NULL,
    "Source" text NULL,
    "CreatedAt" timestamptz NOT NULL,
    "UpdatedAt" timestamptz NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "Metadata" text NULL,
    CONSTRAINT "FK_Alerts_Printers_PrinterId" FOREIGN KEY ("PrinterId") REFERENCES "Printers" ("Id")
);

-- Crear índices para Alerts
CREATE INDEX IF NOT EXISTS "IX_Alerts_PrinterId" ON "Alerts" ("PrinterId");
CREATE INDEX IF NOT EXISTS "IX_Alerts_Status" ON "Alerts" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Alerts_Type" ON "Alerts" ("Type");
CREATE INDEX IF NOT EXISTS "IX_Alerts_CreatedAt" ON "Alerts" ("CreatedAt");

-- Crear tabla Reports
CREATE TABLE IF NOT EXISTS "Reports" (
    "Id" serial NOT NULL PRIMARY KEY,
    "Title" text NULL,
    "Type" text NULL,
    "Description" text NULL,
    "UserId" varchar(450) NULL,
    "Status" text NULL,
    "FilterParameters" text NULL,
    "FileUrl" text NULL,
    "FileSize" bigint NULL,
    "CreatedAt" timestamptz NOT NULL,
    "GeneratedAt" timestamptz NULL,
    "UpdatedAt" timestamptz NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    CONSTRAINT "FK_Reports_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id")
);

-- Crear índices para Reports
CREATE INDEX IF NOT EXISTS "IX_Reports_UserId" ON "Reports" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Reports_Type" ON "Reports" ("Type");
CREATE INDEX IF NOT EXISTS "IX_Reports_Status" ON "Reports" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Reports_CreatedAt" ON "Reports" ("CreatedAt");

-- Insertar roles iniciales
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "Description") VALUES
('1', 'Admin', 'ADMIN', 'Administrador del sistema'),
('2', 'Technician', 'TECHNICIAN', 'Técnico de mantenimiento'),
('3', 'User', 'USER', 'Usuario estándar')
ON CONFLICT ("Id") DO NOTHING;

-- Crear un usuario admin por defecto (contraseña: Admin123!)
-- Nota: En producción, los usuarios deben crearse a través de la API
INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "FirstName", "LastName", "IsActive") VALUES
('admin-001', 'admin', 'ADMIN', 'admin@monitorimpresoras.com', 'ADMIN@MONITORIMPRESORAS.COM', true, 'AQAAAAIAAYagAAAAEAAAAK2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2Z2', 'ADMIN-SECURITY-STAMP', 'Admin', 'System', true)
ON CONFLICT ("Id") DO NOTHING;

-- Asignar rol Admin al usuario admin
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") VALUES
('admin-001', '1')
ON CONFLICT ("UserId", "RoleId") DO NOTHING;

SELECT 'Configuración de base de datos PostgreSQL completada exitosamente.' as Result;
