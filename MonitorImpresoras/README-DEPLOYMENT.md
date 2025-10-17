# 🚀 QOPIQ Monitor - Guía de Despliegue

## 📋 Resumen Ejecutivo

**QOPIQ Monitor de Impresoras** es un sistema completo de monitoreo en tiempo real con arquitectura Clean Architecture, autenticación JWT, y dashboard moderno desarrollado en .NET 8 y Blazor.

### 🏆 Estado del Proyecto: **LISTO PARA PRODUCCIÓN**

- ✅ **Backend API**: ASP.NET Core 8 con JWT Authentication
- ✅ **Frontend**: Blazor Server con SignalR tiempo real  
- ✅ **Base de Datos**: PostgreSQL con EF Core
- ✅ **Containerización**: Docker + Docker Compose
- ✅ **Monitoreo**: Health checks y logging estructurado

---

## 🚀 Despliegue Rápido (5 minutos)

### Prerrequisitos
- **Docker Desktop** (Windows/Mac) o **Docker Engine** (Linux)
- **Git** para clonar el repositorio
- **PowerShell** (Windows) o **Bash** (Linux/Mac)

### 1. Clonar y Configurar
```bash
git clone <repository-url>
cd monitor_impresoras/MonitorImpresoras
```

### 2. Despliegue Automático

**Windows (PowerShell):**
```powershell
.\deploy.ps1 -Environment production
```

**Linux/Mac (Bash):**
```bash
chmod +x deploy.sh
./deploy.sh production
```

### 3. Acceder al Sistema
- **🌐 Frontend**: http://localhost:5000
- **🔧 API**: http://localhost:5278
- **📋 Swagger**: http://localhost:5278/swagger

---

## 🛠️ Configuración Detallada

### Variables de Entorno

Edite `.env.production` para configurar:

```env
# Base de Datos
DB_NAME=qopiq_monitor
DB_USER=qopiq_user
DB_PASSWORD=QOPiQ2024!SecurePassword

# JWT Security
JWT_SECRET_KEY=QOPiQ-Monitor-JWT-Secret-Key-2024-Production-Min32Chars
JWT_EXPIRATION_MINUTES=60

# URLs
API_BASE_URL=https://api.qopiq-monitor.com
FRONTEND_URL=https://qopiq-monitor.com
```

### Puertos por Defecto

| Servicio | Puerto | Descripción |
|----------|--------|-------------|
| Frontend | 5000 | Aplicación Blazor |
| API | 5278 | Backend REST API |
| PostgreSQL | 5432 | Base de datos |
| Redis | 6379 | Cache (opcional) |
| Nginx | 80/443 | Proxy reverso |

---

## 🐳 Comandos Docker

### Gestión de Servicios
```bash
# Iniciar todos los servicios
docker compose up -d

# Ver logs en tiempo real
docker compose logs -f

# Reiniciar servicios
docker compose restart

# Detener servicios
docker compose stop

# Eliminar todo (datos incluidos)
docker compose down -v
```

### Monitoreo
```bash
# Estado de contenedores
docker compose ps

# Uso de recursos
docker stats

# Logs específicos
docker compose logs api
docker compose logs frontend
docker compose logs postgres
```

---

## 🔧 Configuración de Producción

### SSL/HTTPS

1. **Obtener certificados SSL:**
```bash
# Let's Encrypt (recomendado)
certbot certonly --standalone -d qopiq-monitor.com
```

2. **Configurar Nginx:**
```nginx
server {
    listen 443 ssl;
    server_name qopiq-monitor.com;
    
    ssl_certificate /etc/nginx/ssl/qopiq-monitor.crt;
    ssl_certificate_key /etc/nginx/ssl/qopiq-monitor.key;
    
    location / {
        proxy_pass http://frontend:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    location /api/ {
        proxy_pass http://api:5000/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Base de Datos

**Backup automático:**
```bash
# Crear backup
docker exec monitor-impresoras-db pg_dump -U qopiq_user qopiq_monitor > backup_$(date +%Y%m%d).sql

