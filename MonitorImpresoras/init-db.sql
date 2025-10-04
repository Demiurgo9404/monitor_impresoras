-- Script de inicialización para PostgreSQL
-- Monitor de Impresoras Database

-- Crear extensiones necesarias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Crear usuario de aplicación (si no existe)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'monitor_app') THEN
        CREATE ROLE monitor_app WITH LOGIN PASSWORD 'monitor_app_password';
    END IF;
END
$$;

-- Otorgar permisos
GRANT CONNECT ON DATABASE monitor_impresoras TO monitor_app;
GRANT USAGE ON SCHEMA public TO monitor_app;
GRANT CREATE ON SCHEMA public TO monitor_app;

-- Configurar timezone
SET timezone = 'UTC';

-- Crear tablas base (Entity Framework las creará automáticamente, pero esto es para referencia)
-- Las migraciones de EF Core se encargarán de la estructura real

-- Insertar datos iniciales de ejemplo
-- Estos datos se crearán después de que EF Core ejecute las migraciones

-- Configurar políticas de backup
COMMENT ON DATABASE monitor_impresoras IS 'Base de datos para el sistema Monitor de Impresoras - Versión 1.0.0';

-- Log de inicialización
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('00000000000000_InitialSetup', '8.0.0')
ON CONFLICT DO NOTHING;
