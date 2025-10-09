# PrinterAgent - Agente Distribuido de Monitoreo de Impresoras

## 📋 Descripción

PrinterAgent es un agente distribuido que permite monitorear impresoras desde cualquier ubicación del mundo y transmitir los datos al sistema central de Monitor de Impresoras. Está diseñado para ser desplegado en diferentes sucursales, oficinas o ubicaciones remotas.

## 🏗️ Arquitectura

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Ubicación A   │    │   Ubicación B   │    │   Ubicación C   │
│  PrinterAgent   │    │  PrinterAgent   │    │  PrinterAgent   │
│                 │    │                 │    │                 │
│ ┌─────────────┐ │    │ ┌─────────────┐ │    │ ┌─────────────┐ │
│ │ Impresora 1 │ │    │ │ Impresora 3 │ │    │ │ Impresora 5 │ │
│ │ Impresora 2 │ │    │ │ Impresora 4 │ │    │ │ Impresora 6 │ │
│ └─────────────┘ │    │ └─────────────┘ │    │ └─────────────┘ │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────▼─────────────┐
                    │    Sistema Central        │
                    │  Monitor de Impresoras   │
                    │                          │
                    │ ┌─────────────────────┐  │
                    │ │     Dashboard       │  │
                    │ │     Reportes        │  │
                    │ │     Alertas         │  │
                    │ └─────────────────────┘  │
                    └──────────────────────────┘
```

## 🚀 Componentes

### 1. PrinterAgent.Core
- **Propósito**: Lógica de negocio y servicios core del agente
- **Funcionalidades**:
  - Descubrimiento automático de impresoras en la red local
  - Monitoreo continuo de estado de impresoras
  - Recolección de métricas (niveles de tinta, páginas impresas, etc.)
  - Comunicación segura con el sistema central

### 2. PrinterAgent.API
- **Propósito**: API REST para configuración y control del agente
- **Funcionalidades**:
  - Endpoints para configuración remota
  - API para consulta de estado local
  - Webhooks para notificaciones inmediatas
  - Panel de administración web

## 🌐 Características Principales

### ✅ Conectividad Global
- Comunicación vía HTTPS con el sistema central
- Soporte para proxies corporativos
- Reconexión automática en caso de pérdida de conectividad
- Almacenamiento local de datos durante desconexiones

### ✅ Seguridad
- Autenticación mediante API Keys
- Cifrado de datos en tránsito (TLS 1.3)
- Validación de certificados
- Logs de auditoría

### ✅ Configuración Remota
- Configuración centralizada desde el sistema principal
- Actualización automática de configuraciones
- Gestión de credenciales segura
- Políticas de monitoreo personalizables

### ✅ Monitoreo Inteligente
- Detección automática de impresoras en la red
- Monitoreo adaptativo según el tipo de impresora
- Alertas proactivas por umbrales configurables
- Métricas de rendimiento de red

## 📦 Instalación y Despliegue

### Requisitos
- .NET 8.0 Runtime
- Conexión a Internet
- Acceso a la red local donde están las impresoras
- Windows 10/11 o Windows Server 2019+

### Instalación Rápida
```bash
# 1. Descargar el agente
wget https://releases.monitor-impresoras.com/agent/latest/PrinterAgent.zip

# 2. Extraer archivos
unzip PrinterAgent.zip -d /opt/printer-agent

# 3. Configurar
./configure.sh --central-url https://monitor.empresa.com --api-key YOUR_API_KEY

# 4. Instalar como servicio
./install-service.sh

# 5. Iniciar
systemctl start printer-agent
```

## ⚙️ Configuración

### Archivo de Configuración (appsettings.json)
```json
{
  "Agent": {
    "Id": "agent-sucursal-norte",
    "Name": "Sucursal Norte",
    "Location": "Ciudad de México, México",
    "CentralApiUrl": "https://monitor.empresa.com/api",
    "ApiKey": "your-secure-api-key",
    "ReportingInterval": "00:05:00",
    "HealthCheckInterval": "00:01:00"
  },
  "Network": {
    "ScanRanges": [
      "192.168.1.0/24",
      "10.0.0.0/24"
    ],
    "SnmpCommunity": "public",
    "SnmpTimeout": 5000
  },
  "Logging": {
    "Level": "Information",
    "RetentionDays": 30
  }
}
```

## 🔧 API Endpoints

### Configuración
- `GET /api/config` - Obtener configuración actual
- `PUT /api/config` - Actualizar configuración
- `POST /api/config/reload` - Recargar configuración

### Monitoreo
- `GET /api/printers` - Lista de impresoras detectadas
- `GET /api/printers/{id}/status` - Estado de impresora específica
- `POST /api/printers/scan` - Escanear red en busca de impresoras

### Estado del Agente
- `GET /api/agent/status` - Estado del agente
- `GET /api/agent/health` - Health check
- `GET /api/agent/logs` - Logs recientes

## 📊 Métricas Recolectadas

### Por Impresora
- Estado (Online/Offline/Error)
- Niveles de tinta/tóner por color
- Páginas impresas totales
- Páginas impresas desde último reporte
- Trabajos en cola
- Errores y advertencias
- Temperatura (si está disponible)
- Información del firmware

### Del Agente
- Tiempo de actividad
- Latencia de red al sistema central
- Número de impresoras monitoreadas
- Errores de comunicación
- Uso de recursos (CPU, memoria)

## 🚨 Sistema de Alertas

### Tipos de Alertas
- **Críticas**: Impresora offline, error de hardware
- **Advertencias**: Tóner bajo, papel agotado
- **Informativas**: Nueva impresora detectada, mantenimiento programado

### Canales de Notificación
- Envío inmediato al sistema central
- Email (opcional)
- Webhook personalizado
- Log local

## 🔄 Sincronización de Datos

### Estrategia de Envío
1. **Tiempo Real**: Alertas críticas y cambios de estado
2. **Batch Periódico**: Métricas regulares cada 5 minutos
3. **Resincronización**: Datos acumulados durante desconexiones

### Manejo de Desconexiones
- Buffer local de hasta 24 horas de datos
- Compresión automática de datos antiguos
- Reenvío inteligente al reconectar

## 🛠️ Desarrollo y Extensibilidad

### Plugins
- Sistema de plugins para tipos específicos de impresoras
- API para desarrolladores de terceros
- Soporte para protocolos personalizados

### Integración
- SDK para integración con sistemas existentes
- Webhooks para eventos en tiempo real
- API REST completa

## 📈 Monitoreo del Agente

### Dashboard Local
- Interfaz web en `http://localhost:8080`
- Estado en tiempo real de todas las impresoras
- Configuración visual
- Logs y diagnósticos

### Métricas de Rendimiento
- Latencia de red
- Tasa de éxito de comunicación
- Uso de recursos del sistema
- Estadísticas de descubrimiento de impresoras

## 🔐 Seguridad y Compliance

### Características de Seguridad
- Comunicación cifrada end-to-end
- Rotación automática de API keys
- Logs de auditoría completos
- Validación de integridad de datos

### Compliance
- GDPR ready (no almacena datos personales)
- SOC 2 Type II compatible
- Logs de auditoría para compliance

## 📞 Soporte

### Documentación
- Guía de instalación detallada
- Troubleshooting común
- API Reference completa
- Ejemplos de configuración

### Soporte Técnico
- Email: support@monitor-impresoras.com
- Portal de soporte: https://support.monitor-impresoras.com
- Documentación: https://docs.monitor-impresoras.com

---

**Versión**: 1.0.0  
**Última actualización**: Octubre 2025  
**Licencia**: Propietaria - Monitor de Impresoras Enterprise