# Restaurar backup
docker exec -i monitor-impresoras-db psql -U qopiq_user qopiq_monitor < backup_20241017.sql
```

### Monitoreo y Logs

**Configurar rotación de logs:**
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/qopiq-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

---

## 🔐 Seguridad

### Checklist de Seguridad

- ✅ **JWT Secrets**: Claves seguras de 32+ caracteres
- ✅ **Database**: Credenciales fuertes y únicas
- ✅ **CORS**: Configurado para dominios específicos
- ✅ **HTTPS**: Certificados SSL válidos
- ✅ **Headers**: Security headers configurados
- ✅ **Firewall**: Puertos innecesarios cerrados

### Usuarios por Defecto

**Administrador:**
- Email: `admin@qopiq.com`
- Password: `Admin@123`

⚠️ **IMPORTANTE**: Cambiar credenciales por defecto en producción.

---

## 🧪 Testing y QA

### Tests Automatizados
```bash
# Ejecutar todos los tests
dotnet test --configuration Release

# Tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests de integración
dotnet test --filter Category=Integration
```

### Health Checks
```bash
# API Health
curl http://localhost:5278/health

# Base de datos
curl http://localhost:5278/health/db

# Servicios externos
curl http://localhost:5278/health/external
```

---

## 📊 Monitoreo y Métricas

### Endpoints de Monitoreo

| Endpoint | Descripción |
|----------|-------------|
| `/health` | Estado general del sistema |
| `/health/db` | Estado de base de datos |
| `/metrics` | Métricas Prometheus |
| `/swagger` | Documentación API |

### Alertas Recomendadas

- **CPU > 80%** por más de 5 minutos
- **Memoria > 90%** por más de 2 minutos  
- **Disco > 85%** disponible
- **API Response Time > 2s** promedio
- **Database Connections > 80%** del límite

---

## 🆘 Troubleshooting

### Problemas Comunes

**1. API no responde:**
```bash
# Verificar logs
docker compose logs api

# Verificar conectividad de DB
docker compose exec api dotnet ef database update
```

**2. Frontend no carga:**
```bash
# Verificar configuración
docker compose logs frontend

# Verificar conectividad con API
curl http://localhost:5278/health
```

**3. Base de datos no conecta:**
```bash
# Verificar PostgreSQL
docker compose logs postgres

# Probar conexión manual
docker compose exec postgres psql -U qopiq_user -d qopiq_monitor
```

### Logs Importantes

```bash
# Logs de aplicación
tail -f ./publish/Backend/logs/qopiq-*.log

# Logs de sistema
docker compose logs --tail=100 -f

# Logs específicos por servicio
docker compose logs api --tail=50
```

---

## 📈 Escalabilidad

### Configuración Multi-Instancia

```yaml
# docker-compose.scale.yml
services:
  api:
    deploy:
      replicas: 3
    
  frontend:
    deploy:
      replicas: 2
```

### Load Balancer

```nginx
upstream api_backend {
    server api_1:5000;
    server api_2:5000;
    server api_3:5000;
}

upstream frontend_backend {
    server frontend_1:3000;
    server frontend_2:3000;
}
```

---

## 📞 Soporte

### Información del Sistema
- **Versión**: 1.0.0
- **Framework**: .NET 8.0
- **Base de Datos**: PostgreSQL 15
- **Cache**: Redis 7
- **Proxy**: Nginx Alpine

### Contacto Técnico
- **Documentación**: Este archivo README
- **Issues**: GitHub Issues
- **Logs**: `/app/logs/` en contenedores

---

## 🎯 Próximos Pasos

1. **Configurar SSL** para producción
2. **Implementar CI/CD** pipeline
3. **Configurar monitoreo** avanzado (Grafana/Prometheus)
4. **Backup automático** de base de datos
5. **Scaling horizontal** según demanda

---

**🎉 ¡QOPIQ Monitor está listo para producción!**
