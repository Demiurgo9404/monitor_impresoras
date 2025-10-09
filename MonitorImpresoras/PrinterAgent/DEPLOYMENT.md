# 🚀 Guía de Despliegue - PrinterAgent

## 📋 Resumen Ejecutivo

**PrinterAgent** es un agente distribuido que permite monitorear impresoras desde cualquier ubicación del mundo y transmitir los datos al sistema central de Monitor de Impresoras. Está diseñado para ser desplegado en diferentes sucursales, oficinas o ubicaciones remotas.

## 🎯 Casos de Uso

### ✅ Escenarios Ideales
- **Sucursales remotas** con impresoras locales
- **Oficinas distribuidas** geográficamente
- **Centros de datos** con múltiples impresoras
- **Empresas multinacionales** con presencia global
- **Proveedores de servicios** que gestionan impresoras de clientes

### 🌐 Arquitectura de Despliegue

```
┌─────────────────────────────────────────────────────────────────┐
│                    SISTEMA CENTRAL                              │
│              Monitor de Impresoras                             │
│                http://localhost:5278                           │
└─────────────────────┬───────────────────────────────────────────┘
                      │ Internet / VPN
          ┌───────────┼───────────┐
          │           │           │
    ┌─────▼─────┐ ┌───▼────┐ ┌────▼────┐
    │ Agente A  │ │Agente B│ │Agente C │
    │ Sucursal  │ │ Centro │ │ Oficina │
    │ Norte     │ │ Datos  │ │ Remota  │
    └───────────┘ └────────┘ └─────────┘
         │            │          │
    ┌────▼────┐  ┌────▼────┐ ┌───▼────┐
    │Printer 1│  │Printer 3│ │Printer │
    │Printer 2│  │Printer 4│ │   5    │
    └─────────┘  └─────────┘ └────────┘
```

## 🛠️ Instalación Rápida

### Opción 1: Script Automatizado (Recomendado)

```powershell
# Ejecutar como Administrador
.\install-agent.ps1 -CentralApiUrl "http://tu-servidor:5278/api" -ApiKey "tu-api-key-segura"
```

### Opción 2: Instalación Manual

```powershell
# 1. Compilar el agente
dotnet publish PrinterAgent.API --configuration Release --output C:\PrinterAgent

# 2. Configurar appsettings.Production.json
# 3. Instalar como servicio de Windows
sc create PrinterAgent binPath="C:\PrinterAgent\PrinterAgent.API.exe --environment=Production"

# 4. Iniciar servicio
Start-Service PrinterAgent
```

## ⚙️ Configuración

### Configuración Básica (appsettings.json)

```json
{
  "Agent": {
    "AgentId": "agent-sucursal-001",
    "AgentName": "Agente Sucursal Norte",
    "Location": "Ciudad de México, México",
    "CentralApiUrl": "http://monitor-central.empresa.com:5278/api",
    "ApiKey": "tu-api-key-super-segura",
    "ReportingInterval": "00:05:00",
    "HealthCheckInterval": "00:01:00",
    "Network": {
      "ScanRanges": [
        "192.168.1.0/24",
        "10.0.0.0/24"
      ],
      "SnmpCommunity": "public",
      "SnmpTimeout": 5000,
      "MaxConcurrentScans": 10,
      "EnableAutoDiscovery": true
    }
  }
}
```

### Variables de Entorno (Alternativa)

```bash
AGENT__AGENTID=agent-001
AGENT__CENTRALAPI=http://central-server:5278/api
AGENT__APIKEY=your-secure-key
AGENT__NETWORK__SCANRANGES__0=192.168.1.0/24
```

## 🔐 Seguridad

### Generación de API Keys

```powershell
# En el sistema central, generar API key para el agente
$apiKey = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("agent-$(Get-Random)-$(Get-Date -Format 'yyyyMMdd')"))
Write-Host "API Key generada: $apiKey"
```

### Configuración de Firewall

```powershell
# Permitir tráfico saliente HTTPS (puerto 443)
New-NetFirewallRule -DisplayName "PrinterAgent Outbound" -Direction Outbound -Protocol TCP -RemotePort 443,5278 -Action Allow

# Permitir acceso local al dashboard (opcional)
New-NetFirewallRule -DisplayName "PrinterAgent Dashboard" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
```

## 📊 Monitoreo y Operación

### URLs de Acceso

| Servicio | URL | Descripción |
|----------|-----|-------------|
| Dashboard | `http://localhost:5000` | Interfaz web del agente |
| API | `http://localhost:5000/api/agent` | API REST del agente |
| Health Check | `http://localhost:5000/health` | Estado de salud |
| Swagger | `http://localhost:5000/swagger` | Documentación API |

### Comandos de Gestión

