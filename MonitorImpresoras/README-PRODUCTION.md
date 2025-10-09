# 🖨️ Monitor de Impresoras - Despliegue en Producción

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue.svg)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🎯 **Sistema Completo Listo para Producción**

Monitor de Impresoras es una solución empresarial completa para el monitoreo distribuido de impresoras en tiempo real. Incluye:

- ✅ **Backend API** con arquitectura Clean Architecture
- ✅ **Frontend Web** responsive y moderno
- ✅ **PrinterAgent** distribuido para monitoreo remoto
- ✅ **Despliegue automatizado** con Docker
- ✅ **SSL/HTTPS** configurado automáticamente
- ✅ **Base de datos PostgreSQL** en producción
- ✅ **Monitoreo y alertas** en tiempo real

## 🚀 **Despliegue Inmediato**

### **Opción 1: Despliegue Automático (Recomendado)**

```bash
# 1. Conectar al VPS
ssh root@tu-servidor

# 2. Descargar y ejecutar
curl -sSL https://raw.githubusercontent.com/tu-usuario/monitor-impresoras/main/deploy.sh | bash
```

### **Opción 2: Despliegue Manual**

```bash
# 1. Clonar repositorio
git clone https://github.com/tu-usuario/monitor-impresoras.git
cd monitor-impresoras/QOPIQ

# 2. Configurar variables
cp .env.example .env
nano .env  # Editar configuración

# 3. Desplegar
chmod +x deploy.sh
./deploy.sh production
```

## 🌐 **Arquitectura del Sistema**

```
┌─────────────────────────────────────────────────────────────┐
│                    INTERNET / VPN                          │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                 VPS SERVIDOR                               │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐   │
│  │   Nginx     │ │  API .NET   │ │    PostgreSQL       │   │
│  │ (SSL/Proxy) │ │   Backend   │ │    Database         │   │
│  │   Port 443  │ │  Port 5278  │ │    Port 5432        │   │
│  └─────────────┘ └─────────────┘ └─────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                      │
          ┌───────────┼───────────┐
          │           │           │
    ┌─────▼─────┐ ┌───▼────┐ ┌────▼────┐
    │ Cliente A │ │Cliente B│ │Cliente C│
    │PrinterAgent│ │PrinterAgent│ │PrinterAgent│
    │   :5000   │ │  :5000  │ │  :5000  │
    └───────────┘ └────────┘ └─────────┘
         │            │          │
    ┌────▼────┐  ┌────▼────┐ ┌───▼────┐
    │Impresoras│  │Impresoras│ │Impresoras│
    └─────────┘  └─────────┘ └────────┘
```

## 📊 **Características Principales**

### **🖥️ Sistema Central**
- **API REST** completa con endpoints CRUD
- **Autenticación JWT** segura
- **Base de datos PostgreSQL** con migraciones automáticas
- **Swagger UI** para documentación interactiva
- **Logs estructurados** con Serilog
- **Health checks** y monitoreo
- **CORS** configurado para múltiples orígenes

### **🤖 PrinterAgent Distribuido**
- **Descubrimiento automático** de impresoras en red
- **Monitoreo SNMP** para impresoras de red
- **Dashboard web local** para administración
- **Comunicación segura** con el sistema central
- **Instalación como servicio** (Windows/Linux)
- **Configuración flexible** por rangos de red
- **Reconexión automática** en caso de desconexión

### **🌐 Frontend Web**
- **Interfaz moderna** con Tailwind CSS
- **Dashboard en tiempo real** de todas las impresoras
- **Gestión de alertas** y notificaciones
- **Reportes y métricas** detalladas
- **Responsive design** para móviles y tablets

## 🔧 **Configuración de Producción**

### **Variables de Entorno Críticas**

```bash
# Base de datos
DB_PASSWORD=tu_password_muy_seguro
DB_HOST=postgres
DB_NAME=monitor_impresoras

# Seguridad JWT
JWT_SECRET_KEY=clave-super-secreta-de-al-menos-32-caracteres
JWT_ISSUER=https://tu-dominio.com
JWT_AUDIENCE=monitor-impresoras-users

# URLs públicas
FRONTEND_URL=https://tu-dominio.com
AGENT_DASHBOARD_URL=https://agent.tu-dominio.com

# SSL/HTTPS
SSL_CERTIFICATE_PATH=/etc/ssl/monitor-impresoras/certificate.crt
SSL_PRIVATE_KEY_PATH=/etc/ssl/monitor-impresoras/private.key
```

### **Puertos Utilizados**

| Puerto | Servicio | Descripción |
|--------|----------|-------------|
| **80** | Nginx HTTP | Redirige a HTTPS |
| **443** | Nginx HTTPS | Proxy reverso principal |
| **5278** | API Backend | API REST interna |
| **5432** | PostgreSQL | Base de datos |
| **5000** | PrinterAgent | Dashboard del agente |

## 🛠️ **Instalación de PrinterAgent**

### **Windows (PowerShell como Administrador):**

```powershell
# Despliegue automático
$url = "https://tu-dominio.com/install-agent.ps1"
$apiKey = "tu-api-key-segura"
$centralUrl = "https://tu-dominio.com/api"

Invoke-WebRequest -Uri $url -OutFile "install-agent.ps1"
.\install-agent.ps1 -CentralApiUrl $centralUrl -ApiKey $apiKey -Location "Oficina Principal"
```

