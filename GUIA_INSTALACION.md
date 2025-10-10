# 🚀 Guía de Instalación Rápida - QOPIQ

## 📋 Requisitos Previos

### Para Desarrollo
- Windows 10/11 o Windows Server 2016+
- Docker Desktop 4.12+
- PowerShell 5.1+
- Git
- .NET 6.0 SDK

### Para Producción
- Windows Server 2019/2022 o Linux
- Docker 20.10+
- Docker Compose 1.29+
- 2 vCPUs mínimos
- 4GB RAM mínimo
- 10GB de espacio en disco

## 🛠 Instalación en 5 Pasos

### 1. Clonar el Repositorio
```powershell
# Clonar el repositorio
git clone https://github.com/tuusuario/monitor_impresoras.git
cd monitor_impresoras
```

### 2. Configurar Variables de Entorno
```powershell
# Copiar el archivo de ejemplo
Copy-Item .env.example .env.production

# Editar el archivo .env.production con tus valores
notepad .env.production
```

### 3. Iniciar los Servicios
```powershell
# Otorgar permisos al script (solo primera vez)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process

# Ejecutar el script de despliegue
.\deploy.ps1
```

### 4. Verificar la Instalación
```powershell
# Verificar contenedores en ejecución
docker ps

# Ver logs de la aplicación
docker-compose logs -f
```

### 5. Acceder a la Aplicación
- **Interfaz Web**: http://localhost:5000
- **Documentación API**: http://localhost:5000/swagger
- **Métricas**: http://localhost:5000/metrics

## 🔧 Configuración Avanzada

### Variables de Entorno Importantes
```ini
# Base de datos
POSTGRES_PASSWORD=TuContraseñaSegura123!

# Aplicación
ASPNETCORE_ENVIRONMENT=Production

# JWT
JWT__Key=TuClaveSecretaMuySegura1234567890
JWT__Issuer=QOPIQ-API
JWT__Audience=QOPIQ-Client
```

### Comandos Útiles
```powershell
# Detener la aplicación
docker-compose down

# Reiniciar un servicio específico
docker-compose restart api

# Ver logs en tiempo real
docker-compose logs -f --tail=100

# Respaldar la base de datos
docker exec -t postgres pg_dumpall -c -U postgres > backup_$(Get-Date -Format "yyyyMMdd").sql
```

## 🚨 Solución de Problemas

### Error: Puerto en uso
```powershell
# Verificar qué proceso está usando el puerto 5000
netstat -ano | findstr :5000

# O usar este comando para matar el proceso
taskkill /PID <PID> /F
```

### Error: Docker no responde
```powershell
# Reiniciar el servicio de Docker
Restart-Service docker

# O reiniciar Docker Desktop
& "C:\Program Files\Docker\Docker\Docker Desktop.exe" --restart
```

## 📞 Soporte

### Canales de Ayuda
- **Documentación**: [docs.qopiq.com](https://docs.qopiq.com)
- **Soporte Técnico**: soporte@qopiq.com
- **Emergencias**: +1 (555) 123-4567

### Horario de Soporte
- **Lunes a Viernes**: 9:00 AM - 6:00 PM (EST)
- **Emergencias**: 24/7

## 📄 Licencia
Este proyecto está bajo la licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.
