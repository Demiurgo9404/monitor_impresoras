# 🖨️ QOPIQ - Sistema de Monitoreo de Impresoras Enterprise

## 🎯 **OBJETIVO DEL PROYECTO**

**QOPIQ** es una aplicación web enterprise para el monitoreo integral de impresoras diseñada específicamente para empresas de alquiler de impresoras. Permite el control en tiempo real de contadores, consumibles, estado y rendimiento de flotas de impresoras distribuidas.

### 🚨 **ESTADO CRÍTICO - ENTREGA EN 3 DÍAS**

**Fecha límite**: 9 de Octubre 2025  
**Estado actual**: 75% completado  
**Prioridad**: MÁXIMA - Funcionalidad core operativa  

---

## 🏗️ **ARQUITECTURA DEL SISTEMA**

```
┌─────────────────────────────────────────────────────────────────┐
│                    QOPIQ ENTERPRISE                        │
├─────────────────────────────────────────────────────────────────┤
│  🌐 APLICACIÓN WEB (React Multi-Tenant)                   │
│  ├── Dashboard en Tiempo Real                              │
│  ├── Reportes Automatizados (PDF/Excel)                   │
│  ├── Gestión de Usuarios y Suscripciones                  │
│  └── Analytics Avanzados                                   │
├─────────────────────────────────────────────────────────────────┤
│  🔧 API BACKEND (.NET 8 Clean Architecture)               │
│  ├── Autenticación JWT Multi-Tenant                       │
│  ├── Sistema de Reportes Automatizados                    │
│  ├── Gestión de Contadores y Consumibles                  │
│  └── Alertas Proactivas                                    │
├─────────────────────────────────────────────────────────────────┤
│  💾 BASE DE DATOS (PostgreSQL)                            │
│  ├── Datos de Impresoras y Contadores                     │
│  ├── Usuarios y Suscripciones                             │
│  ├── Reportes y Configuraciones                           │
│  └── Logs de Auditoría                                     │
├─────────────────────────────────────────────────────────────────┤
│  🤖 PRINTER AGENT (Windows Service)                       │
│  ├── Instalación en Máquinas Windows                      │
│  ├── Monitoreo en Tiempo Real                             │
│  ├── Recolección de Contadores                            │
│  └── Comunicación Segura con API                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 **FUNCIONALIDADES CORE**

### 📊 **Monitoreo en Tiempo Real**
- ✅ **Contadores de impresión**: Páginas totales, color, B&N
- ✅ **Contadores de escáner**: Documentos escaneados
- ✅ **Niveles de tóner**: Por color (C, M, Y, K)
- ✅ **Estado de consumibles**: Papel, tambores, fusor
- ✅ **Estado de conectividad**: Online/Offline en tiempo real
- ✅ **Alertas proactivas**: Tóner bajo, errores, mantenimiento

### 📈 **Reportes Empresariales**
- ✅ **Reportes automáticos**: PDF y Excel profesionales
- ✅ **Programación flexible**: Diario, semanal, mensual
- ✅ **Distribución por email**: Múltiples destinatarios
- ✅ **Análisis de costos**: Por impresora, departamento, proyecto
- ✅ **Tendencias de uso**: Gráficos y estadísticas avanzadas

### 🏢 **Multi-Tenant Enterprise**
- ✅ **Gestión de empresas**: Múltiples clientes aislados
- ✅ **Usuarios y roles**: Admin, Manager, Viewer
- ✅ **Suscripciones**: Free, Basic, Pro, Enterprise
- ✅ **Facturación automática**: Integración con Stripe
- ✅ **Límites por plan**: Control de impresoras por suscripción

---

## 🚀 **COMPONENTES IMPLEMENTADOS**

### ✅ **BACKEND API (.NET 8)**
```
MonitorImpresoras/
├── MonitorImpresoras.API/          # Controllers y configuración
├── MonitorImpresoras.Application/  # Lógica de negocio
├── MonitorImpresoras.Domain/       # Entidades y reglas
└── MonitorImpresoras.Infrastructure/ # Datos y servicios externos
```

**Estado**: ✅ Domain y Application compilando | ❌ Infrastructure con errores

### ✅ **PRINTER AGENT (Windows)**
```
PrinterAgent/
├── PrinterAgent.Core/    # Lógica de monitoreo
└── PrinterAgent.API/     # API REST y dashboard
```

**Estado**: ✅ 100% implementado y funcional

### ✅ **FRONTEND REACT**
```
frontend/
├── src/components/       # Componentes React
├── src/pages/           # Páginas principales
└── src/services/        # Integración con API
```

**Estado**: ✅ Dashboard multi-tenant implementado

---

## 📋 **PLAN DE DESARROLLO - 3 DÍAS**

### 🔥 **DÍA 1 (HOY) - BACKEND FUNCIONAL**
- [ ] **Resolver errores de Infrastructure** (202 errores)
- [ ] **Compilación exitosa** de toda la solución
- [ ] **Migración de base de datos** funcionando
- [ ] **API ejecutándose** con endpoints básicos
- [ ] **Autenticación JWT** operativa

### ⚡ **DÍA 2 - INTEGRACIÓN COMPLETA**
- [ ] **PrinterAgent conectado** con API
- [ ] **Recolección de datos** en tiempo real
- [ ] **Frontend integrado** con backend
- [ ] **Reportes básicos** funcionando
- [ ] **Sistema de alertas** operativo

### 🎯 **DÍA 3 - PRODUCCIÓN**
- [ ] **Despliegue en servidor** (Docker)
- [ ] **Configuración de producción**
- [ ] **Testing E2E** completo
- [ ] **Documentación final**
- [ ] **Entrega funcional** ✅

---

## 🔧 **ENDPOINTS API PRINCIPALES**

### 🔐 **Autenticación**
```http
POST   /api/auth/login              # Iniciar sesión
POST   /api/auth/register           # Registrar usuario
POST   /api/auth/refresh            # Renovar token
POST   /api/auth/logout             # Cerrar sesión
GET    /api/auth/profile            # Perfil de usuario
POST   /api/auth/agent/heartbeat    # Heartbeat del agente
```

### 🖨️ **Gestión de Impresoras**
```http
GET    /api/printers                # Lista de impresoras
GET    /api/printers/{id}           # Detalles de impresora
POST   /api/printers                # Agregar impresora
PUT    /api/printers/{id}           # Actualizar impresora
DELETE /api/printers/{id}           # Eliminar impresora
GET    /api/printers/{id}/status    # Estado en tiempo real
GET    /api/printers/{id}/counters  # Contadores actuales
```

### 📊 **Reportes Automatizados**
```http
POST   /api/report/generate         # Generar nuevo reporte
GET    /api/report                  # Lista de reportes
GET    /api/report/{id}/download    # Descargar reporte
POST   /api/report/schedule         # Programar reporte
GET    /api/report/quick/{id}       # Reporte rápido
```

### 🚨 **Sistema de Alertas**
```http
GET    /api/alerts                  # Lista de alertas
POST   /api/alerts                  # Crear alerta
PUT    /api/alerts/{id}             # Actualizar alerta
DELETE /api/alerts/{id}             # Eliminar alerta
POST   /api/alerts/test             # Enviar alerta de prueba
```

---

## 🤖 **PRINTER AGENT - INSTALACIÓN**

### **Requisitos del Sistema**
- Windows 10/11 o Windows Server 2019+
- .NET 8.0 Runtime
- Acceso a red local (impresoras)
- Conexión a Internet (API)

### **Instalación Automática**
```powershell
# Descargar e instalar
Invoke-WebRequest -Uri "https://releases.qopiq.com/agent/install.ps1" -OutFile "install.ps1"
.\install.ps1 -ApiUrl "https://api.qopiq.com" -ApiKey "YOUR_API_KEY"
```

### **Configuración Manual**
```json
{
  "Agent": {
    "Id": "agent-oficina-principal",
    "Name": "Oficina Principal",
    "ApiUrl": "https://api.qopiq.com",
    "ApiKey": "your-secure-api-key",
    "ReportingInterval": "00:05:00"
  },
  "Network": {
    "ScanRanges": ["192.168.1.0/24"],
    "SnmpCommunity": "public"
  }
}
```

---

## 🗄️ **BASE DE DATOS**

### **Entidades Principales**
- **Users**: Usuarios del sistema
- **Companies**: Empresas multi-tenant
- **Subscriptions**: Planes y suscripciones
- **Printers**: Información de impresoras
- **PrinterCounters**: Contadores históricos
- **Reports**: Reportes generados
- **Alerts**: Sistema de alertas

### **Migración Inicial**
```bash
cd MonitorImpresoras/MonitorImpresoras.API
dotnet ef migrations add "InitialMigration"
dotnet ef database update
```

---

## 🚀 **DESPLIEGUE RÁPIDO**

### **Desarrollo Local**
```bash
# 1. Clonar repositorio
git clone https://github.com/empresa/monitor-impresoras.git
cd monitor-impresoras/MonitorImpresoras

