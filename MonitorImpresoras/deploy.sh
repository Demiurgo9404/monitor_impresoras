#!/bin/bash

# Script de despliegue para Monitor de Impresoras
# Uso: ./deploy.sh [production|staging|development]

set -e  # Salir en caso de error

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuración
ENVIRONMENT=${1:-production}
PROJECT_NAME="monitor-impresoras"
BACKUP_DIR="/opt/backups/${PROJECT_NAME}"
DEPLOY_DIR="/opt/${PROJECT_NAME}"

echo -e "${BLUE}🚀 Iniciando despliegue de Monitor de Impresoras${NC}"
echo -e "${BLUE}Entorno: ${ENVIRONMENT}${NC}"
echo -e "${BLUE}Fecha: $(date)${NC}"
echo ""

# Función para logging
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

error() {
    echo -e "${RED}[ERROR] $1${NC}"
    exit 1
}

warning() {
    echo -e "${YELLOW}[WARNING] $1${NC}"
}

# Verificar que se ejecuta como root
if [[ $EUID -ne 0 ]]; then
   error "Este script debe ejecutarse como root (sudo)"
fi

# Verificar dependencias
log "Verificando dependencias..."
command -v docker >/dev/null 2>&1 || error "Docker no está instalado"
command -v docker-compose >/dev/null 2>&1 || error "Docker Compose no está instalado"
command -v git >/dev/null 2>&1 || error "Git no está instalado"

# Crear directorios necesarios
log "Creando estructura de directorios..."
mkdir -p ${DEPLOY_DIR}
mkdir -p ${BACKUP_DIR}
mkdir -p /var/log/${PROJECT_NAME}
mkdir -p /etc/ssl/${PROJECT_NAME}

# Backup de la configuración actual (si existe)
if [ -f "${DEPLOY_DIR}/.env" ]; then
    log "Creando backup de configuración..."
    cp ${DEPLOY_DIR}/.env ${BACKUP_DIR}/.env.$(date +%Y%m%d_%H%M%S)
fi

# Backup de la base de datos (si existe)
if docker ps | grep -q "${PROJECT_NAME}-db"; then
    log "Creando backup de base de datos..."
    docker exec ${PROJECT_NAME}-db pg_dump -U postgres monitor_impresoras > ${BACKUP_DIR}/db_backup_$(date +%Y%m%d_%H%M%S).sql
fi

# Clonar o actualizar código
log "Actualizando código fuente..."
if [ -d "${DEPLOY_DIR}/.git" ]; then
    cd ${DEPLOY_DIR}
    git fetch origin
    git reset --hard origin/main
else
    git clone https://github.com/tu-usuario/monitor-impresoras.git ${DEPLOY_DIR}
    cd ${DEPLOY_DIR}
fi

# Configurar variables de entorno
log "Configurando variables de entorno..."
if [ ! -f ".env" ]; then
    if [ -f ".env.example" ]; then
        cp .env.example .env
        warning "Se creó .env desde .env.example. DEBES configurar las variables antes de continuar."
        warning "Edita el archivo .env con tus configuraciones específicas."
        read -p "¿Has configurado el archivo .env? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            error "Configura el archivo .env antes de continuar"
        fi
    else
        error "No se encontró archivo .env ni .env.example"
    fi
fi

# Generar secretos si no existen
log "Generando secretos..."
if ! grep -q "JWT_SECRET_KEY=your-super-secret" .env; then
    JWT_SECRET=$(openssl rand -base64 32)
    sed -i "s/JWT_SECRET_KEY=.*/JWT_SECRET_KEY=${JWT_SECRET}/" .env
    log "JWT Secret generado automáticamente"
fi

# Configurar SSL (Let's Encrypt)
setup_ssl() {
    log "Configurando SSL con Let's Encrypt..."
    
    # Instalar certbot si no existe
    if ! command -v certbot &> /dev/null; then
        apt-get update
        apt-get install -y certbot python3-certbot-nginx
    fi
    
    # Obtener dominio del .env
    DOMAIN=$(grep FRONTEND_URL .env | cut -d'=' -f2 | sed 's|https://||' | sed 's|http://||')
    
    if [ ! -z "$DOMAIN" ] && [ "$DOMAIN" != "localhost" ]; then
        log "Obteniendo certificado SSL para ${DOMAIN}..."
        certbot certonly --standalone --non-interactive --agree-tos --email admin@${DOMAIN} -d ${DOMAIN}
        
        # Copiar certificados
        cp /etc/letsencrypt/live/${DOMAIN}/fullchain.pem /etc/ssl/${PROJECT_NAME}/certificate.crt
        cp /etc/letsencrypt/live/${DOMAIN}/privkey.pem /etc/ssl/${PROJECT_NAME}/private.key
        
        # Configurar renovación automática
        (crontab -l 2>/dev/null; echo "0 12 * * * /usr/bin/certbot renew --quiet --post-hook 'docker-compose -f ${DEPLOY_DIR}/docker-compose.yml restart nginx'") | crontab -
    else
        warning "Dominio no configurado o es localhost. Usando certificados auto-firmados."
        # Generar certificados auto-firmados
        openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
            -keyout /etc/ssl/${PROJECT_NAME}/private.key \
            -out /etc/ssl/${PROJECT_NAME}/certificate.crt \
            -subj "/C=MX/ST=CDMX/L=Mexico/O=MonitorImpresoras/CN=localhost"
    fi
}

