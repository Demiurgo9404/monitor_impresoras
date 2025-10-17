# 🚀 QOPIQ Monitor v1.0 - Sistema Enterprise de Monitoreo de Impresoras

## 📋 Descripción del Producto

**QOPIQ Monitor** es una solución enterprise completa para el monitoreo en tiempo real de impresoras, desarrollada con arquitectura Clean Architecture, tecnologías .NET 8 y Blazor Server, diseñada para empresas que requieren supervisión profesional de su flota de impresoras.

---

## 🏆 Características Principales

### ✅ **Monitoreo en Tiempo Real**
- Dashboard interactivo con actualizaciones automáticas
- Estados visuales: 🟢 Online, 🟡 Warning, 🔴 Offline
- Notificaciones instantáneas via SignalR
- Gráficos y métricas en tiempo real

### ✅ **Arquitectura Enterprise**
- Clean Architecture con separación de capas
- API REST documentada con Swagger
- Autenticación JWT + RefreshToken
- Base de datos PostgreSQL con EF Core

### ✅ **Interfaz Moderna**
- Frontend Blazor Server responsive
- Dashboard profesional con Chart.js
- UI/UX optimizada para dispositivos móviles
- Búsqueda y filtrado avanzado

### ✅ **Seguridad Avanzada**
- Autenticación JWT enterprise
- Roles y permisos granulares
- HTTPS/SSL configurado
- Auditoría y logging estructurado

### ✅ **Despliegue Automatizado**
- Containerización Docker completa
- Scripts de despliegue automatizado
- Configuración de producción lista
- Documentación técnica completa

---

## 📦 Contenido del Paquete

```
QOPIQ_Monitor_v1.0/
├── 📁 publish/                    # Aplicaciones compiladas
│   ├── Backend/                   # API .NET 8 lista para producción
│   └── Frontend/                  # Blazor Server compilado
├── 📁 docker/                     # Containerización
│   ├── Dockerfile.api             # Contenedor Backend
│   ├── Dockerfile.frontend        # Contenedor Frontend  
│   ├── docker-compose.yml         # Orquestación completa
│   └── .env.production           # Variables de entorno
├── 📁 scripts/                    # Automatización
│   ├── deploy.ps1                # Script Windows
│   └── deploy.sh                 # Script Linux/Mac
├── 📁 docs/                      # Documentación
│   ├── README-DEPLOYMENT.md      # Guía de despliegue
│   ├── ARCHITECTURE.md           # Documentación técnica
│   └── USER-MANUAL.md            # Manual de usuario
└── README.md                     # Este archivo
```

---

## 🚀 Instalación Rápida (5 minutos)

### Prerrequisitos
- **Docker Desktop** (Windows/Mac) o **Docker Engine** (Linux)
- **Git** para clonar repositorio
- **PowerShell** (Windows) o **Bash** (Linux/Mac)

### Opción 1: Despliegue Automático
```powershell
# Windows
cd QOPIQ_Monitor_v1.0/scripts
.\deploy.ps1 -Environment production

# Linux/Mac  
cd QOPIQ_Monitor_v1.0/scripts
chmod +x deploy.sh && ./deploy.sh production
```

### Opción 2: Docker Compose Manual
```bash
cd QOPIQ_Monitor_v1.0/docker
docker compose up -d --build
```

### Acceso al Sistema
- **🌐 Frontend**: http://localhost:5000
- **🔧 API**: http://localhost:5278  
- **📋 Swagger**: http://localhost:5278/swagger

---

## 🔐 Credenciales por Defecto

**Administrador:**
- **Email**: `admin@qopiq.com`
- **Contraseña**: `Admin@123`

⚠️ **IMPORTANTE**: Cambiar credenciales en el primer acceso a producción.

---

## 🏗️ Arquitectura del Sistema

### Stack Tecnológico
- **Backend**: ASP.NET Core 8, Clean Architecture, JWT, PostgreSQL
- **Frontend**: Blazor Server, SignalR, Bootstrap 5, Chart.js  
- **DevOps**: Docker, Docker Compose, Nginx, Redis
- **Seguridad**: JWT Authentication, HTTPS/SSL, Serilog

### Capas de la Aplicación
1. **Domain**: Entidades de negocio y reglas
2. **Application**: Casos de uso y servicios
3. **Infrastructure**: Implementaciones y acceso a datos
4. **API**: Controladores REST y SignalR Hubs
5. **Frontend**: Interfaz de usuario Blazor

---

## 📊 Funcionalidades Implementadas

### Dashboard Principal
- ✅ Vista general de impresoras en tiempo real
- ✅ Gráficos interactivos de estado
- ✅ Estadísticas y métricas
- ✅ Lista de impresoras con búsqueda

### Gestión de Impresoras  
- ✅ CRUD completo de impresoras
- ✅ Monitoreo de estado automático
- ✅ Alertas y notificaciones
- ✅ Historial de eventos

### Administración
- ✅ Gestión de usuarios y roles
- ✅ Autenticación JWT segura
- ✅ Configuración del sistema
- ✅ Reportes y exportación

