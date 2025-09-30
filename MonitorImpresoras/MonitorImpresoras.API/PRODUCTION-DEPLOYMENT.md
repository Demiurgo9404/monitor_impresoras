# 📋 Guía de Despliegue en Producción - Monitor de Impresoras

## 🚀 Despliegue Automatizado con GitHub Actions

Este proyecto incluye un pipeline CI/CD completo que permite despliegues automáticos a IIS en entorno de producción.

### 📋 Prerrequisitos

1. **Servidor IIS** configurado con:
   - Windows Server 2019/2022
   - IIS 10.0+
   - .NET 8.0 Hosting Bundle instalado
   - Certificado SSL válido

2. **Base de Datos PostgreSQL**:
   - PostgreSQL 15+
   - Usuario con permisos de creación de tablas
   - Configuración de conexión segura

3. **Redis** (opcional pero recomendado):
   - Redis 6.0+
   - Configuración con autenticación y SSL

### 🔧 Configuración Inicial

#### 1. Variables de Entorno en GitHub Secrets

Crear los siguientes secretos en el repositorio:

```bash
# Base de datos de producción
DB_PASSWORD=tu_password_seguro_aqui
DB_HOST=prod-db.monitorimpresoras.com
DB_NAME=MonitorImpresoras_Production

# JWT para producción
JWT_KEY=tu_clave_jwt_segura_minimo_256_bits

# Email para notificaciones
SMTP_USERNAME=alerts@monitorimpresoras.com
SMTP_PASSWORD=tu_password_email_seguro

# Perfil de publicación IIS
IIS_PUBLISH_PROFILE=contenido_del_archivo_publish_profile_xml
```

#### 2. Configuración del Publish Profile

1. En Visual Studio, crear un perfil de publicación para IIS
2. Exportar el perfil como archivo `.publishsettings`
3. Copiar el contenido del archivo a GitHub Secrets como `IIS_PUBLISH_PROFILE`

### 🏗️ Arquitectura de Producción

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Load Balancer │    │      IIS 1      │    │      IIS 2      │
│   (Nginx/HAProxy│────│   Application   │────│   Application   │
│     / Azure LB) │    │     Server      │    │     Server      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │   PostgreSQL    │
                    │   Cluster       │
                    │   (Primary +    │
                    │    Replicas)    │
                    └─────────────────┘
                                 │
                    ┌─────────────────┐
                    │     Redis       │
                    │   Cache Cluster │
                    └─────────────────┘
```

### 🔒 Configuración de Seguridad

#### Headers de Seguridad
El proyecto incluye configuración automática de headers OWASP:
- `Strict-Transport-Security` → HSTS activado
- `X-Content-Type-Options` → Protección contra MIME sniffing
- `X-Frame-Options` → Protección contra clickjacking
- `Referrer-Policy` → Control de referrer
- `Content-Security-Policy` → Protección XSS

#### Rate Limiting
Configuración automática de límites de request:
- API general: 60 requests/minuto
- Login: 10 requests/5 minutos
- Protección contra ataques de fuerza bruta

#### Configuración IIS Endurecida
Archivo `Web.config` incluye:
- Compresión HTTP dinámica y estática
- Límites de tamaño de archivos
- Headers de seguridad adicionales
- Configuración de errores personalizados

### 📊 Monitoreo y Observabilidad

#### Health Checks
Endpoints disponibles:
- `GET /health` → Health check básico
- `GET /health/detailed` → Información completa del sistema
- `GET /health/database` → Estado de la base de datos
- `GET /health/redis` → Estado de Redis
- `GET /health/printers` → Estado de impresoras conectadas

#### Logging
- **Serilog** configurado para producción
- Logs estructurados en formato JSON
- Archivo separado para errores críticos
- Integración opcional con PostgreSQL

#### Métricas
- Métricas automáticas con Prometheus
- Dashboard opcional con Grafana
- Alertas configurables por thresholds

### 🚀 Despliegue Paso a Paso

#### 1. Configuración Inicial del Servidor

```powershell
# Instalar IIS con características necesarias
Install-WindowsFeature -Name Web-Server, Web-Common-Http, Web-Default-Doc, Web-Dir-Browsing, Web-Http-Errors, Web-Static-Content, Web-Health, Web-Http-Logging, Web-Request-Monitor, Web-Http-Tracing, Web-Performance, Web-Stat-Compression, Web-Dynamic-Compression, Web-Mgmt-Tools, Web-Mgmt-Console

# Instalar .NET 8.0 Hosting Bundle
# Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0

# Crear sitio web en IIS
New-IISSite -Name "MonitorImpresoras" -PhysicalPath "C:\inetpub\wwwroot\monitor" -BindingInformation "*:80:" -Protocol http
```

#### 2. Configuración de Base de Datos

```sql
-- Crear base de datos de producción
CREATE DATABASE "MonitorImpresoras_Production"
    WITH OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'es_ES.UTF-8'
    LC_CTYPE = 'es_ES.UTF-8'
    TEMPLATE = template0;

