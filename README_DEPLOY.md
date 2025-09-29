# 📋 Guía de Despliegue - Monitor Impresoras

Esta guía proporciona instrucciones detalladas para desplegar Monitor Impresoras en un entorno de producción usando Windows Server + IIS.

## 📋 Tabla de Contenidos

- [Requisitos Previos](#requisitos-previos)
- [Configuración del Entorno](#configuración-del-entorno)
- [Despliegue Manual](#despliegue-manual)
- [Despliegue Automatizado](#despliegue-automatizado)
- [Configuración de Variables de Entorno](#configuración-de-variables-de-entorno)
- [Verificación Post-Despliegue](#verificación-post-despliegue)
- [Rollback](#rollback)
- [Monitoreo](#monitoreo)
- [Solución de Problemas](#solución-de-problemas)

## 🚀 Requisitos Previos

### Servidor Windows
- **Sistema Operativo**: Windows Server 2019 o superior
- **IIS Instalado**: Con módulos ASP.NET Core y URL Rewrite
- **.NET 9 Hosting Bundle**: Instalado y configurado
- **Memoria RAM**: Mínimo 4GB (recomendado 8GB)
- **Espacio en Disco**: 10GB disponibles

### Base de Datos
- **PostgreSQL 13+**: Instalado y accesible desde el servidor
- **Usuario con permisos**: Crear base de datos y ejecutar migraciones
- **Conexión segura**: Configurar SSL si es necesario

### Red y Seguridad
- **Firewall**: Puerto 80 (HTTP) y 443 (HTTPS) abiertos
- **Certificado SSL**: Para producción (opcional para desarrollo)
- **DNS**: Dominio apuntando al servidor

## 🔧 Configuración del Entorno

### 1. Instalación de IIS y ASP.NET Core

```powershell
# Instalar IIS con módulos necesarios
Install-WindowsFeature -Name Web-Server, Web-Common-Http, Web-Default-Doc, Web-Dir-Browsing, Web-Http-Errors, Web-Static-Content, Web-Health, Web-Http-Logging, Web-Request-Monitor, Web-Http-Tracing, Web-Performance, Web-Stat-Compression, Web-Dyn-Compression, Web-Security, Web-Filtering, Web-App-Dev, Web-Net-Ext, Web-Net-Ext45, Web-ASP, Web-ASPNET, Web-ASPNET45, Web-ISAPI-Ext, Web-ISAPI-Filter, Web-Mgmt-Tools, Web-Mgmt-Console, Web-Mgmt-Compat, Web-Metabase, Web-WMI, Web-Lgcy-Scripting, Web-Lgcy-Mgmt-Console

# Instalar .NET 9 Hosting Bundle
# Descargar desde: https://dotnet.microsoft.com/download/dotnet/9.0
# Ejecutar el instalador con permisos de administrador
```

### 2. Configuración de PostgreSQL

```sql
-- Crear base de datos para producción
CREATE DATABASE "MonitorImpresoras_Production"
    WITH OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'es_ES.UTF-8'
    LC_CTYPE = 'es_ES.UTF-8'
    TEMPLATE = template0;

-- Crear usuario de aplicación
CREATE USER monitorapp WITH PASSWORD 'tu_password_seguro_aqui';
GRANT ALL PRIVILEGES ON DATABASE "MonitorImpresoras_Production" TO monitorapp;

-- Configurar conexión remota si es necesario
-- Editar pg_hba.conf y postgresql.conf según necesites
```

## 🚀 Despliegue Manual

### Paso 1: Compilación de la Aplicación

```bash
# En tu máquina de desarrollo
git clone https://github.com/tuusuario/monitor_impresoras.git
cd monitor_impresoras/MonitorImpresoras

# Compilar aplicación
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release

# Publicar aplicación
dotnet publish MonitorImpresoras.API/MonitorImpresoras.API.csproj --configuration Release --output ./publish
```

### Paso 2: Transferencia de Archivos

```powershell
# Desde tu máquina local (Linux/Mac) o usando herramientas como WinSCP
# Copiar el contenido de ./publish al servidor

# Ejemplo con scp (desde Linux/Mac)
scp -r ./publish/* usuario@servidor:/ruta/temporal/
```

### Paso 3: Despliegue en IIS

```powershell
# Ejecutar como Administrador en el servidor

# 1. Crear directorio de aplicación
New-Item -ItemType Directory -Path "C:\inetpub\MonitorImpresoras" -Force

# 2. Copiar archivos
Copy-Item "C:\temp\publish\*" "C:\inetpub\MonitorImpresoras" -Recurse -Force

# 3. Crear Application Pool
Import-Module WebAdministration
New-IISAppPool -Name "MonitorImpresorasPool"
Set-ItemProperty "IIS:\AppPools\MonitorImpresorasPool" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\MonitorImpresorasPool" -Name "managedPipelineMode" -Value 0
Set-ItemProperty "IIS:\AppPools\MonitorImpresorasPool" -Name "processModel.identityType" -Value 2

# 4. Crear sitio web
New-IISSite -Name "MonitorImpresoras" -PhysicalPath "C:\inetpub\MonitorImpresoras" -BindingInformation "*:80:" -ApplicationPool "MonitorImpresorasPool"

# 5. Configurar permisos NTFS
$acl = Get-Acl "C:\inetpub\MonitorImpresoras"
$networkServiceRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "NETWORK SERVICE", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$iisIusrsRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "IIS_IUSRS", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$acl.SetAccessRule($networkServiceRule)
$acl.SetAccessRule($iisIusrsRule)
Set-Acl "C:\inetpub\MonitorImpresoras" $acl

# 6. Ejecutar migraciones
cd "C:\inetpub\MonitorImpresoras"
dotnet ef database update --context ApplicationDbContext --connection-string "tu_connection_string"

# 7. Reiniciar Application Pool
Restart-WebAppPool -Name "MonitorImpresorasPool"

# 8. Verificar funcionamiento
Invoke-WebRequest -Uri "http://localhost/api/v1/health" -UseBasicParsing
```

## 🤖 Despliegue Automatizado

### Usando GitHub Actions (Recomendado)

1. **Configurar Secrets en GitHub**:
   ```bash
   PRODUCTION_CONNECTION_STRING=Host=prod-db;Database=MonitorImpresoras_Production;Username=monitorapp;Password=tu_password
   PRODUCTION_HOST=tu-servidor.com
   PRODUCTION_USER=tu_usuario_windows
   PRODUCTION_CREDENTIALS=base64_encoded_credentials
   PRODUCTION_URL=https://api.monitorimpresoras.com
   ```

2. **El pipeline se ejecuta automáticamente** cuando haces push a la rama `main`

3. **Monitorear el despliegue** en la pestaña "Actions" del repositorio

### Script de Despliegue Automatizado

Usa el script incluido `Deploy-MonitorImpresoras.ps1`:

```powershell
# Ejecutar despliegue automatizado
.\Deploy-MonitorImpresoras.ps1 `
    -SourcePath "C:\temp\publish" `
    -DestinationPath "C:\inetpub\MonitorImpresoras" `
    -BackupBeforeDeploy
```

## 🔐 Configuración de Variables de Entorno

### Variables Requeridas por Entorno

#### Development (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MonitorImpresoras_Development;Username=postgres;Password=development123;"
  },
  "JWT": {
    "Key": "DevelopmentSuperSecretKeyThatIsAtLeast32CharactersLong123!"
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "25"
  }
}
```

#### Staging (appsettings.Staging.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=staging-db;Database=MonitorImpresoras_Staging;Username=postgres;Password=${DB_PASSWORD};"
  },
  "JWT": {
    "Key": "${JWT_KEY}"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}"
  }
}
```

#### Production (appsettings.Production.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db;Database=MonitorImpresoras_Production;Username=postgres;Password=${DB_PASSWORD};Pooling=true;"
  },
  "JWT": {
    "Key": "${JWT_KEY}"
  },
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": "587",
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}"
  }
}
```

### Configuración en Windows Server

#### Método 1: Variables de Entorno del Sistema
```powershell
# Configurar variables de entorno
[Environment]::SetEnvironmentVariable("DB_PASSWORD", "tu_password_seguro", "Machine")
[Environment]::SetEnvironmentVariable("JWT_KEY", "tu_jwt_key_seguro", "Machine")
[Environment]::SetEnvironmentVariable("SMTP_USERNAME", "tu_email@empresa.com", "Machine")
[Environment]::SetEnvironmentVariable("SMTP_PASSWORD", "tu_app_password", "Machine")

# Reiniciar IIS para que tome las nuevas variables
iisreset
```

#### Método 2: Archivo de Variables (Más Seguro)
Crear un archivo `C:\inetpub\MonitorImpresoras\environment.txt`:
```
DB_PASSWORD=tu_password_seguro
JWT_KEY=tu_jwt_key_seguro
SMTP_USERNAME=tu_email@empresa.com
SMTP_PASSWORD=tu_app_password
```

## ✅ Verificación Post-Despliegue

### Health Checks Obligatorios

```bash
# 1. Verificar que la aplicación responde
curl -f http://localhost/api/v1/health

# 2. Verificar health extendido
curl -f http://localhost/api/v1/health/extended

# 3. Verificar métricas de Prometheus
curl -f http://localhost/metrics

# 4. Verificar que la API funciona
curl -f http://localhost/api/v1/reports/available

# 5. Verificar logs de Serilog
# Los logs deberían aparecer en la tabla SystemEvents de PostgreSQL
```

### Comandos de Verificación en PowerShell

```powershell
# Health check básico
$response = Invoke-WebRequest -Uri "http://localhost/api/v1/health" -UseBasicParsing
$response.StatusCode  # Debería ser 200

# Estado del sitio en IIS
Import-Module WebAdministration
Get-IISSiteState -Name "MonitorImpresoras"

# Estado del Application Pool
Get-IISAppPool -Name "MonitorImpresorasPool"

# Verificar procesos de .NET
Get-Process -Name "dotnet" | Where-Object {$_.Path -like "*MonitorImpresoras*"}
```

## 🔄 Rollback

### Rollback Automático con Script

```powershell
# Restaurar desde backup específico
.\Rollback-MonitorImpresoras.ps1 `
    -BackupPath "C:\Backups\MonitorImpresoras-20250128-143000" `
    -Force
```

### Rollback Manual Paso a Paso

```powershell
# 1. Detener sitio web
Stop-IISSite -Name "MonitorImpresoras" -Confirm:$false

# 2. Hacer backup de versión actual (opcional)
Copy-Item "C:\inetpub\MonitorImpresoras" "C:\Backups\Current-$(Get-Date -Format 'yyyyMMdd-HHmmss')" -Recurse

# 3. Eliminar versión actual
Remove-Item "C:\inetpub\MonitorImpresoras" -Recurse -Force

# 4. Restaurar desde backup
Copy-Item "C:\Backups\MonitorImpresoras-20250128-143000" "C:\inetpub\MonitorImpresoras" -Recurse

# 5. Configurar permisos
$acl = Get-Acl "C:\inetpub\MonitorImpresoras"
# ... (mismo código de permisos del despliegue)

# 6. Iniciar sitio web
Start-IISSite -Name "MonitorImpresoras"

# 7. Health check
Invoke-WebRequest -Uri "http://localhost/api/v1/health" -UseBasicParsing
```

## 📊 Monitoreo

### Métricas Disponibles

- **Endpoint**: `http://tu-servidor/metrics`
- **Métricas clave**:
  - `api_requests_total` - Total de solicitudes HTTP
  - `api_request_duration_seconds` - Latencia de respuestas
  - `reports_generated_total` - Reportes generados por tipo
  - `active_users` - Usuarios activos en las últimas 24h
  - `system_memory_usage` - Uso de memoria del sistema

### Logs y Auditoría

- **Logs de aplicación**: PostgreSQL tabla `SystemEvents`
- **Logs de IIS**: `%SystemDrive%\inetpub\logs\LogFiles`
- **Event Viewer**: Windows Logs > Application

### Health Checks

```bash
# Health básico (público)
curl http://tu-servidor/api/v1/health

# Health extendido (público)
curl http://tu-servidor/api/v1/health/extended

# Health seguro (requiere autenticación admin)
curl -H "Authorization: Bearer tu_token_admin" http://tu-servidor/api/v1/health/secure

# Kubernetes readiness probe
curl http://tu-servidor/api/v1/health/ready

# Kubernetes liveness probe
curl http://tu-servidor/api/v1/health/live
```

## 🔧 Configuración Adicional

### SSL/HTTPS

```powershell
# 1. Instalar certificado SSL
Import-Certificate -FilePath "C:\cert\monitorimpresoras.crt" -CertStoreLocation "cert:\LocalMachine\My"

# 2. Configurar binding HTTPS en IIS
New-IISSiteBinding -Name "MonitorImpresoras" -BindingInformation "*:443:" -CertificateThumbPrint "tu_thumbprint" -Protocol https

# 3. Configurar redirección HTTP a HTTPS
# Crear regla de URL Rewrite en IIS Manager o web.config
```

### Configuración de Performance

```xml
<!-- web.config en C:\inetpub\MonitorImpresoras -->
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\MonitorImpresoras.API.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
      <environmentVariables>
        <set name="ASPNETCORE_ENVIRONMENT" value="Production" />
        <set name="DB_PASSWORD" value="%DB_PASSWORD%" />
        <set name="JWT_KEY" value="%JWT_KEY%" />
        <set name="SMTP_USERNAME" value="%SMTP_USERNAME%" />
        <set name="SMTP_PASSWORD" value="%SMTP_PASSWORD%" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

## 🚨 Solución de Problemas

### Problema: Aplicación no inicia

```powershell
# Verificar logs de Windows Event Viewer
# Application and Services Logs > Microsoft > Windows > IIS-IISReset

# Verificar permisos
Get-Acl "C:\inetpub\MonitorImpresoras" | Format-List

# Verificar Application Pool
Get-IISAppPool -Name "MonitorImpresorasPool" | Format-List

# Reiniciar servicios
iisreset
```

### Problema: Base de datos no accesible

```powershell
# Probar conexión manualmente
cd "C:\inetpub\MonitorImpresoras"
dotnet ef database update --context ApplicationDbContext --connection-string "tu_connection_string"

# Verificar configuración de conexión
# Editar connection string en appsettings.Production.json
```

### Problema: Health check falla

```bash
# Verificar que la aplicación responde
curl -v http://localhost/api/v1/health

# Verificar logs en la aplicación
# Consultar tabla SystemEvents en PostgreSQL

# Reiniciar Application Pool
Restart-WebAppPool -Name "MonitorImpresorasPool"
```

### Problema: Alto consumo de memoria

```powershell
# Configurar límites en Application Pool
Set-ItemProperty "IIS:\AppPools\MonitorImpresorasPool" -Name "recycling.privateMemoryLimit" -Value 1048576  # 1GB

# Configurar reciclaje periódico
Set-ItemProperty "IIS:\AppPools\MonitorImpresorasPool" -Name "recycling.periodicRestart.time" -Value "00:00:00"
Set-ItemProperty "IIS:\AppPools\MonitorImpresorasPool" -Name "recycling.periodicRestart.requests" -Value 10000
```

## 📞 Soporte

Para problemas o consultas técnicas:

1. **Revisar logs**: `C:\inetpub\logs\LogFiles` y tabla `SystemEvents`
2. **Health checks**: Siempre verificar `/api/v1/health` primero
3. **Equipo técnico**: Contactar al equipo de DevOps
4. **Documentación**: Esta guía y comentarios en el código

## 🚨 **Sistema de Alertas y Notificaciones**

### **Alertas Automáticas Configuradas**
```csharp
✅ Alertas Críticas: Impresoras desconectadas, errores críticos
✅ Alertas de Advertencia: Tóner bajo (< 15%), papel bajo (< 10%)
✅ Alertas Informativas: Reportes diarios, impresoras reconectadas
✅ Canales Múltiples: Email, Slack, Teams, WhatsApp (extensible)
```

### **Configuración de SMTP para Alertas**
```json
{
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": "587",
    "UseSsl": true,
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}",
    "FromName": "Sistema de Alertas - Monitor Impresoras",
    "FromAddress": "alertas@monitorimpresoras.com"
  }
}
```

### **Ejemplo de Alerta por Email**
```html
🚨 CRÍTICA: Impresora Desconectada: HP LaserJet Pro

La impresora 'HP LaserJet Pro' (ID: 123) se ha desconectado del sistema.

Ubicación: Oficina Principal
Modelo: HP LaserJet Pro M404dn
Estado anterior: Online
Hora de desconexión: 2025-01-29 14:30:15

Información Adicional:
- Última impresión: 2025-01-29 14:25:10
- Nivel de tóner: 45%
- Nivel de papel: 78%

Sistema de Monitor de Impresoras
Enviado: 29/01/2025 14:30:15
```

### **Reportes Diarios Automáticos**
```bash
⏰ 8:00 AM - Reporte Diario Consolidado
📊 Contenido del reporte:
✅ Estado general del sistema
✅ Impresoras activas/inactivas
✅ Niveles de consumibles (tóner, papel)
✅ Estadísticas de uso (últimas 24h)
✅ Eventos destacados del día
✅ Estado de servicios críticos
```

### **Jobs Programados con Quartz.NET**
```csharp
✅ DailyReportJob: 0 0 8 * * ? (8:00 AM diario)
✅ PrinterStatusCheckJob: 0 */15 * * * ? (cada 15 minutos)
✅ SystemMetricsCheckJob: 0 */10 * * * ? (cada 10 minutos)
```

### **Configuración de Destinatarios**
```json
{
  "Notifications": {
    "DefaultRecipients": "admin@empresa.com,manager@empresa.com,soporte@empresa.com",
    "Email": {
      "Enabled": true
    },
    "Slack": {
      "Enabled": false,
      "WebhookUrl": "${SLACK_WEBHOOK_URL}"
    }
  }
}
```

### **API de Gestión de Notificaciones**
```bash
# Enviar alerta crítica manual
POST /api/v1/notifications/critical
Authorization: Bearer {admin_token}
{
  "title": "Alerta Manual",
  "message": "Mensaje personalizado",
  "recipients": ["admin@empresa.com"]
}

# Enviar alerta de advertencia
POST /api/v1/notifications/warning
Authorization: Bearer {manager_token}

# Obtener estadísticas de notificaciones
GET /api/v1/notifications/statistics
Authorization: Bearer {admin_token}

# Probar configuración
POST /api/v1/notifications/test
Authorization: Bearer {admin_token}
{
  "channels": ["Email", "Slack"]
}
```

### **Monitoreo de Alertas**
```sql
-- Ver historial de notificaciones en PostgreSQL
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'notification_%'
ORDER BY "TimestampUtc" DESC
LIMIT 50;

-- Estadísticas por severidad
SELECT
  "Severity",
  COUNT(*) as "Total",
  SUM(CASE WHEN "IsSuccess" THEN 1 ELSE 0 END) as "Exitosas",
  SUM(CASE WHEN NOT "IsSuccess" THEN 1 ELSE 0 END) as "Fallidas"
FROM "SystemEvents"
WHERE "EventType" LIKE 'notification_%'
  AND "TimestampUtc" >= NOW() - INTERVAL '30 days'
GROUP BY "Severity";
```

### **Configuración de Producción**
```yaml
# Variables de entorno en servidor IIS
DB_PASSWORD=tu_password_seguro
JWT_KEY=tu_jwt_key_seguro
SMTP_USERNAME=alertas@empresa.com
SMTP_PASSWORD=tu_app_password_seguro
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK
```

### **Logs de Alertas**
```
[14:30:15 INF] ALERT: PRINTER_OFFLINE Printer HP LaserJet Pro (ID: 123)
{
  "Application": "MonitorImpresoras",
  "Environment": "Production",
  "PrinterId": 123,
  "PreviousStatus": "Online",
  "Location": "Office 1"
}

[14:30:16 INF] NOTIFICATION: CRITICAL_SENT Email sent successfully to 3 recipients
{
  "NotificationId": "550e8400-e29b-41d4-a716-446655440000",
  "Channel": "Email",
  "Recipients": ["admin@empresa.com", "manager@empresa.com", "soporte@empresa.com"]
}
```

### **Configuración de Slack (Opcional)**
```json
{
  "Notifications": {
    "Slack": {
      "Enabled": true,
      "WebhookUrl": "${SLACK_WEBHOOK_URL}",
      "Channel": "#alertas",
      "Username": "Sistema de Alertas"
    }
  }
}
```

### **Ejemplo de Mensaje Slack**
```
🚨 *ALERTA CRÍTICA*
Impresora Desconectada: HP LaserJet Pro

La impresora 'HP LaserJet Pro' se ha desconectado del sistema.

📍 Ubicación: Oficina Principal
🔧 Modelo: HP LaserJet Pro M404dn
⏰ Hora: 2025-01-29 14:30:15

*Acción requerida:* Verificar conectividad física y de red.
```

### **Configuración de Teams (Opcional)**
```json
{
  "Notifications": {
    "Teams": {
      "Enabled": true,
      "WebhookUrl": "${TEAMS_WEBHOOK_URL}"
    }
  }
}
```

### **Monitoreo de Estado de Alertas**
```bash
# Verificar que los jobs de alertas están corriendo
curl http://localhost/api/v1/health/extended

# Verificar métricas de notificaciones
curl http://localhost/metrics | grep notification

# Ver logs recientes de alertas
SELECT * FROM "SystemEvents"
WHERE "Category" = 'notifications'
  AND "TimestampUtc" >= NOW() - INTERVAL '1 hour'
ORDER BY "TimestampUtc" DESC;
```

### **Configuración de Umbrales de Alertas**
```csharp
// En AlertService.cs - Configurable por entorno
⚠️ Tóner bajo: < 15% (cada 6 horas máximo)
⚠️ Papel bajo: < 10% (cada 4 horas máximo)
🚨 Impresora desconectada: Inmediata (cada 30 minutos máximo)
🚨 Error crítico: Inmediata (cada 15 minutos máximo)
📊 Reporte diario: 8:00 AM todos los días
```

### **Solución de Problemas de Alertas**
```bash
# 1. Verificar configuración SMTP
curl -X POST http://localhost/api/v1/notifications/test \
  -H "Authorization: Bearer {admin_token}" \
  -d '{"channels": ["Email"]}'

# 2. Verificar logs de errores
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'notification_failed%'
ORDER BY "TimestampUtc" DESC;

# 3. Verificar permisos de Application Pool
Get-Acl "C:\inetpub\MonitorImpresoras" | Format-List

# 4. Probar conectividad SMTP manualmente
# Usar herramienta como smtp4dev para desarrollo
```

### **Configuración de Seguridad**
```csharp
✅ Autorización basada en roles:
  - Alertas críticas: Solo administradores
  - Alertas de advertencia: Admin + Manager
  - Alertas informativas: Todos los usuarios

✅ Auditoría completa:
  - Registro de todas las notificaciones enviadas
  - Seguimiento de éxitos y fallos
  - Información de destinatarios y canales

✅ Configuración segura:
  - Credenciales SMTP en variables de entorno
  - Logs sin información sensible
  - Validación de permisos en cada operación
```

### **Dashboard de Alertas (Futuro)**
```bash
🏗️ Características planificadas:
✅ Lista de alertas recientes con colores
✅ Filtros por severidad, canal, fecha
✅ Posibilidad de marcar como resuelto
✅ Estadísticas visuales de notificaciones
✅ Configuración de reglas de alertas
✅ Historial completo de notificaciones
```

### **Configuración Multi-Canal**
```json
{
  "Notifications": {
    "DefaultRecipients": "admin@empresa.com,manager@empresa.com,soporte@empresa.com",
    "Email": {
      "Enabled": true
    },
    "Slack": {
      "Enabled": true,
      "WebhookUrl": "${SLACK_WEBHOOK_URL}",
      "Channel": "#alertas-ti",
      "Username": "Sistema de Alertas"
    },
    "Teams": {
      "Enabled": true,
      "WebhookUrl": "${TEAMS_WEBHOOK_URL}",
      "Channel": "Alertas TI"
    },
    "WhatsApp": {
      "Enabled": true
    }
  },
  "Twilio": {
    "AccountSid": "${TWILIO_ACCOUNT_SID}",
    "AuthToken": "${TWILIO_AUTH_TOKEN}",
    "FromPhoneNumber": "whatsapp:${TWILIO_PHONE_NUMBER}"
  }
}
```

### **Ejemplo de Mensaje Teams (Adaptive Cards)**
```json
{
  "@type": "MessageCard",
  "@context": "http://schema.org/extensions",
  "themeColor": "FF0000",
  "summary": "Alerta Crítica",
  "sections": [{
    "activityTitle": "🚨 CRÍTICA: Impresora Desconectada",
    "activitySubtitle": "HP LaserJet Pro (ID:123)",
    "facts": [
      {"name": "Ubicación", "value": "Oficina Principal"},
      {"name": "IP", "value": "192.168.1.50"},
      {"name": "Estado", "value": "Offline"}
    ],
    "markdown": true
  }]
}
```

### **Ejemplo de Mensaje Slack (Blocks)**
```json
{
  "blocks": [
    {
      "type": "header",
      "text": {
        "type": "plain_text",
        "text": ":rotating_light: CRÍTICA: Impresora Desconectada"
      }
    },
    {
      "type": "section",
      "text": {
        "type": "mrkdwn",
        "text": "La impresora 'HP LaserJet Pro' se ha desconectado del sistema."
      }
    },
    {
      "type": "section",
      "fields": [
        {
          "type": "mrkdwn",
          "text": "*Impresora:*\nHP LaserJet Pro"
        },
        {
          "type": "mrkdwn",
          "text": "*IP:*\n192.168.1.50"
        }
      ]
    }
  ]
}
```

### **Ejemplo de Mensaje WhatsApp**
```
🚨 CRÍTICA: Impresora Desconectada
📌 Nombre: HP LaserJet Pro
🌐 IP: 192.168.1.50
📍 Ubicación: Oficina Principal
⏰ Hora: 2025-01-30 14:20:00

Sistema Monitor Impresoras
🔗 Marcar como resuelto: [Enlace]
```

### **Sistema de Escalamiento Automático**
```csharp
✅ Nivel 1 (15 min): Notificación inicial a destinatarios principales
✅ Nivel 2 (30 min): Escalamiento a supervisores
✅ Nivel 3 (45 min): Escalamiento final a dirección de TI
✅ Trazabilidad completa en tabla NotificationEscalationHistory
```

### **API de Dashboard de Alertas**
```bash
# Dashboard completo de alertas
GET /api/v1/alerts/dashboard
Authorization: Bearer {admin_token}

# Estadísticas de escalamiento
GET /api/v1/alerts/escalation-statistics
Authorization: Bearer {admin_token}

# Reconocer alerta
PUT /api/v1/alerts/{notificationId}/acknowledge
Authorization: Bearer {user_token}

# Enviar alerta de prueba
POST /api/v1/alerts/test
Authorization: Bearer {admin_token}
```

### **Configuración de Variables de Entorno**
```yaml
# Variables adicionales en servidor IIS
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK
TEAMS_WEBHOOK_URL=https://outlook.office.com/webhook/YOUR/TEAMS/WEBHOOK
TWILIO_ACCOUNT_SID=your_twilio_account_sid
TWILIO_AUTH_TOKEN=your_twilio_auth_token
TWILIO_PHONE_NUMBER=+1234567890
```

### **Monitoreo de Estado Multi-Canal**
```bash
# Verificar estado de todos los canales
curl http://localhost/api/v1/alerts/dashboard | jq '.ChannelStatus'

# Ver estadísticas de escalamiento
curl http://localhost/api/v1/alerts/escalation-statistics

# Probar configuración de canales
curl -X POST http://localhost/api/v1/alerts/test \
  -H "Authorization: Bearer {admin_token}" \
  -d '{"channels": ["Email", "Slack", "Teams", "WhatsApp"]}'
```

### **Logs de Sistema Multi-Canal**
```
[14:30:15 INF] ALERT: PRINTER_OFFLINE Printer HP LaserJet Pro (ID: 123)
[14:30:16 INF] NOTIFICATION: CRITICAL_SENT Email sent to 3 recipients
[14:30:17 INF] NOTIFICATION: CRITICAL_SENT Teams message sent successfully
[14:30:18 INF] NOTIFICATION: CRITICAL_SENT Slack message sent successfully
[14:30:19 INF] NOTIFICATION: CRITICAL_SENT WhatsApp message sent to 2 recipients
[14:30:20 INF] ESCALATION: Tracking started for notification escalation
[14:45:20 INF] ESCALATION: No acknowledgment after 15 minutes, escalating to supervisors
```

### **Configuración de Seguridad Multi-Canal**
```csharp
✅ Autorización granular por canal:
  - Email: Todos los usuarios autenticados
  - Slack/Teams: Manager y superiores
  - WhatsApp: Solo administradores (solo alertas críticas)

✅ Auditoría completa:
  - Registro de todas las notificaciones enviadas
  - Seguimiento de canales utilizados
  - Información de destinatarios por canal
  - Estado de entrega por canal

✅ Configuración segura:
  - Webhooks en variables de entorno
  - Credenciales Twilio en entorno seguro
  - Logs sin información sensible
  - Validación de permisos por operación
```

### **Configuración de Producción**
```yaml
# Configuración completa en appsettings.Production.json
{
  "Notifications": {
    "DefaultRecipients": "admin@empresa.com,manager@empresa.com,soporte@empresa.com",
    "Email": { "Enabled": true },
    "Slack": {
      "Enabled": true,
      "WebhookUrl": "${SLACK_WEBHOOK_URL}",
      "Channel": "#alertas-ti"
    },
    "Teams": {
      "Enabled": true,
      "WebhookUrl": "${TEAMS_WEBHOOK_URL}"
    },
    "WhatsApp": {
      "Enabled": true
    }
  },
  "Twilio": {
    "AccountSid": "${TWILIO_ACCOUNT_SID}",
    "AuthToken": "${TWILIO_AUTH_TOKEN}",
    "FromPhoneNumber": "whatsapp:${TWILIO_PHONE_NUMBER}"
  }
}
```

### **Solución de Problemas Multi-Canal**
```bash
# 1. Verificar configuración de cada canal
curl -X POST http://localhost/api/v1/alerts/test \
  -H "Authorization: Bearer {admin_token}" \
  -d '{"channels": ["Email", "Slack", "Teams"]}'

# 2. Ver logs específicos por canal
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'notification_%'
  AND "Category" = 'teams'  -- o 'slack', 'whatsapp', 'email'
ORDER BY "TimestampUtc" DESC;

# 3. Verificar permisos de Application Pool para llamadas HTTP
Get-NetFirewallRule -DisplayName "Allow HTTP" | Format-List

# 4. Probar conectividad a servicios externos
# Slack: curl -X POST https://hooks.slack.com/services/YOUR/WEBHOOK
# Teams: curl -X POST https://outlook.office.com/webhook/YOUR/WEBHOOK
```

### **Dashboard de Alertas (Características)**
```bash
🏗️ Funcionalidades implementadas:
✅ Resumen de alertas (total, críticas, advertencias, info)
✅ Lista de alertas recientes con colores por severidad
✅ Estado de canales (Email, Slack, Teams, WhatsApp)
✅ Estadísticas de escalamiento
✅ Posibilidad de reconocer alertas
✅ Filtros por canal, severidad y estado
✅ Métricas de tiempo de respuesta promedio
```

### **Configuración de Machine Learning**
```json
{
  "MachineLearning": {
    "ModelPath": "MLModels/MaintenancePredictionModel.zip",
    "TrainingDataRetentionDays": 90,
    "PredictionWindowDays": 30,
    "MinDataPointsForPrediction": 10,
    "ModelRetrainingIntervalDays": 7,
    "PredictionConfidenceThreshold": 0.6
  },
  "Telemetry": {
    "CollectionIntervalMinutes": 5,
    "DataRetentionDays": 30,
    "NormalizationIntervalMinutes": 5,
    "OutlierThreshold": 3,
    "MinDataQualityScore": 70
  }
}
```

### **Ejemplo de Datos de Telemetría**
```json
{
  "PrinterId": 1,
  "TimestampUtc": "2025-01-30T14:30:00Z",
  "TonerLevel": 25,
  "PaperLevel": 15,
  "PagesPrinted": 45,
  "ErrorsCount": 3,
  "Status": "Online",
  "Temperature": 45.5,
  "CpuUsage": 60.2,
  "MemoryUsage": 70.1,
  "JobsInQueue": 2,
  "CollectionMethod": "SNMP",
  "CollectionSuccessful": true,
  "CollectionTimeMs": 150
}
```

### **Ejemplo de Predicción de Mantenimiento**
```json
{
  "Id": 1,
  "PrinterId": 1,
  "PredictedAt": "2025-01-30T14:30:00Z",
  "PredictionType": "TonerDepletion",
  "Probability": 0.85,
  "EstimatedDate": "2025-02-02T14:30:00Z",
  "DaysUntilEvent": 3,
  "Confidence": 0.87,
  "ModelVersion": "1.0.0",
  "RecommendedAction": "Reemplazar tóner en los próximos 2-3 días",
  "RequiresImmediateAttention": true
}
```

### **API de Mantenimiento Predictivo**
```bash
# Resumen de predicciones
GET /api/v1/predictions/summary
Authorization: Bearer {manager_token}

# Predicciones por impresora
GET /api/v1/predictions/{printerId}
Authorization: Bearer {user_token}

# Generar nuevas predicciones
POST /api/v1/predictions/{printerId}/generate
Authorization: Bearer {manager_token}

# Estadísticas de precisión
GET /api/v1/predictions/accuracy
Authorization: Bearer {admin_token}

# Tendencias históricas
GET /api/v1/predictions/trends?days=30
Authorization: Bearer {manager_token}
```

### **Configuración de Variables de Entorno**
```yaml
# Variables adicionales en servidor IIS
ML_MODEL_PATH=C:\inetpub\MonitorImpresoras\MLModels\MaintenancePredictionModel.zip
TELEMETRY_COLLECTION_INTERVAL=5
PREDICTION_WINDOW_DAYS=30
MIN_PREDICTION_CONFIDENCE=0.6
```

### **Monitoreo de Sistema Predictivo**
```bash
# Verificar estado del modelo ML
curl http://localhost/api/v1/health/extended | jq '.predictiveMaintenance'

# Ver métricas de predicciones
curl http://localhost/metrics | grep prediction

# Ver logs de entrenamiento
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'ml_%'
ORDER BY "TimestampUtc" DESC;
```

### **Logs del Sistema Predictivo**
```
[14:30:00 INF] TELEMETRY: Collected metrics for 15 printers in 2.3s
[14:30:05 INF] DATA_CLEANING: Processed 150 records, removed 3 outliers
[14:30:10 INF] ML_PREDICTION: Generated 4 predictions for printer 1
[14:30:15 INF] ML_MODEL: Model trained successfully, R²=0.87, RMSE=2.1
[14:45:00 WRN] PREDICTION: Critical toner depletion predicted for printer 2 in 2 days
```

### **Configuración de Seguridad**
```csharp
✅ Autorización granular para predicciones:
  - Resumen de predicciones: Manager y superiores
  - Predicciones por impresora: Todos los usuarios autenticados
  - Generación de predicciones: Manager y superiores
  - Estadísticas de precisión: Solo administradores

✅ Auditoría completa:
  - Registro de todas las predicciones generadas
  - Seguimiento de precisión de predicciones
  - Información de modelo ML utilizado
  - Logs de entrenamiento y validación

✅ Configuración segura:
  - Modelos ML en directorios seguros
  - Configuración de umbrales en variables de entorno
  - Validación estricta de permisos por operación
```

### **Configuración de Producción**
```yaml
# Configuración completa en appsettings.Production.json
{
  "MachineLearning": {
    "ModelPath": "C:\\inetpub\\MonitorImpresoras\\MLModels\\MaintenancePredictionModel.zip",
    "TrainingDataRetentionDays": 90,
    "PredictionWindowDays": 30,
    "MinDataPointsForPrediction": 10,
    "ModelRetrainingIntervalDays": 7,
    "PredictionConfidenceThreshold": 0.6
  },
  "Telemetry": {
    "CollectionIntervalMinutes": 5,
    "DataRetentionDays": 30,
    "NormalizationIntervalMinutes": 5,
    "OutlierThreshold": 3,
    "MinDataQualityScore": 70
  }
}
```

### **Solución de Problemas Predictivos**
```bash
# 1. Verificar calidad de datos de telemetría
curl http://localhost/api/v1/predictions/accuracy

# 2. Ver logs de errores de predicción
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'ml_prediction_failed%'
ORDER BY "TimestampUtc" DESC;

# 3. Verificar permisos de directorio de modelos
Get-Acl "C:\inetpub\MonitorImpresoras\MLModels" | Format-List

# 4. Reentrenar modelo si es necesario
# Usar endpoint administrativo para reentrenamiento
```

### **Dashboard Predictivo (Características)**
```bash
🏗️ Funcionalidades implementadas:
✅ Resumen ejecutivo de predicciones
✅ Ranking de impresoras más críticas
✅ Gráficas de tendencias históricas
✅ Métricas de precisión del modelo
✅ Vista detallada por impresora
✅ Estadísticas de tipos de fallo
✅ Configuración de umbrales de alerta
```

### **Configuración de Aprendizaje Continuo**
```json
{
  "MachineLearning": {
    "AutoRetrainSchedule": "0 3 * * *",
    "MinFeedbackCount": 20,
    "MinTrainingDataSize": 100,
    "RetrainingThreshold": 0.02,
    "ModelRetentionDays": 30,
    "FeedbackRetentionDays": 90
  },
  "Telemetry": {
    "CollectionIntervalMinutes": 5,
    "DataRetentionDays": 30,
    "NormalizationIntervalMinutes": 5,
    "OutlierThreshold": 3,
    "MinDataQualityScore": 70
  }
}
```

### **Ejemplo de Datos de Feedback**
```json
{
  "Id": 1,
  "PredictionId": 123,
  "IsCorrect": false,
  "Comment": "El fallo ocurrió antes de lo previsto",
  "ProposedCorrection": "La predicción debería haber sido 2 días antes",
  "CreatedBy": "tecnico@empresa.com",
  "CreatedAt": "2025-01-30T10:15:00Z",
  "Quality": "High",
  "IsTrainingData": true,
  "TrainingWeight": 1.5
}
```

### **API de Feedback y Reentrenamiento**
```bash
# Enviar feedback sobre predicción
POST /api/v1/feedback/{predictionId}/feedback
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "isCorrect": false,
  "comment": "El fallo ocurrió antes de lo previsto",
  "proposedCorrection": "La predicción debería haber sido 2 días antes"
}

# Estadísticas avanzadas de precisión
GET /api/v1/feedback/statistics
Authorization: Bearer {manager_token}

# Reentrenamiento manual del modelo
POST /api/v1/feedback/retrain
Authorization: Bearer {admin_token}

# Historial de feedback por predicción
GET /api/v1/feedback/{predictionId}/feedback
Authorization: Bearer {user_token}

# Métricas de calidad del modelo ML
GET /api/v1/feedback/model-quality
Authorization: Bearer {admin_token}
```

### **Configuración de Variables de Entorno**
```yaml
# Variables adicionales en servidor IIS
ML_AUTO_RETRAIN_SCHEDULE=0 3 * * *
ML_MIN_FEEDBACK_COUNT=20
ML_MIN_TRAINING_DATA=100
ML_RETRAINING_THRESHOLD=0.02
TELEMETRY_COLLECTION_INTERVAL=5
```

### **Monitoreo de Aprendizaje Continuo**
```bash
# Verificar estado del modelo ML actual
curl http://localhost/api/v1/feedback/model-quality | jq '.CurrentAccuracy'

# Ver estadísticas de precisión por tipo
curl http://localhost/api/v1/feedback/statistics | jq '.AccuracyByType'

# Ver tendencias de precisión
curl http://localhost/api/v1/feedback/model-quality | jq '.AccuracyTrend'

# Ver logs de reentrenamiento
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'ml_retraining_%'
ORDER BY "TimestampUtc" DESC;
```

### **Logs del Sistema de Aprendizaje Continuo**
```
[03:00:00 INF] ML_RETRAINING: Iniciando reentrenamiento automático
[03:00:05 INF] ML_RETRAINING: Datos entrenamiento: 2500 registros, 152 feedback
[03:00:15 INF] ML_RETRAINING: Modelo entrenado R²=0.89, RMSE=1.8 días
[03:00:20 INF] ML_RETRAINING: Mejora detectada: +2.3%, modelo actualizado
[03:00:25 INF] ML_RETRAINING: Modelo guardado como v1.0.20250130.0300
[14:15:00 INF] ML_FEEDBACK: Procesado feedback negativo para predicción 123
[14:15:05 INF] ML_FEEDBACK: Datos de entrenamiento generados para corrección
```

### **Configuración de Seguridad**
```csharp
✅ Autorización granular para aprendizaje continuo:
  - Enviar feedback: Todos los usuarios autenticados
  - Ver estadísticas: Manager y superiores
  - Reentrenamiento manual: Solo administradores
  - Métricas de calidad: Solo administradores

✅ Auditoría completa:
  - Registro de todo feedback enviado
  - Seguimiento de calidad del feedback
  - Logs de reentrenamiento automático
  - Información de versiones de modelo
  - Métricas de mejora por reentrenamiento

✅ Configuración segura:
  - Jobs programados en configuración segura
  - Modelos ML en directorios protegidos
  - Validación estricta de datos de entrenamiento
  - Protección contra ataques de datos maliciosos
```

### **Configuración de Producción**
```yaml
# Configuración completa en appsettings.Production.json
{
  "MachineLearning": {
    "AutoRetrainSchedule": "0 3 * * *",
    "MinFeedbackCount": 20,
    "MinTrainingDataSize": 100,
    "RetrainingThreshold": 0.02,
    "ModelRetentionDays": 30,
    "FeedbackRetentionDays": 90
  },
  "Telemetry": {
    "CollectionIntervalMinutes": 5,
    "DataRetentionDays": 30,
    "NormalizationIntervalMinutes": 5,
    "OutlierThreshold": 3,
    "MinDataQualityScore": 70
  }
}
```

### **Solución de Problemas de Aprendizaje Continuo**
```bash
# 1. Verificar que el job de reentrenamiento esté ejecutándose
curl http://localhost/api/v1/health/extended | jq '.scheduledJobs.ModelRetraining'

# 2. Ver logs de errores de reentrenamiento
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'ml_retraining_failed%'
ORDER BY "TimestampUtc" DESC;

# 3. Verificar cantidad de datos disponibles para entrenamiento
curl http://localhost/api/v1/feedback/statistics | jq '.TotalFeedback'

# 4. Probar reentrenamiento manual
curl -X POST http://localhost/api/v1/feedback/retrain \
  -H "Authorization: Bearer {admin_token}"
```

### **Dashboard de Aprendizaje Continuo (Características)**
```bash
🏗️ Funcionalidades implementadas:
✅ Estadísticas avanzadas de precisión por tipo
✅ Métricas de calidad del modelo ML
✅ Tendencias históricas de precisión
✅ Información de versiones del modelo
✅ Ranking de tipos de predicción más precisos
✅ Métricas de tiempo de anticipación promedio
✅ Información de feedback por usuario
✅ Configuración de umbrales de reentrenamiento
```

### **Configuración de Rendimiento y Escalabilidad**
```json
{
  "Performance": {
    "EnablePerformanceMonitoring": true,
    "MetricsCollectionIntervalSeconds": 30,
    "SlowQueryThresholdMs": 1000,
    "MaxDatabaseConnections": 100,
    "ConnectionTimeoutSeconds": 30,
    "CommandTimeoutSeconds": 60,
    "EnableDetailedLogging": false
  },
  "Caching": {
    "MemoryCacheExpirationMinutes": 30,
    "DistributedCacheExpirationMinutes": 60,
    "SlidingExpirationMinutes": 15,
    "MaxCacheSizeMB": 100,
    "EnableCacheCompression": true
  },
  "MultiTenant": {
    "EnableTenantIsolation": true,
    "DefaultTenantId": 1,
    "MaxTenantsPerDatabase": 100,
    "TenantDataRetentionDays": 365
  },
  "LoadBalancing": {
    "EnableHealthChecks": true,
    "HealthCheckIntervalSeconds": 30,
    "UnhealthyThreshold": 3,
    "HealthyThreshold": 2
  }
}
```

### **Configuración de Índices de Base de Datos**
```sql
-- Índices compuestos optimizados para consultas frecuentes
CREATE INDEX IX_PrinterTelemetry_Optimized
ON PrinterTelemetry(PrinterId, TimestampUtc DESC, CollectionSuccessful)
WHERE TimestampUtc > NOW() - INTERVAL '30 days';

CREATE INDEX IX_Alerts_Optimized
ON Alerts(PrinterId, Status, Severity, CreatedAt DESC);

CREATE INDEX IX_NotificationEscalationHistory_Optimized
ON NotificationEscalationHistory(NotificationId, EscalationLevel, EscalatedAt);

CREATE INDEX IX_MaintenancePredictions_Optimized
ON MaintenancePredictions(PrinterId, PredictionType, PredictedAt DESC, Probability);

-- Índices parciales para consultas específicas
CREATE INDEX IX_PrinterTelemetry_Recent
ON PrinterTelemetry(TimestampUtc DESC, Status)
WHERE TimestampUtc > NOW() - INTERVAL '7 days';

CREATE INDEX IX_Alerts_Critical
ON Alerts(PrinterId, CreatedAt DESC)
WHERE Severity = 'Critical';
```

### **Configuración de Variables de Entorno**
```yaml
# Variables adicionales en servidor IIS
PERFORMANCE_MONITORING_ENABLED=true
DATABASE_CONNECTION_POOL_SIZE=50
CACHE_MEMORY_EXPIRATION_MINUTES=30
CACHE_DISTRIBUTED_EXPIRATION_MINUTES=60
TENANT_ISOLATION_ENABLED=true
DEFAULT_TENANT_ID=1
```

### **Monitoreo de Rendimiento**
```bash
# Ver métricas actuales del sistema
curl http://localhost/api/v1/performance/metrics

# Ver consultas lentas de BD
curl http://localhost/api/v1/performance/slow-queries

# Ejecutar auditoría completa de rendimiento
curl -X POST http://localhost/api/v1/performance/audit \
  -H "Authorization: Bearer {admin_token}"

# Obtener recomendaciones de optimización
curl http://localhost/api/v1/performance/recommendations \
  -H "Authorization: Bearer {manager_token}"

# Ejecutar pruebas de carga simuladas
curl -X POST http://localhost/api/v1/performance/load-test \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{"printerCount": 1000, "durationMinutes": 5}'
```

### **Logs de Rendimiento**
```
[14:30:00 INF] PERFORMANCE: Auditoría iniciada - Sistema estable
[14:30:05 INF] DB_OPTIMIZATION: Índices creados - Mejora del 35% en consultas
[14:30:10 INF] CACHE: Cache distribuido configurado - 50% menos consultas a BD
[14:30:15 INF] METRICS: Prometheus configurado - Métricas en tiempo real
[14:45:00 WRN] PERFORMANCE: Consulta lenta detectada - 1.2s en telemetry query
[15:00:00 INF] LOAD_TEST: Pruebas completadas - 1000 impresoras simuladas exitosamente
```

### **Configuración de Seguridad**
```csharp
✅ Autorización granular para optimización:
  - Ver métricas: Todos los usuarios autenticados
  - Ejecutar auditoría: Manager y superiores
  - Optimizar consultas: Solo administradores
  - Ejecutar pruebas de carga: Solo administradores

✅ Auditoría completa:
  - Registro de todas las optimizaciones realizadas
  - Seguimiento de cambios en índices de BD
  - Logs de configuración de cache y métricas
  - Información de pruebas de carga ejecutadas

✅ Configuración segura:
  - Límites configurables de recursos
  - Protección contra ataques de denegación de servicio
  - Validación estricta de parámetros de carga
  - Configuración de timeouts seguros
```

### **Configuración de Producción**
```yaml
# Configuración completa en appsettings.Production.json
{
  "Performance": {
    "EnablePerformanceMonitoring": true,
    "MetricsCollectionIntervalSeconds": 30,
    "SlowQueryThresholdMs": 1000,
    "MaxDatabaseConnections": 100,
    "ConnectionTimeoutSeconds": 30,
    "CommandTimeoutSeconds": 60
  },
  "Caching": {
    "MemoryCacheExpirationMinutes": 30,
    "DistributedCacheExpirationMinutes": 60,
    "SlidingExpirationMinutes": 15,
    "MaxCacheSizeMB": 100
  },
  "MultiTenant": {
    "EnableTenantIsolation": true,
    "DefaultTenantId": 1,
    "MaxTenantsPerDatabase": 100
  }
}
```

### **Solución de Problemas de Rendimiento**
```bash
# 1. Verificar estado de health checks
curl http://localhost/health/ready

# 2. Ver métricas de Prometheus
curl http://localhost/metrics | grep -E "(response_time|db_connections|cache)"

# 3. Ver logs de errores de rendimiento
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'performance_%'
ORDER BY "TimestampUtc" DESC;

# 4. Ejecutar optimización automática
curl -X POST http://localhost/api/v1/performance/optimize \
  -H "Authorization: Bearer {admin_token}"
```

### **Dashboard de Rendimiento (Características)**
```bash
🏗️ Funcionalidades implementadas:
✅ Auditoría completa de rendimiento del sistema
✅ Métricas de consultas lentas de BD
✅ Optimización automática de consultas
✅ Recomendaciones de mejora de rendimiento
✅ Métricas actuales del sistema en tiempo real
✅ Reportes de rendimiento históricos
✅ Pruebas de carga simuladas
✅ Configuración de alertas de rendimiento
```

### **Configuración de Seguridad de Infraestructura**
```json
{
  "Security": {
    "EnableSecurityAuditing": true,
    "SecurityAuditIntervalHours": 24,
    "SecurityEventRetentionDays": 90,
    "EnableIntrusionDetection": true,
    "EnableSecurityAlerts": true,
    "SecurityAlertRecipients": ["admin@empresa.com", "security@empresa.com"],
    "MaxFailedLoginAttempts": 5,
    "AccountLockoutDurationMinutes": 30,
    "PasswordMinLength": 12,
    "RequirePasswordComplexity": true,
    "PasswordMaxAgeDays": 90,
    "EnableMfaForAdmins": true,
    "SessionTimeoutMinutes": 30,
    "EnableDetailedSecurityLogging": true
  },
  "WindowsSecurity": {
    "EnableAccountLockout": true,
    "DisableUnnecessaryServices": true,
    "EnableWindowsFirewall": true,
    "EnableWindowsUpdates": true,
    "AuditLoginEvents": true,
    "AuditPrivilegeChanges": true,
    "ServicesToDisable": ["Telnet", "FTP", "SMBv1", "RemoteRegistry"]
  },
  "IisSecurity": {
    "EnableTls13": true,
    "DisableTls11AndBelow": true,
    "EnableSecurityHeaders": true,
    "EnableRequestFiltering": true,
    "EnableRateLimiting": true,
    "EnableDetailedLogging": true,
    "RateLimitRequestsPerMinute": 100,
    "MaxRequestSizeBytes": 10485760
  },
  "PostgreSqlSecurity": {
    "EnableSsl": true,
    "RequireClientCertificates": false,
    "EnableAuditLogging": true,
    "LogMinDurationStatement": 1000,
    "MaxConnections": 100,
    "ConnectionTimeoutSeconds": 30,
    "EnableEncryptedBackups": true,
    "BackupRetentionDays": 30
  }
}
```

### **Configuración de Seguridad de Base de Datos PostgreSQL**
```sql
-- Crear roles separados con permisos mínimos
CREATE ROLE db_admin WITH SUPERUSER CREATEDB CREATEROLE LOGIN PASSWORD 'strong_password_123!';
CREATE ROLE db_app WITH LOGIN PASSWORD 'app_password_456!';
CREATE ROLE db_readonly WITH LOGIN PASSWORD 'readonly_password_789!';

-- Otorgar permisos mínimos
GRANT CONNECT ON DATABASE monitorimpresoras TO db_app;
GRANT USAGE ON SCHEMA public TO db_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO db_app;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO db_readonly;

-- Configuración SSL en postgresql.conf
ssl = on
ssl_cert_file = '/etc/ssl/certs/postgresql.crt'
ssl_key_file = '/etc/ssl/private/postgresql.key'
ssl_ca_file = '/etc/ssl/certs/ca.crt'

-- Configuración pg_hba.conf - solo conexiones SSL
hostssl monitorimpresoras db_app 127.0.0.1/32 scram-sha-256
hostssl monitorimpresoras db_admin 10.0.0.0/8 scram-sha-256
hostssl monitorimpresoras db_readonly 192.168.0.0/16 scram-sha-256
host    all             all     0.0.0.0/0      reject

-- Configuración de auditoría
log_statement = 'ddl'
log_min_duration_statement = 1000
log_connections = on
log_disconnections = on
```

### **Configuración de Seguridad de IIS**
```xml
<!-- web.config - Encabezados de seguridad -->
<httpProtocol>
  <customHeaders>
    <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
    <add name="Content-Security-Policy" value="default-src 'self'; script-src 'self' 'unsafe-inline'" />
    <add name="X-Frame-Options" value="DENY" />
    <add name="X-Content-Type-Options" value="nosniff" />
    <add name="X-XSS-Protection" value="1; mode=block" />
    <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
    <add name="Permissions-Policy" value="geolocation=(), microphone=(), camera=()" />
  </customHeaders>
</httpProtocol>

<!-- web.config - Filtrado de requests -->
<security>
  <requestFiltering>
    <requestLimits maxAllowedContentLength="10485760" />
    <fileExtensions allowUnlisted="true">
      <add fileExtension=".exe" allowed="false" />
      <add fileExtension=".bat" allowed="false" />
      <add fileExtension=".cmd" allowed="false" />
    </fileExtensions>
    <hiddenSegments>
      <add segment="App_Data" />
      <add segment="bin" />
      <add segment="App_Code" />
    </hiddenSegments>
  </requestFiltering>
</security>
```

### **Configuración de Variables de Entorno de Seguridad**
```yaml
# Variables adicionales en servidor IIS
SECURITY_AUDITING_ENABLED=true
MAX_FAILED_LOGIN_ATTEMPTS=5
ACCOUNT_LOCKOUT_DURATION_MINUTES=30
PASSWORD_MIN_LENGTH=12
REQUIRE_PASSWORD_COMPLEXITY=true
SESSION_TIMEOUT_MINUTES=30
ENABLE_MFA_FOR_ADMINS=true
IIS_TLS_VERSION=1.3
POSTGRESQL_SSL_REQUIRED=true
SECURITY_ALERT_EMAILS=admin@empresa.com,security@empresa.com
```

### **Monitoreo de Seguridad**
```bash
# Ejecutar auditoría completa de seguridad
curl -X POST "http://localhost/api/v1/security/audit" \
  -H "Authorization: Bearer {admin_token}"

# Verificar cumplimiento de estándares de seguridad
curl "http://localhost/api/v1/security/compliance" \
  -H "Authorization: Bearer {admin_token}"

# Ejecutar pruebas de penetración internas
curl -X POST "http://localhost/api/v1/security/penetration-test" \
  -H "Authorization: Bearer {admin_token}"

# Obtener recomendaciones de hardening
curl "http://localhost/api/v1/security/recommendations/windows" \
  -H "Authorization: Bearer {manager_token}"

# Configurar seguridad de API
curl -X POST "http://localhost/api/v1/security/harden/api" \
  -H "Authorization: Bearer {admin_token}"
```

### **Logs de Seguridad**
```
[14:30:00 INF] SECURITY_AUDIT: Auditoría iniciada - Sistema seguro
[14:30:05 INF] WINDOWS_HARDENING: Políticas aplicadas - 5 intentos bloqueo
[14:30:10 INF] IIS_HARDENING: TLS 1.3 habilitado - Versiones obsoletas deshabilitadas
[14:30:15 INF] POSTGRESQL_HARDENING: SSL configurado - Conexiones encriptadas
[14:30:20 INF] API_SECURITY: Middleware de seguridad configurado - Protección avanzada
[14:45:00 WRN] SECURITY_EVENT: Intento de login fallido detectado - Usuario bloqueado
[15:00:00 INF] PENETRATION_TEST: Pruebas completadas - 3 vulnerabilidades encontradas
[15:00:05 INF] SECURITY_ALERT: Vulnerabilidades críticas notificadas - Acción requerida
```

### **Configuración de Seguridad**
```csharp
✅ Hardening completo de infraestructura:
  - Windows Server: Políticas de grupo, servicios, firewall, actualizaciones
  - IIS: TLS 1.3, encabezados de seguridad, rate limiting, logging detallado
  - PostgreSQL: Roles mínimos, SSL obligatorio, auditoría, backups encriptados
  - API ASP.NET: Middleware de seguridad, validación estricta, protección contra ataques

✅ Autorización granular para seguridad:
  - Ejecutar auditoría: Solo administradores
  - Ver recomendaciones: Manager y superiores
  - Ejecutar hardening: Solo administradores
  - Ver logs de seguridad: Manager y superiores

✅ Auditoría completa:
  - Registro de todas las configuraciones de seguridad aplicadas
  - Seguimiento de eventos de seguridad críticos
  - Logs de cambios en configuración de infraestructura
  - Información de pruebas de penetración realizadas

✅ Configuración segura:
  - Variables de entorno para credenciales sensibles
  - Configuración de timeouts seguros
  - Límites de recursos configurables
  - Protección contra ataques de denegación de servicio
```

### **Configuración de Producción de Seguridad**
```yaml
# Configuración completa en appsettings.Production.json
{
  "Security": {
    "EnableSecurityAuditing": true,
    "SecurityAuditIntervalHours": 24,
    "SecurityEventRetentionDays": 90,
    "EnableIntrusionDetection": true,
    "EnableSecurityAlerts": true,
    "SecurityAlertRecipients": ["admin@empresa.com", "security@empresa.com"],
    "MaxFailedLoginAttempts": 5,
    "AccountLockoutDurationMinutes": 30,
    "PasswordMinLength": 12,
    "RequirePasswordComplexity": true,
    "PasswordMaxAgeDays": 90,
    "EnableMfaForAdmins": true,
    "SessionTimeoutMinutes": 30
  },
  "WindowsSecurity": {
    "EnableAccountLockout": true,
    "DisableUnnecessaryServices": true,
    "EnableWindowsFirewall": true,
    "EnableWindowsUpdates": true,
    "AuditLoginEvents": true,
    "AuditPrivilegeChanges": true
  },
  "IisSecurity": {
    "EnableTls13": true,
    "DisableTls11AndBelow": true,
    "EnableSecurityHeaders": true,
    "EnableRequestFiltering": true,
    "EnableRateLimiting": true,
    "EnableDetailedLogging": true,
    "RateLimitRequestsPerMinute": 100,
    "MaxRequestSizeBytes": 10485760
  },
  "PostgreSqlSecurity": {
    "EnableSsl": true,
    "RequireClientCertificates": false,
    "EnableAuditLogging": true,
    "LogMinDurationStatement": 1000,
    "MaxConnections": 100,
    "ConnectionTimeoutSeconds": 30,
    "EnableEncryptedBackups": true,
    "BackupRetentionDays": 30
  }
}
```

### **Solución de Problemas de Seguridad**
```bash
# 1. Verificar estado de seguridad actual
curl -X POST "http://localhost/api/v1/security/audit" \
  -H "Authorization: Bearer {admin_token}"

# 2. Ver logs de eventos de seguridad
SELECT * FROM "SystemEvents"
WHERE "EventType" LIKE 'security_%'
ORDER BY "TimestampUtc" DESC;

# 3. Verificar cumplimiento de estándares
curl "http://localhost/api/v1/security/compliance" \
  -H "Authorization: Bearer {admin_token}"

# 4. Ejecutar pruebas de seguridad adicionales
curl -X POST "http://localhost/api/v1/security/penetration-test" \
  -H "Authorization: Bearer {admin_token}"
```

### **Dashboard de Seguridad (Características)**
```bash
🏗️ Funcionalidades implementadas:
✅ Auditoría completa de seguridad del sistema
✅ Hardening automático de Windows Server
✅ Hardening automático de IIS con TLS 1.3
✅ Hardening automático de PostgreSQL con SSL
✅ Configuración avanzada de seguridad de API
✅ Sistema de alertas de seguridad en tiempo real
✅ Pruebas de penetración internas simuladas
✅ Recomendaciones específicas de seguridad
✅ Verificación de cumplimiento de estándares
✅ Configuración de políticas de autorización avanzadas
```

### **Configuración de Observabilidad y Monitoreo**
```json
{
  "Observability": {
    "EnableStructuredLogging": true,
    "EnablePerformanceMetrics": true,
    "EnableHealthChecks": true,
    "EnableAdvancedAlerts": true,
    "EnableRealTimeDashboard": true,
    "LoggingRetentionDays": 90,
    "MetricsRetentionDays": 30,
    "AlertHistoryRetentionDays": 90,
    "EnableElkIntegration": false,
    "EnablePrometheusMetrics": true,
    "EnableGrafanaDashboards": true,
    "HealthCheckIntervalSeconds": 30,
    "MetricsCollectionIntervalSeconds": 15,
    "AlertCooldownMinutes": 15
  },
  "Logging": {
    "JsonFormattingEnabled": true,
    "RequestIdCorrelationEnabled": true,
    "UserIdCorrelationEnabled": true,
    "SessionIdCorrelationEnabled": true,
    "TimestampUtcEnabled": true,
    "MachineNameEnabled": true,
    "EnvironmentNameEnabled": true,
    "ApplicationNameEnabled": true,
    "MinLogLevel": "Information",
    "EnableConsoleLogging": true,
    "EnableFileLogging": true,
    "LogFilePath": "C:\\Logs\\MonitorImpresoras",
    "MaxFileSizeMB": 50,
    "MaxFilesToRetain": 10
  },
  "Metrics": {
    "EnableSystemMetrics": true,
    "EnableApplicationMetrics": true,
    "EnableDatabaseMetrics": true,
    "EnableAiMetrics": true,
    "EnableJobMetrics": true,
    "MetricsEndpoint": "/metrics",
    "MetricsPort": 9090,
    "EnableMetricsEndpoint": true,
    "MetricsLabels": ["environment", "service", "version"],
    "CustomMetricsEnabled": true
  },
  "HealthChecks": {
    "EnableBasicHealthChecks": true,
    "EnableInfrastructureChecks": true,
    "EnableApplicationChecks": true,
    "EnableAiChecks": true,
    "EnableDatabaseChecks": true,
    "EnableNetworkChecks": true,
    "EnableSecurityChecks": true,
    "HealthCheckEndpoint": "/health",
    "DetailedHealthCheckEndpoint": "/health/detailed",
    "HealthCheckTimeoutSeconds": 30,
    "UnhealthyThreshold": 3,
    "HealthyThreshold": 2
  },
  "Alerts": {
    "EnableDynamicAlerts": true,
    "EnableMultiChannelNotifications": true,
    "EnableEscalationPolicies": true,
    "AlertCooldownMinutes": 15,
    "MaxAlertsPerHour": 100,
    "EmailNotificationsEnabled": true,
    "TeamsNotificationsEnabled": true,
    "SmsNotificationsEnabled": false,
    "WebhookNotificationsEnabled": true,
    "AlertRetentionDays": 90,
    "FalsePositiveThreshold": 0.05
  }
}
```

### **Configuración de Logging Estructurado**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "MonitorImpresoras": "Information"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss.fff",
        "UseUtcTimestamp": true
      }
    },
    "File": {
      "Path": "C:\\Logs\\MonitorImpresoras\\app-{Date}.log",
      "Append": true,
      "MaxSize": 52428800,
      "MaxFiles": 10,
      "FormatterName": "json"
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "C:\\Logs\\MonitorImpresoras\\structured-{Date}.json",
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog.Formatting.Json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId", "WithProcessId"],
    "Properties": {
      "Application": "MonitorImpresoras.API",
      "ServiceVersion": "1.0.0"
    }
  }
}
```

### **Configuración de Variables de Entorno de Observabilidad**
```yaml
# Variables adicionales en servidor IIS
OBSERVABILITY_ENABLED=true
LOGGING_RETENTION_DAYS=90
METRICS_RETENTION_DAYS=30
ALERT_HISTORY_RETENTION_DAYS=90
ELK_INTEGRATION_ENABLED=false
PROMETHEUS_METRICS_ENABLED=true
GRAFANA_DASHBOARDS_ENABLED=true
HEALTH_CHECK_INTERVAL_SECONDS=30
METRICS_COLLECTION_INTERVAL_SECONDS=15
ALERT_COOLDOWN_MINUTES=15
LOG_LEVEL=Information
LOG_FILE_PATH=C:\Logs\MonitorImpresoras
MAX_LOG_FILE_SIZE_MB=50
```

### **Monitoreo de Observabilidad**
```bash
# Obtener métricas actuales del sistema
curl "http://localhost/api/v1/observability/metrics/current" \
  -H "Authorization: Bearer {user_token}"

# Obtener métricas históricas (últimas 24 horas)
curl "http://localhost/api/v1/observability/metrics/history?fromDate=2024-01-01T00:00:00&toDate=2024-01-02T00:00:00" \
  -H "Authorization: Bearer {manager_token}"

# Ejecutar health checks extendidos
curl -X POST "http://localhost/api/v1/observability/health/extended" \
  -H "Authorization: Bearer {manager_token}"

# Registrar evento personalizado en logs
curl -X POST "http://localhost/api/v1/observability/log/event" \
  -H "Authorization: Bearer {user_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "custom_business_event",
    "message": "Evento de negocio personalizado",
    "level": "Info",
    "additionalData": {"key": "value"}
  }'

# Crear alerta dinámica de impresora
curl -X POST "http://localhost/api/v1/observability/alerts/printer" \
  -H "Authorization: Bearer {manager_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "printerId": 1,
    "isOnline": false,
    "failureProbability": 0.90,
    "temperature": 65,
    "tonerLevel": 0.05,
    "paperLevel": 0.10,
    "lastSeenMinutes": 20
  }'

# Procesar evento de alerta
curl -X POST "http://localhost/api/v1/observability/alerts/process" \
  -H "Authorization: Bearer {manager_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "HighFailureProbability",
    "severity": "Critical",
    "description": "Probabilidad de fallo crítica detectada",
    "source": "PrinterMonitoring"
  }'

# Obtener historial de alertas
curl "http://localhost/api/v1/observability/alerts/history?fromDate=2024-01-01&toDate=2024-01-02&severity=Critical" \
  -H "Authorization: Bearer {manager_token}"

# Configurar reglas de alertas dinámicas
curl -X POST "http://localhost/api/v1/observability/alerts/configure" \
  -H "Authorization: Bearer {admin_token}"

# Enviar notificación de prueba
curl -X POST "http://localhost/api/v1/observability/alerts/test" \
  -H "Authorization: Bearer {admin_token}"

# Configurar logging estructurado
curl -X POST "http://localhost/api/v1/observability/logging/configure" \
  -H "Authorization: Bearer {admin_token}"

# Obtener métricas de dashboard en tiempo real
curl "http://localhost/api/v1/observability/dashboard/realtime" \
  -H "Authorization: Bearer {user_token}"
```

### **Logs de Observabilidad**
```
[14:30:00 INF] OBSERVABILITY: Sistema de observabilidad iniciado - Logging estructurado activo
[14:30:05 INF] METRICS: Métricas de rendimiento configuradas - Colección cada 15 segundos
[14:30:10 INF] HEALTH_CHECKS: Health checks extendidos habilitados - 35 checks configurados
[14:30:15 INF] ALERTS: Sistema de alertas dinámicas activado - 5 reglas configuradas
[14:45:00 WRN] SLOW_OPERATION: Consulta lenta detectada - 2.1s en operación SELECT
[15:00:00 INF] HEALTH_CHECK: Sistema saludable - Todos los checks pasaron
[15:15:00 WRN] HIGH_MEMORY: Uso de memoria alto detectado - 87% utilizado
[15:30:00 INF] ALERT_SENT: Alerta crítica enviada - Probabilidad de fallo 92%
```

### **Configuración de Observabilidad**
```csharp
✅ Sistema completo de observabilidad implementado:
  - Logging centralizado: Formato JSON estructurado con correlación automática
  - Métricas completas: Sistema, aplicación, BD, IA y jobs monitoreados
  - Health checks extendidos: 35 checks en 7 categorías diferentes
  - Alertas avanzadas: Dinámicas, multi-canal, con reglas configurables
  - Dashboard interactivo: Métricas en tiempo real para operación 24/7

✅ Autorización granular para observabilidad:
  - Ver métricas básicas: Todos los usuarios autenticados
  - Ver métricas históricas: Manager y superiores
  - Ejecutar health checks: Manager y superiores
  - Configurar alertas: Solo administradores
  - Ver logs detallados: Manager y superiores

✅ Auditoría completa:
  - Registro de todas las operaciones de observabilidad
  - Seguimiento de métricas críticas y alertas generadas
  - Logs de configuración de monitoreo aplicada
  - Información de pruebas de observabilidad ejecutadas

✅ Configuración segura:
  - Límites configurables de retención de logs y métricas
  - Protección contra exposición de datos sensibles
  - Configuración de umbrales de alertas seguros
  - Validación estricta de parámetros de observabilidad
```

### **Configuración de Producción de Observabilidad**
```yaml
# Configuración completa en appsettings.Production.json
{
  "Observability": {
    "EnableStructuredLogging": true,
    "EnablePerformanceMetrics": true,
    "EnableHealthChecks": true,
    "EnableAdvancedAlerts": true,
    "EnableRealTimeDashboard": true,
    "LoggingRetentionDays": 90,
    "MetricsRetentionDays": 30,
    "AlertHistoryRetentionDays": 90,
    "EnableElkIntegration": false,
    "EnablePrometheusMetrics": true,
    "EnableGrafanaDashboards": true,
    "HealthCheckIntervalSeconds": 30,
    "MetricsCollectionIntervalSeconds": 15,
    "AlertCooldownMinutes": 15
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "C:\\Logs\\MonitorImpresoras\\structured-{Date}.json",
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog.Formatting.Json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90
        }
      }
    ]
  }
}
```

### **Solución de Problemas de Observabilidad**
```bash
# 1. Verificar estado de health checks
curl -X POST "http://localhost/api/v1/observability/health/extended" \
  -H "Authorization: Bearer {manager_token}"

# 2. Ver métricas actuales del sistema
curl "http://localhost/api/v1/observability/metrics/current" \
  -H "Authorization: Bearer {user_token}"

# 3. Ver logs recientes de errores
curl "http://localhost/api/v1/observability/dashboard/realtime" \
  -H "Authorization: Bearer {user_token}"

# 4. Enviar notificación de prueba
curl -X POST "http://localhost/api/v1/observability/alerts/test" \
  -H "Authorization: Bearer {admin_token}"

# 5. Ver historial de alertas críticas
curl "http://localhost/api/v1/observability/alerts/history?fromDate=2024-01-01&toDate=2024-01-02&severity=Critical" \
  -H "Authorization: Bearer {manager_token}"
```

### **Dashboard de Observabilidad (Características)**
```bash
🏗️ Funcionalidades implementadas:
✅ Logging centralizado estructurado con formato JSON
✅ Métricas completas de rendimiento (sistema, aplicación, BD, IA)
✅ Health checks extendidos en 7 categorías diferentes
✅ Alertas dinámicas multi-canal con reglas configurables
✅ Dashboard interactivo con métricas en tiempo real
✅ Sistema de correlación automática de eventos
✅ Configuración avanzada de retención de datos
✅ API completa para gestión de observabilidad
✅ Pruebas automatizadas de sistema de monitoreo
✅ Configuración segura y auditada de observabilidad
```

**¡Sistema de observabilidad y monitoreo integral completamente implementado!** 📊🔍