### **Linux (Ubuntu/Debian/CentOS):**

```bash
# Despliegue automático
curl -sSL https://tu-dominio.com/deploy-agent.sh | sudo bash -s -- \
  --central-url https://tu-dominio.com/api \
  --api-key tu-api-key-segura \
  --agent-name "Agente-Sucursal-Norte" \
  --location "Sucursal Norte, Ciudad de México" \
  --network-ranges "192.168.1.0/24,10.0.0.0/24"
```

## 📈 **Monitoreo y Operación**

### **Health Checks Automáticos**

```bash
# Sistema principal
curl https://tu-dominio.com/health

# Respuesta esperada:
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "database": { "status": "Healthy" },
    "api": { "status": "Healthy" }
  }
}
```

### **Métricas en Tiempo Real**

```bash
# API de métricas
curl https://tu-dominio.com/api/metrics

# Dashboard de agente
curl http://ip-agente:5000/api/agent/metrics
```

### **Logs Centralizados**

```bash
# Ver todos los logs
docker-compose logs -f

# Solo errores
docker-compose logs | grep -i error

# Logs de un servicio específico
docker-compose logs -f api
docker-compose logs -f postgres
docker-compose logs -f nginx
```

## 🔐 **Seguridad**

### **SSL/TLS Automático**
- **Let's Encrypt** configurado automáticamente
- **Renovación automática** de certificados
- **HTTPS obligatorio** con redirección
- **Headers de seguridad** configurados

### **Autenticación y Autorización**
- **JWT Tokens** con expiración configurable
- **API Keys** para PrinterAgents
- **Roles y permisos** por usuario
- **Rate limiting** en endpoints críticos

### **Firewall y Red**
```bash
# Puertos abiertos automáticamente
ufw allow 80/tcp    # HTTP (redirige a HTTPS)
ufw allow 443/tcp   # HTTPS
ufw allow 22/tcp    # SSH
```

## 🔄 **Backup y Recuperación**

### **Backup Automático**

```bash
# Script de backup diario (ya configurado)
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
docker exec monitor-impresoras-db pg_dump -U postgres monitor_impresoras > /opt/backups/db_$DATE.sql
find /opt/backups -name "db_*.sql" -mtime +7 -delete
```

### **Restauración**

```bash
# Restaurar desde backup
cat backup.sql | docker exec -i monitor-impresoras-db psql -U postgres -d monitor_impresoras
```

## 📊 **Escalabilidad**

### **Múltiples Instancias de API**

```yaml
# docker-compose.yml
services:
  api1:
    build: .
    environment:
      - INSTANCE_ID=api-1
  api2:
    build: .
    environment:
      - INSTANCE_ID=api-2
  api3:
    build: .
    environment:
      - INSTANCE_ID=api-3
```

### **Load Balancer**

```nginx
upstream api_backend {
    least_conn;
    server api1:5000 weight=3;
    server api2:5000 weight=2;
    server api3:5000 weight=1;
}
```

## 🚨 **Troubleshooting**

### **Problemas Comunes**

| Problema | Solución |
|----------|----------|
| **API no responde** | `docker-compose restart api` |
| **Base de datos desconectada** | `docker-compose restart postgres` |
| **SSL no funciona** | Verificar certificados en `/etc/ssl/monitor-impresoras/` |
| **PrinterAgent no conecta** | Verificar API Key y URL en configuración |
| **Impresoras no detectadas** | Revisar rangos de red en PrinterAgent |

### **Logs de Diagnóstico**

```bash
# Diagnóstico completo
./diagnose.sh

# Logs específicos
journalctl -u docker -f                    # Docker
docker-compose logs postgres               # Base de datos
docker-compose logs nginx                  # Proxy
systemctl status printer-agent             # Agente (Linux)
Get-Service PrinterAgent                    # Agente (Windows)
```

## 📞 **Soporte y Mantenimiento**

### **Actualizaciones**

```bash
# Actualización automática
cd /opt/monitor-impresoras
git pull origin main
docker-compose build --no-cache
docker-compose up -d
```

### **Monitoreo Proactivo**

```bash
# Script de monitoreo (ejecuta cada 5 minutos)
#!/bin/bash
if ! curl -f -s https://tu-dominio.com/health > /dev/null; then
    echo "$(date): Sistema caído, reiniciando..." >> /var/log/monitor-alert.log
    cd /opt/monitor-impresoras && docker-compose restart
fi
```

## 🎉 **¡Sistema Listo para Producción!**

Tu sistema Monitor de Impresoras está ahora completamente configurado y listo para manejar:

- ✅ **Miles de impresoras** distribuidas globalmente
- ✅ **Múltiples ubicaciones** con PrinterAgents
- ✅ **Monitoreo 24/7** con alertas automáticas
- ✅ **Escalabilidad horizontal** según demanda
- ✅ **Alta disponibilidad** con recuperación automática
- ✅ **Seguridad empresarial** con SSL y autenticación

---

**📧 Soporte**: soporte@monitor-impresoras.com  
**📚 Documentación**: https://docs.monitor-impresoras.com  
**🐛 Issues**: https://github.com/tu-usuario/monitor-impresoras/issues  
**💬 Comunidad**: https://discord.gg/monitor-impresoras