-- Crear usuario de aplicación
CREATE USER monitor_user WITH PASSWORD 'tu_password_seguro';
GRANT ALL PRIVILEGES ON DATABASE "MonitorImpresoras_Production" TO monitor_user;
```

#### 3. Configuración SSL/TLS

```powershell
# Instalar certificado SSL
Import-Certificate -FilePath "C:\certs\monitorimpresoras.crt" -CertStoreLocation "cert:\LocalMachine\My"

# Configurar binding HTTPS en IIS
New-IISSiteBinding -Name "MonitorImpresoras" -BindingInformation "*:443:" -CertificateThumbPrint "THUMBPRINT_DEL_CERTIFICADO" -Protocol https
```

#### 4. Despliegue con GitHub Actions

1. **Hacer push a rama `main`**
2. **GitHub Actions ejecutará automáticamente**:
   - ✅ Restauración de dependencias
   - ✅ Compilación en Release
   - ✅ Ejecución de tests
   - ✅ Publicación de aplicación
   - ✅ Despliegue automático a IIS

### 🔍 Verificación del Despliegue

#### 1. Verificar Aplicación
```bash
# Health check básico
curl https://monitorimpresoras.com/health

# API funcionando
curl https://monitorimpresoras.com/api/printers
```

#### 2. Verificar Seguridad
```bash
# Headers de seguridad
curl -I https://monitorimpresoras.com/

# Rate limiting (debe retornar 429 después de límite)
for i in {1..70}; do curl -w "%{http_code}\n" -s -o /dev/null https://monitorimpresoras.com/api/printers; done
```

#### 3. Verificar Logs
```bash
# Logs de aplicación
Get-Content "C:\inetpub\logs\LogFiles\W3SVC1\u_ex*.log" -Tail 10

# Logs de Serilog
Get-Content "C:\inetpub\wwwroot\monitor\logs\app_log.txt" -Tail 10
```

### 🛠️ Configuración de Mantenimiento

#### Backup Automático
```powershell
# Script de backup diario
$backupScript = @"
# Backup de base de datos
pg_dump -h prod-db.monitorimpresoras.com -U postgres MonitorImpresoras_Production > "C:\backups\monitor_db_$(Get-Date -Format 'yyyy-MM-dd').sql"

# Backup de configuración
Copy-Item "C:\inetpub\wwwroot\monitor\appsettings.Production.json" "C:\backups\config_$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss').json"
"@

$backupScript | Out-File "C:\scripts\daily-backup.ps1"
```

#### Monitoreo Continuo
```powershell
# Script de monitoreo
$monitoringScript = @"
# Verificar servicios críticos
$services = @('IISADMIN', 'W3SVC', 'postgresql-x64-15')
foreach ($service in $services) {
    $status = Get-Service $service
    if ($status.Status -ne 'Running') {
        Send-MailMessage -To "soporte@monitorimpresoras.com" -Subject "Servicio $service caído" -Body "El servicio $service no está corriendo"
    }
}
"@

$monitoringScript | Out-File "C:\scripts\service-monitor.ps1"
```

### 🚨 Resolución de Problemas

#### Problemas Comunes

1. **Error 500 - Internal Server Error**
   ```bash
   # Verificar logs de aplicación
   Get-Content "C:\inetpub\wwwroot\monitor\logs\errors-*.txt" -Tail 20

   # Verificar permisos de carpeta
   icacls "C:\inetpub\wwwroot\monitor"
   ```

2. **Error de Base de Datos**
   ```bash
   # Verificar conexión
   Test-NetConnection -ComputerName prod-db.monitorimpresoras.com -Port 5432

   # Verificar logs de PostgreSQL
   Get-Content "C:\Program Files\PostgreSQL\15\data\log\postgresql-*.log" -Tail 20
   ```

3. **Problemas de Rendimiento**
   ```bash
   # Verificar uso de recursos
   Get-Process w3wp | Select-Object Id, ProcessName, CPU, WorkingSet

   # Verificar memoria disponible
   Get-Counter '\Memory\Available MBytes'
   ```

### 📞 Soporte y Contacto

**Equipo de Soporte Técnico:**
- Email: soporte@monitorimpresoras.com
- Teléfono: +1 (555) 123-4567
- Horario: Lunes a Viernes, 9:00 - 18:00 UTC

**Equipo de Seguridad:**
- Email: seguridad@monitorimpresoras.com
- Respuesta garantizada en < 4 horas para incidentes críticos

---

## ✅ Checklist de Despliegue

- [ ] Variables de entorno configuradas en GitHub Secrets
- [ ] Perfil de publicación creado y subido como secreto
- [ ] Servidor IIS configurado con .NET 8.0 Hosting Bundle
- [ ] Base de datos PostgreSQL creada y accesible
- [ ] Certificado SSL instalado y configurado
- [ ] Pipeline CI/CD ejecutándose correctamente
- [ ] Health checks respondiendo correctamente
- [ ] Headers de seguridad verificados
- [ ] Logs de aplicación funcionando
- [ ] Backup automático configurado
- [ ] Monitoreo de servicios activo

**Estado del Despliegue:** 🔄 En Progreso | ✅ Completado | ❌ Error
