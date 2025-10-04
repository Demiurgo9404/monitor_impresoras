# 🚀 QOPIQ - Guía Completa de Despliegue

## 📋 Índice
1. [Resumen del Sistema](#resumen-del-sistema)
2. [Requisitos Previos](#requisitos-previos)
3. [Configuración del Entorno](#configuración-del-entorno)
4. [Despliegue Paso a Paso](#despliegue-paso-a-paso)
5. [Verificación del Sistema](#verificación-del-sistema)
6. [Usuarios de Prueba](#usuarios-de-prueba)
7. [Configuración de Producción](#configuración-de-producción)
8. [Monitoreo y Mantenimiento](#monitoreo-y-mantenimiento)
9. [Solución de Problemas](#solución-de-problemas)

---

## 🎯 Resumen del Sistema

**QOPIQ** es una plataforma SaaS multi-tenant para empresas de renta de impresoras que incluye:

### ✅ **Componentes Principales:**
- **Backend API**: .NET 8 con Clean Architecture
- **Frontend Web**: Blazor Server con dashboards por roles
- **Base de Datos**: PostgreSQL con multi-tenancy
- **Sistema de Reportes**: PDF/Excel automatizados con scheduler
- **Autenticación**: JWT con roles granulares
- **PrinterAgent**: Agente distribuido para monitoreo

### 🏗️ **Arquitectura por Roles:**
- **SuperAdmin**: Control global de la plataforma
- **CompanyAdmin**: Gestión de empresa y proyectos
- **ProjectManager**: Gestión de proyectos asignados
- **Viewer**: Acceso de solo lectura a reportes

---

## 🔧 Requisitos Previos

### **Software Requerido:**
- ✅ .NET 8 SDK
- ✅ PostgreSQL 14+
- ✅ Node.js 18+ (para herramientas de frontend)
- ✅ Git
- ✅ PowerShell 7+ (para scripts de despliegue)

### **Hardware Mínimo:**
- **CPU**: 4 cores
- **RAM**: 8GB
- **Disco**: 50GB SSD
- **Red**: Conexión estable a internet

### **Puertos Requeridos:**
- **5278**: Backend API
- **5000**: Frontend Web
- **5432**: PostgreSQL (si local)

---

## ⚙️ Configuración del Entorno

### **1. Clonar el Repositorio**
```bash
git clone https://github.com/Demiurgo9404/monitor_impresoras.git
cd monitor_impresoras
```

### **2. Configurar Base de Datos**
```sql
-- Crear base de datos
CREATE DATABASE qopiq_db;
CREATE USER qopiq_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE qopiq_db TO qopiq_user;
```

### **3. Configurar Variables de Entorno**
Crear archivo `.env` en la raíz del proyecto:
```env
# Database
DATABASE_URL=Host=localhost;Database=qopiq_db;Username=qopiq_user;Password=your_secure_password

# JWT
JWT_SECRET=your_super_secure_jwt_secret_key_here
JWT_ISSUER=QOPIQ
JWT_AUDIENCE=QOPIQ-Users

# Email (SMTP)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your_email@gmail.com
SMTP_PASSWORD=your_app_password
SMTP_FROM=noreply@qopiq.com

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5278
```

---

## 🚀 Despliegue Paso a Paso

### **Opción A: Despliegue Automático (Recomendado)**

1. **Ejecutar Script de Verificación Completa:**
```powershell
cd scripts
.\verify-complete-system.ps1
```

Este script ejecutará automáticamente:
- ✅ Verificación del backend
- ✅ Verificación del frontend
- ✅ Creación de usuarios de prueba
- ✅ Validación de endpoints
- ✅ Pruebas de conectividad

### **Opción B: Despliegue Manual**

#### **Paso 1: Preparar Backend**
```powershell
cd MonitorImpresoras\MonitorImpresoras.API
dotnet restore
dotnet ef database update
dotnet build --configuration Release
dotnet run --urls="http://localhost:5278"
```

#### **Paso 2: Preparar Frontend**
```powershell
cd MonitorImpresoras\MonitorImpresoras.Web
dotnet restore
dotnet build --configuration Release
dotnet run --urls="http://localhost:5000"
```

#### **Paso 3: Crear Usuarios de Prueba**
```powershell
cd scripts
.\setup-test-users.ps1
```

---

## ✅ Verificación del Sistema

### **1. Verificar Backend**
- 🌐 **API Base**: http://localhost:5278
- 📚 **Swagger**: http://localhost:5278/swagger
- ❤️ **Health Check**: http://localhost:5278/health

### **2. Verificar Frontend**
- 🏠 **Aplicación**: http://localhost:5000
- 📊 **Reportes**: http://localhost:5000/reports
- 📅 **Calendario**: http://localhost:5000/scheduled-reports
- 🎨 **Templates**: http://localhost:5000/templates

### **3. Endpoints Críticos**
```http
GET /api/auth/login          # Autenticación
GET /api/report              # Reportes
GET /api/scheduledreport     # Reportes programados
GET /api/printers            # Impresoras
GET /api/admin/companies     # Empresas (SuperAdmin)
```

---

## 👥 Usuarios de Prueba

### **Credenciales por Rol:**

#### 👑 **SuperAdmin**
- **Email**: `admin@qopiq.com`
- **Password**: `Admin123!`
- **Acceso**: Control total de la plataforma

#### 🏢 **CompanyAdmin**
- **Email**: `companyadmin@qopiq.com`
- **Password**: `CompanyAdmin123!`
- **Acceso**: Gestión de empresa y proyectos

#### 📋 **ProjectManager**
- **Email**: `pm@qopiq.com`
- **Password**: `ProjectManager123!`
- **Acceso**: Proyectos asignados

#### 👁️ **Viewer**
- **Email**: `viewer@qopiq.com`
- **Password**: `Viewer123!`
- **Acceso**: Solo lectura de reportes

---

## 🏭 Configuración de Producción

### **1. Variables de Entorno de Producción**
```env
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=your_production_database_url
JWT_SECRET=your_production_jwt_secret
SMTP_HOST=your_production_smtp_host
```

### **2. Configuración de SSL**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "certificate.pfx",
          "Password": "certificate_password"
        }
      }
    }
  }
}
```

### **3. Docker Deployment (Opcional)**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "MonitorImpresoras.API.dll"]
```

### **4. Configuración de Proxy Reverso (Nginx)**
```nginx
server {
    listen 80;
    server_name your-domain.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    location /api/ {
        proxy_pass http://localhost:5278;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

---

## 📊 Monitoreo y Mantenimiento

### **1. Health Checks**
- **Endpoint**: `/health`
- **Frecuencia**: Cada 30 segundos
- **Alertas**: Email automático en caso de fallo

### **2. Logs**
- **Ubicación**: `logs/qopiq-{date}.log`
- **Nivel**: Information en producción
- **Rotación**: Diaria

### **3. Backup de Base de Datos**
```bash
# Backup diario automático
pg_dump -h localhost -U qopiq_user qopiq_db > backup_$(date +%Y%m%d).sql
```

### **4. Métricas Clave**
- ✅ Tiempo de respuesta de API < 500ms
- ✅ Disponibilidad > 99.9%
- ✅ Uso de CPU < 80%
- ✅ Uso de memoria < 85%

---

## 🔧 Solución de Problemas

### **Problemas Comunes:**

#### **1. Backend no inicia**
```bash
# Verificar puerto
netstat -an | findstr :5278

# Verificar logs
tail -f logs/qopiq-*.log

# Verificar base de datos
dotnet ef database update
```

#### **2. Frontend no conecta con Backend**
- Verificar URL en `appsettings.json`
- Verificar CORS en backend
- Verificar firewall/antivirus

#### **3. Autenticación falla**
- Verificar JWT_SECRET
- Verificar usuarios en base de datos
- Verificar headers de autorización

#### **4. Reportes no se generan**
- Verificar permisos de archivos
- Verificar configuración SMTP
- Verificar scheduler service

### **Comandos de Diagnóstico:**
```powershell
# Verificar servicios
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}

# Verificar puertos
netstat -an | findstr "5278\|5000"

# Verificar logs
Get-Content logs\qopiq-*.log -Tail 50

# Verificar base de datos
psql -h localhost -U qopiq_user -d qopiq_db -c "SELECT COUNT(*) FROM Users;"
```

---

## 📞 Soporte

### **Contacto Técnico:**
- **Email**: support@qopiq.com
- **Documentación**: https://docs.qopiq.com
- **Issues**: https://github.com/Demiurgo9404/monitor_impresoras/issues

### **Recursos Adicionales:**
- 📚 **API Documentation**: `/swagger`
- 🎥 **Video Tutorials**: En desarrollo
- 💬 **Community Forum**: En desarrollo

---

## 🎉 ¡Felicitaciones!

Si has llegado hasta aquí, **QOPIQ está completamente desplegado y listo para producción**. 

### **Próximos Pasos:**
1. ✅ Realizar pruebas de usuario final
2. ✅ Configurar monitoreo en producción
3. ✅ Entrenar a los usuarios finales
4. ✅ Planificar estrategia de lanzamiento

**¡QOPIQ está listo para transformar la gestión de impresoras en tu organización! 🚀**
