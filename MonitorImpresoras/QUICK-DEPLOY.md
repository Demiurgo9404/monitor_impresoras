# 🚀 Guía de Despliegue Rápido - Monitor de Impresoras

## ⚡ Despliegue en 5 Minutos

### 1. **Preparar VPS**

```bash
# Conectar al VPS
ssh root@tu-vps-ip

# Actualizar sistema
apt update && apt upgrade -y

# Instalar dependencias básicas
apt install -y git curl wget docker.io docker-compose nginx certbot
systemctl enable docker
systemctl start docker
```

### 2. **Clonar y Configurar**

```bash
# Clonar repositorio
git clone https://github.com/tu-usuario/monitor-impresoras.git
cd monitor-impresoras/MonitorImpresoras

# Copiar configuración de ejemplo
cp .env.example .env

# Editar configuración (IMPORTANTE)
nano .env
```

**Configuración mínima en `.env`:**
```bash
DB_PASSWORD=tu_password_seguro_aqui
JWT_SECRET_KEY=tu-jwt-secret-de-al-menos-32-caracteres-muy-seguro
JWT_ISSUER=https://tu-dominio.com
FRONTEND_URL=https://tu-dominio.com
```

### 3. **Desplegar con Docker**

```bash
# Hacer ejecutable el script
chmod +x deploy.sh

# Ejecutar despliegue
./deploy.sh production
```

### 4. **Verificar Despliegue**

```bash
# Ver estado de contenedores
docker-compose ps

# Verificar logs
docker-compose logs -f api

# Probar endpoints
curl http://localhost:5278/health
curl http://localhost:5278/api/printers
```

## 🌐 **URLs de Acceso**

| Servicio | URL | Descripción |
|----------|-----|-------------|
| **API Principal** | `http://tu-ip:5278` | Backend principal |
| **Swagger UI** | `http://tu-ip:5278/swagger` | Documentación API |
| **Health Check** | `http://tu-ip:5278/health` | Estado del sistema |
| **Base de Datos** | `localhost:5432` | PostgreSQL |

## 🤖 **Instalar PrinterAgent en Cliente**

### Windows (PowerShell como Admin):
```powershell
# Descargar e instalar
Invoke-WebRequest -Uri "https://tu-dominio.com/install-agent.ps1" -OutFile "install-agent.ps1"
.\install-agent.ps1 -CentralApiUrl "https://tu-dominio.com/api" -ApiKey "tu-api-key"
```

### Linux:
```bash
# Descargar e instalar
curl -sSL https://tu-dominio.com/deploy-agent.sh | sudo bash -s -- \
  --central-url https://tu-dominio.com/api \
  --api-key tu-api-key \
  --location "Oficina Principal"
```

## 🔧 **Comandos de Administración**

### **Backend Principal:**
```bash
# Ver logs en tiempo real
docker-compose logs -f api

# Reiniciar servicios
docker-compose restart

# Backup de base de datos
docker exec monitor-impresoras-db pg_dump -U postgres monitor_impresoras > backup.sql

# Restaurar base de datos
cat backup.sql | docker exec -i monitor-impresoras-db psql -U postgres -d monitor_impresoras
```

### **PrinterAgent (Windows):**
```powershell
# Ver estado del servicio
Get-Service PrinterAgent

# Ver logs
Get-Content "C:\PrinterAgent\logs\agent-*.txt" -Tail 50 -Wait

# Reiniciar agente
Restart-Service PrinterAgent
```

### **PrinterAgent (Linux):**
```bash
# Ver estado
systemctl status printer-agent

# Ver logs
journalctl -u printer-agent -f

# Reiniciar
systemctl restart printer-agent
```

## 🔐 **Configuración SSL (Opcional)**

```bash
# Obtener certificado Let's Encrypt
certbot --nginx -d tu-dominio.com

# El script de despliegue ya configura SSL automáticamente
```

## 📊 **Monitoreo y Métricas**

### **Health Checks:**
```bash
# Sistema principal
curl https://tu-dominio.com/health

# Agente remoto
curl http://ip-agente:5000/health
```

### **Métricas de Base de Datos:**
```sql
-- Conectar a PostgreSQL
docker exec -it monitor-impresoras-db psql -U postgres -d monitor_impresoras

-- Ver estadísticas
SELECT COUNT(*) as total_printers FROM "Printers";
SELECT COUNT(*) as total_alerts FROM "Alerts";
SELECT COUNT(*) as active_agents FROM "Agents" WHERE "LastSeen" > NOW() - INTERVAL '5 minutes';
```

## 🚨 **Troubleshooting Rápido**

### **Problema: API no responde**
```bash
# Verificar contenedores
docker-compose ps

# Ver logs de errores
docker-compose logs api | grep ERROR

# Reiniciar API
docker-compose restart api
```

### **Problema: Base de datos no conecta**
```bash
# Verificar PostgreSQL
docker-compose logs postgres

# Probar conexión
docker exec -it monitor-impresoras-db psql -U postgres -c "SELECT version();"
```

### **Problema: PrinterAgent no se conecta**
```bash
# Windows
Get-EventLog -LogName Application -Source "PrinterAgent" -Newest 10

# Linux
journalctl -u printer-agent --since "1 hour ago"
```

## 📈 **Escalabilidad**

### **Múltiples Agentes:**
```bash
# Instalar en diferentes ubicaciones
./deploy-agent.sh --central-url https://tu-dominio.com/api --api-key key1 --location "Sucursal A"
./deploy-agent.sh --central-url https://tu-dominio.com/api --api-key key2 --location "Sucursal B"
```

### **Load Balancer (Nginx):**
```nginx
upstream api_backend {
    server api1:5278;
    server api2:5278;
    server api3:5278;
}
```

## 🔄 **Actualizaciones**

### **Sistema Principal:**
```bash
cd /opt/monitor-impresoras
git pull origin main
docker-compose build --no-cache
docker-compose up -d
```

### **PrinterAgent:**
```bash
# Windows
.\install-agent.ps1 -Update

# Linux
./deploy-agent.sh --update
```

## 📞 **Soporte de Emergencia**

### **Logs Críticos:**
```bash
# Todos los logs del sistema
docker-compose logs --tail=100

# Solo errores
docker-compose logs | grep -i error

# Logs de un servicio específico
docker-compose logs -f postgres
```

### **Backup de Emergencia:**
```bash
# Backup completo
mkdir -p /backup/$(date +%Y%m%d)
docker exec monitor-impresoras-db pg_dump -U postgres monitor_impresoras > /backup/$(date +%Y%m%d)/db.sql
cp -r /opt/monitor-impresoras/.env /backup/$(date +%Y%m%d)/
```

## ✅ **Checklist de Despliegue**

- [ ] VPS configurado con Docker
- [ ] Repositorio clonado
- [ ] Archivo `.env` configurado
- [ ] Script de despliegue ejecutado
- [ ] Servicios funcionando (`docker-compose ps`)
- [ ] API respondiendo (`curl /health`)
- [ ] Base de datos conectada
- [ ] SSL configurado (producción)
- [ ] DNS apuntando al servidor
- [ ] PrinterAgent instalado en al menos un cliente
- [ ] Monitoreo configurado
- [ ] Backups programados

---

**🎉 ¡Tu sistema Monitor de Impresoras está listo para producción!**

Para soporte: [GitHub Issues](https://github.com/tu-usuario/monitor-impresoras/issues)