# Configurar SSL
setup_ssl

# Actualizar configuración de nginx con rutas SSL
log "Configurando Nginx..."
sed -i "s|/etc/nginx/ssl/certificate.crt|/etc/ssl/${PROJECT_NAME}/certificate.crt|g" nginx.conf
sed -i "s|/etc/nginx/ssl/private.key|/etc/ssl/${PROJECT_NAME}/private.key|g" nginx.conf

# Detener servicios existentes
log "Deteniendo servicios existentes..."
docker-compose down --remove-orphans || true

# Limpiar imágenes antiguas
log "Limpiando imágenes Docker antiguas..."
docker system prune -f

# Construir y desplegar
log "Construyendo y desplegando aplicación..."
docker-compose build --no-cache
docker-compose up -d

# Esperar a que los servicios estén listos
log "Esperando a que los servicios estén listos..."
sleep 30

# Verificar que los servicios están funcionando
log "Verificando servicios..."
if docker-compose ps | grep -q "Up"; then
    log "✅ Servicios desplegados correctamente"
else
    error "❌ Algunos servicios no se iniciaron correctamente"
fi

# Ejecutar migraciones de base de datos
log "Ejecutando migraciones de base de datos..."
docker-compose exec -T api dotnet ef database update || warning "No se pudieron ejecutar las migraciones automáticamente"

# Verificar endpoints
log "Verificando endpoints..."
sleep 10

# Health check
if curl -f -s http://localhost:5278/health > /dev/null; then
    log "✅ API Health Check: OK"
else
    warning "❌ API Health Check: FAILED"
fi

# API Test
if curl -f -s http://localhost:5278/api/printers > /dev/null; then
    log "✅ API Printers Endpoint: OK"
else
    warning "❌ API Printers Endpoint: FAILED"
fi

# Configurar logrotate
log "Configurando rotación de logs..."
cat > /etc/logrotate.d/${PROJECT_NAME} << EOF
/var/log/${PROJECT_NAME}/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 root root
    postrotate
        docker-compose -f ${DEPLOY_DIR}/docker-compose.yml restart api
    endscript
}
EOF

# Configurar monitoreo básico
log "Configurando monitoreo..."
cat > /usr/local/bin/monitor-${PROJECT_NAME} << 'EOF'
#!/bin/bash
cd /opt/monitor-impresoras
if ! docker-compose ps | grep -q "Up"; then
    echo "$(date): Servicios caídos, reiniciando..." >> /var/log/monitor-impresoras/monitor.log
    docker-compose up -d
fi
EOF

chmod +x /usr/local/bin/monitor-${PROJECT_NAME}

# Agregar a crontab para monitoreo cada 5 minutos
(crontab -l 2>/dev/null; echo "*/5 * * * * /usr/local/bin/monitor-${PROJECT_NAME}") | crontab -

# Configurar firewall
log "Configurando firewall..."
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 22/tcp
ufw --force enable

# Mostrar información final
log "🎉 Despliegue completado exitosamente!"
echo ""
echo -e "${GREEN}📊 Información del despliegue:${NC}"
echo -e "${BLUE}• Entorno: ${ENVIRONMENT}${NC}"
echo -e "${BLUE}• Directorio: ${DEPLOY_DIR}${NC}"
echo -e "${BLUE}• Logs: /var/log/${PROJECT_NAME}/${NC}"
echo -e "${BLUE}• Backups: ${BACKUP_DIR}${NC}"
echo ""
echo -e "${GREEN}🌐 URLs de acceso:${NC}"
echo -e "${BLUE}• API: http://$(hostname -I | awk '{print $1}'):5278${NC}"
echo -e "${BLUE}• Health: http://$(hostname -I | awk '{print $1}'):5278/health${NC}"
echo -e "${BLUE}• Swagger: http://$(hostname -I | awk '{print $1}'):5278/swagger${NC}"
echo ""
echo -e "${GREEN}🔧 Comandos útiles:${NC}"
echo -e "${BLUE}• Ver logs: docker-compose logs -f${NC}"
echo -e "${BLUE}• Reiniciar: docker-compose restart${NC}"
echo -e "${BLUE}• Estado: docker-compose ps${NC}"
echo -e "${BLUE}• Backup DB: docker exec monitor-impresoras-db pg_dump -U postgres monitor_impresoras > backup.sql${NC}"
echo ""
echo -e "${YELLOW}⚠️  Recuerda:${NC}"
echo -e "${YELLOW}• Configurar DNS para apuntar a este servidor${NC}"
echo -e "${YELLOW}• Revisar y personalizar el archivo .env${NC}"
echo -e "${YELLOW}• Configurar backups automáticos${NC}"
echo -e "${YELLOW}• Monitorear logs regularmente${NC}"
echo ""
log "✅ Despliegue finalizado - $(date)"