```powershell
# Ver estado del servicio
Get-Service PrinterAgent

# Iniciar/Detener servicio
Start-Service PrinterAgent
Stop-Service PrinterAgent

# Ver logs en tiempo real
Get-Content "C:\PrinterAgent\logs\agent-*.txt" -Wait -Tail 50

# Forzar escaneo de red
Invoke-RestMethod -Uri "http://localhost:5000/api/agent/scan" -Method POST

# Obtener estado de salud
Invoke-RestMethod -Uri "http://localhost:5000/api/agent/health" -Method GET
```

## 🔧 Troubleshooting

### Problemas Comunes

#### 1. Agente no se conecta al sistema central

```powershell
# Verificar conectividad
Test-NetConnection -ComputerName "tu-servidor-central" -Port 5278

# Verificar configuración
Get-Content "C:\PrinterAgent\appsettings.Production.json"

# Ver logs de conexión
Get-Content "C:\PrinterAgent\logs\agent-*.txt" | Select-String "Central"
```

#### 2. No detecta impresoras

```powershell
# Verificar rangos de red
Get-Content "C:\PrinterAgent\appsettings.Production.json" | Select-String "ScanRanges"

# Probar conectividad a impresoras
Test-NetConnection -ComputerName "192.168.1.100" -Port 9100

# Forzar escaneo manual
Invoke-RestMethod -Uri "http://localhost:5000/api/agent/scan" -Method POST
```

#### 3. Servicio no inicia

```powershell
# Ver eventos del sistema
Get-EventLog -LogName Application -Source "PrinterAgent" -Newest 10

# Ejecutar manualmente para debug
cd C:\PrinterAgent
.\PrinterAgent.API.exe --environment=Production

# Verificar dependencias .NET
dotnet --version
```

## 📈 Escalabilidad

### Múltiples Agentes

```json
{
  "Agents": [
    {
      "Id": "agent-mx-001",
      "Name": "México - Sucursal Norte",
      "Location": "Ciudad de México"
    },
    {
      "Id": "agent-us-001", 
      "Name": "USA - Dallas Office",
      "Location": "Dallas, Texas"
    },
    {
      "Id": "agent-br-001",
      "Name": "Brasil - São Paulo",
      "Location": "São Paulo, Brasil"
    }
  ]
}
```

### Balanceador de Carga

```nginx
# nginx.conf para múltiples sistemas centrales
upstream printer_monitor {
    server central1.empresa.com:5278;
    server central2.empresa.com:5278;
    server central3.empresa.com:5278;
}

server {
    listen 443 ssl;
    server_name monitor.empresa.com;
    
    location /api {
        proxy_pass http://printer_monitor;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

## 🔄 Actualizaciones

### Actualización Automática

```powershell
# Script de actualización
$latestVersion = Invoke-RestMethod -Uri "https://api.github.com/repos/empresa/printer-agent/releases/latest"
$downloadUrl = $latestVersion.assets[0].browser_download_url

# Descargar nueva versión
Invoke-WebRequest -Uri $downloadUrl -OutFile "PrinterAgent-Update.zip"

# Detener servicio, actualizar, reiniciar
Stop-Service PrinterAgent
Expand-Archive "PrinterAgent-Update.zip" -DestinationPath "C:\PrinterAgent" -Force
Start-Service PrinterAgent
```

### Actualización Manual

```powershell
# 1. Hacer backup de configuración
Copy-Item "C:\PrinterAgent\appsettings.Production.json" "C:\PrinterAgent\appsettings.backup.json"

# 2. Detener servicio
Stop-Service PrinterAgent

# 3. Reemplazar archivos (excepto configuración)
# 4. Restaurar configuración si es necesario
# 5. Reiniciar servicio
Start-Service PrinterAgent
```

## 📞 Soporte

### Información de Diagnóstico

```powershell
# Generar reporte de diagnóstico
$diagnostics = @{
    "AgentVersion" = (Get-Item "C:\PrinterAgent\PrinterAgent.API.exe").VersionInfo.FileVersion
    "ServiceStatus" = (Get-Service PrinterAgent).Status
    "LastRestart" = (Get-Service PrinterAgent).StartTime
    "ConfigFile" = Test-Path "C:\PrinterAgent\appsettings.Production.json"
    "LogFiles" = (Get-ChildItem "C:\PrinterAgent\logs\*.txt" | Measure-Object).Count
    "NetworkRanges" = (Get-Content "C:\PrinterAgent\appsettings.Production.json" | ConvertFrom-Json).Agent.Network.ScanRanges
}

$diagnostics | ConvertTo-Json -Depth 3
```

### Contacto de Soporte

- **Email**: soporte@monitor-impresoras.com
- **Portal**: https://soporte.monitor-impresoras.com
- **Documentación**: https://docs.monitor-impresoras.com
- **GitHub**: https://github.com/empresa/printer-agent

---

**Versión**: 1.0.0  
**Fecha**: Octubre 2025  
**Autor**: Equipo Monitor de Impresoras