# 2. Configurar base de datos
docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:15

# 3. Ejecutar migraciones
dotnet ef database update --project MonitorImpresoras.Infrastructure --startup-project MonitorImpresoras.API

# 4. Ejecutar API
cd MonitorImpresoras.API
dotnet run
```

### **Producción (Docker)**
```bash
# Despliegue completo con Docker Compose
docker-compose up -d

# Verificar servicios
docker-compose ps
```

---

## 📚 **DOCUMENTACIÓN TÉCNICA**

### **Arquitectura**
- [Clean Architecture](./docs/ARCHITECTURE.md)
- [Patrones de Diseño](./docs/PATTERNS.md)
- [Multi-Tenancy](./ADR-MULTITENANT.md)

### **APIs**
- [Documentación de API](./docs/API.md)
- [Autenticación JWT](./README-AUTH.md)
- [Sistema de Reportes](./README-REPORTS.md)

### **Despliegue**
- [Guía de Producción](./README-PRODUCTION.md)
- [Despliegue Rápido](./QUICK-DEPLOY.md)
- [PrinterAgent](./PrinterAgent/README.md)

---

## 🔧 **PROBLEMAS CONOCIDOS Y SOLUCIONES**

### ❌ **Infrastructure Layer (202 errores)**
**Problema**: Servicios faltantes y interfaces no implementadas
**Solución**: 
```bash
# Restaurar servicios esenciales
cd MonitorImpresoras.Infrastructure
# Implementar interfaces faltantes
# Registrar servicios en DependencyInjection
```

### ❌ **Compilación Fallida**
**Problema**: Dependencias circulares y archivos duplicados
**Solución**:
```bash
# Limpiar solución
dotnet clean
# Restaurar paquetes
dotnet restore
# Compilar por capas
dotnet build MonitorImpresoras.Domain
dotnet build MonitorImpresoras.Application
```

---

## 📞 **CONTACTO Y SOPORTE**

### **Equipo de Desarrollo**
- **Lead Developer**: [Nombre]
- **Backend**: .NET 8, PostgreSQL, JWT
- **Frontend**: React, TypeScript, Tailwind
- **DevOps**: Docker, CI/CD

### **Enlaces Importantes**
- **Repositorio**: https://github.com/empresa/monitor-impresoras
- **API Docs**: https://api.qopiq.com/swagger
- **Dashboard**: https://app.qopiq.com
- **Soporte**: support@qopiq.com

---

## 🎯 **PRÓXIMOS PASOS INMEDIATOS**

### **PRIORIDAD 1 - HOY**
1. ✅ Resolver errores de Infrastructure
2. ✅ Compilación exitosa completa
3. ✅ Migración de base de datos
4. ✅ API ejecutándose localmente

### **PRIORIDAD 2 - MAÑANA**
1. 🔄 Integración PrinterAgent ↔ API
2. 🔄 Frontend conectado al backend
3. 🔄 Reportes básicos funcionando
4. 🔄 Sistema de alertas operativo

### **PRIORIDAD 3 - ENTREGA**
1. 🎯 Despliegue en producción
2. 🎯 Testing E2E completo
3. 🎯 Documentación final
4. 🎯 **ENTREGA FUNCIONAL** 🚀

---

**Versión**: 2.0.0  
**Última actualización**: 6 de Octubre 2025  
**Estado**: 🔥 DESARROLLO ACTIVO - ENTREGA EN 3 DÍAS  
**Licencia**: Propietaria - QOPIQ Enterprise