### API REST
- ✅ Endpoints documentados con Swagger
- ✅ Autenticación JWT
- ✅ Versionado de API
- ✅ Health checks implementados

---

## 🔧 Configuración

### Variables de Entorno Principales
```env
# Base de Datos
DB_NAME=qopiq_monitor
DB_USER=qopiq_user
DB_PASSWORD=SecurePassword123

# JWT Security  
JWT_SECRET_KEY=YourSecretKey32CharsMinimum
JWT_EXPIRATION_MINUTES=60

# URLs
API_BASE_URL=https://api.tu-dominio.com
FRONTEND_URL=https://tu-dominio.com
```

### Puertos por Defecto
| Servicio | Puerto | Descripción |
|----------|--------|-------------|
| Frontend | 5000 | Aplicación Blazor |
| API | 5278 | Backend REST API |
| PostgreSQL | 5432 | Base de datos |
| Redis | 6379 | Cache distribuido |

---

## 📚 Documentación

### Guías Incluidas
- **📖 [Guía de Despliegue](docs/README-DEPLOYMENT.md)**: Instalación y configuración detallada
- **🏗️ [Arquitectura](docs/ARCHITECTURE.md)**: Documentación técnica completa  
- **👥 [Manual de Usuario](docs/USER-MANUAL.md)**: Guía de uso del sistema

### Recursos Adicionales
- **Swagger UI**: Documentación interactiva de API
- **Health Checks**: Monitoreo de estado del sistema
- **Logs**: Logging estructurado con Serilog

---

## 🛠️ Comandos Útiles

### Docker Management
```bash
# Ver estado de servicios
docker compose ps

# Ver logs en tiempo real  
docker compose logs -f

# Reiniciar servicios
docker compose restart

# Detener todo
docker compose down
```

### Troubleshooting
```bash
# Verificar salud de API
curl http://localhost:5278/health

# Verificar base de datos
docker compose exec postgres psql -U qopiq_user -d qopiq_monitor

# Ver logs específicos
docker compose logs api
docker compose logs frontend
```

---

## 🔐 Seguridad

### Características de Seguridad
- ✅ **JWT Authentication**: Tokens seguros con expiración
- ✅ **HTTPS/SSL**: Comunicación encriptada
- ✅ **Password Hashing**: BCrypt para contraseñas
- ✅ **Security Headers**: Headers de seguridad HTTP
- ✅ **CORS**: Configuración de dominios permitidos
- ✅ **Audit Logging**: Registro de acciones de usuarios

### Checklist de Producción
- [ ] Cambiar credenciales por defecto
- [ ] Configurar certificados SSL
- [ ] Actualizar JWT secret keys
- [ ] Configurar firewall
- [ ] Habilitar backup automático
- [ ] Configurar monitoreo

---

## 📈 Rendimiento y Escalabilidad

### Optimizaciones Incluidas
- **Connection Pooling**: Pool de conexiones a BD
- **Redis Caching**: Cache distribuido para sesiones
- **SignalR**: Comunicación eficiente tiempo real
- **Compression**: Compresión HTTP habilitada
- **Health Checks**: Monitoreo automático de servicios

### Escalabilidad Horizontal
- **Load Balancing**: Nginx configurado para múltiples instancias
- **Database Scaling**: PostgreSQL con replicación
- **Container Orchestration**: Docker Swarm/Kubernetes ready
- **Microservices Ready**: Arquitectura preparada para división

---

## 🆘 Soporte

### Información del Sistema
- **Versión**: QOPIQ Monitor v1.0
- **Framework**: .NET 8.0
- **Base de Datos**: PostgreSQL 15
- **Cache**: Redis 7
- **Proxy**: Nginx Alpine

### Resolución de Problemas
1. **Revisar logs**: `docker compose logs -f`
2. **Verificar health checks**: `curl http://localhost:5278/health`
3. **Comprobar conectividad**: Ping a servicios
4. **Consultar documentación**: Guías en carpeta `docs/`

---

## 🎯 Roadmap v2.0

### Mejoras Planificadas
- 🔄 **Observabilidad**: Prometheus + Grafana
- 📡 **Telemetría**: Application Insights  
- 🔔 **IA Predictiva**: Alertas inteligentes
- 📱 **App Móvil**: Blazor Hybrid (MAUI)
- 🏢 **Multi-tenant**: Gestión multiempresa
- 🤖 **Automatización**: Workflows inteligentes

---

## 📄 Licencia

**QOPIQ Monitor v1.0** - Sistema Enterprise de Monitoreo de Impresoras
Desarrollado con arquitectura Clean Architecture y tecnologías .NET 8

---

## 🎉 ¡Listo para Producción!

**QOPIQ Monitor v1.0** está completamente preparado para despliegue en entornos de producción, con todas las características enterprise necesarias para monitoreo profesional de impresoras.

**🚀 ¡Comience a monitorear sus impresoras ahora mismo!**
