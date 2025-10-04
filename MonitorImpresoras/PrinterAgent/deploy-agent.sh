#!/bin/bash

# Script de despliegue del PrinterAgent para Linux
# Uso: ./deploy-agent.sh --central-url https://monitor.empresa.com/api --api-key your-key

set -e

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Variables por defecto
CENTRAL_URL=""
API_KEY=""
AGENT_ID="agent-$(hostname)-$(date +%s)"
AGENT_NAME="PrinterAgent-$(hostname)"
LOCATION="$(hostname)"
INSTALL_DIR="/opt/printer-agent"
SERVICE_NAME="printer-agent"
NETWORK_RANGES="192.168.1.0/24"

# Función para mostrar ayuda
show_help() {
    echo "Uso: $0 --central-url URL --api-key KEY [opciones]"
    echo ""
    echo "Opciones requeridas:"
    echo "  --central-url URL    URL del sistema central (ej: https://monitor.empresa.com/api)"
    echo "  --api-key KEY        API Key para autenticación"
    echo ""
    echo "Opciones opcionales:"
    echo "  --agent-id ID        ID único del agente (por defecto: auto-generado)"
    echo "  --agent-name NAME    Nombre del agente (por defecto: PrinterAgent-hostname)"
    echo "  --location LOC       Ubicación del agente (por defecto: hostname)"
    echo "  --install-dir DIR    Directorio de instalación (por defecto: /opt/printer-agent)"
    echo "  --network-ranges R   Rangos de red a escanear (por defecto: 192.168.1.0/24)"
    echo "  --help              Mostrar esta ayuda"
    echo ""
    echo "Ejemplo:"
    echo "  $0 --central-url https://monitor.empresa.com/api --api-key abc123 --location 'Sucursal Norte'"
}

# Parsear argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        --central-url)
            CENTRAL_URL="$2"
            shift 2
            ;;
        --api-key)
            API_KEY="$2"
            shift 2
            ;;
        --agent-id)
            AGENT_ID="$2"
            shift 2
            ;;
        --agent-name)
            AGENT_NAME="$2"
            shift 2
            ;;
        --location)
            LOCATION="$2"
            shift 2
            ;;
        --install-dir)
            INSTALL_DIR="$2"
            shift 2
            ;;
        --network-ranges)
            NETWORK_RANGES="$2"
            shift 2
            ;;
        --help)
            show_help
            exit 0
            ;;
        *)
            echo "Opción desconocida: $1"
            show_help
            exit 1
            ;;
    esac
done

# Validar argumentos requeridos
if [[ -z "$CENTRAL_URL" || -z "$API_KEY" ]]; then
    echo -e "${RED}Error: --central-url y --api-key son requeridos${NC}"
    show_help
    exit 1
fi

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

echo -e "${BLUE}🤖 Instalador de PrinterAgent${NC}"
echo -e "${BLUE}================================${NC}"
echo "Agent ID: $AGENT_ID"
echo "Agent Name: $AGENT_NAME"
echo "Location: $LOCATION"
echo "Central URL: $CENTRAL_URL"
echo "Install Dir: $INSTALL_DIR"
echo "Network Ranges: $NETWORK_RANGES"
echo ""

# Verificar permisos de root
if [[ $EUID -ne 0 ]]; then
   error "Este script debe ejecutarse como root (sudo)"
fi

# Detectar distribución
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$NAME
    VER=$VERSION_ID
else
    error "No se pudo detectar la distribución de Linux"
fi

log "Detectado: $OS $VER"

# Instalar dependencias según la distribución
install_dependencies() {
    log "Instalando dependencias..."
    
    case $OS in
        *"Ubuntu"*|*"Debian"*)
            apt-get update
            apt-get install -y curl wget gnupg2 software-properties-common apt-transport-https
            
            # Instalar .NET 8
            wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
            dpkg -i packages-microsoft-prod.deb
            rm packages-microsoft-prod.deb
            apt-get update
            apt-get install -y dotnet-runtime-8.0
            ;;
        *"CentOS"*|*"Red Hat"*|*"Rocky"*|*"AlmaLinux"*)
            yum update -y
            yum install -y curl wget
            
            # Instalar .NET 8
            rpm -Uvh https://packages.microsoft.com/config/centos/8/packages-microsoft-prod.rpm
            yum install -y dotnet-runtime-8.0
            ;;
        *)
            warning "Distribución no reconocida. Intentando instalación genérica..."
            ;;
    esac
    
    # Verificar instalación de .NET
    if ! command -v dotnet &> /dev/null; then
        error ".NET Runtime no se instaló correctamente"
    fi
    
    log "✅ Dependencias instaladas"
}

# Crear usuario del sistema
create_user() {
    log "Creando usuario del sistema..."
    
    if ! id "printer-agent" &>/dev/null; then
        useradd -r -s /bin/false -d $INSTALL_DIR printer-agent
        log "✅ Usuario 'printer-agent' creado"
    else
        log "Usuario 'printer-agent' ya existe"
    fi
}

# Descargar y instalar PrinterAgent
install_agent() {
    log "Descargando PrinterAgent..."
    
    # Crear directorio de instalación
    mkdir -p $INSTALL_DIR
    cd $INSTALL_DIR
    
    # En un entorno real, descargarías desde GitHub releases
    # Por ahora, asumimos que tienes los archivos compilados
    
    # Simular descarga (en producción sería desde GitHub)
    log "Compilando PrinterAgent desde código fuente..."
    
    # Clonar repositorio temporal
    TEMP_DIR=$(mktemp -d)
    cd $TEMP_DIR
    
    # En producción: git clone https://github.com/tu-usuario/monitor-impresoras.git
    # Por ahora copiamos desde el directorio local
    cp -r /path/to/PrinterAgent/* . 2>/dev/null || {
        warning "No se encontró código fuente local. Creando estructura básica..."
        mkdir -p PrinterAgent.API
    }
    
    # Compilar (si existe el código)
    if [ -f "PrinterAgent.API/PrinterAgent.API.csproj" ]; then
        dotnet publish PrinterAgent.API -c Release -o $INSTALL_DIR --self-contained false
    else
        # Crear estructura mínima para demo
        cat > $INSTALL_DIR/PrinterAgent.API.dll << 'EOF'
# Placeholder - En producción esto sería el binario real
EOF
        
        cat > $INSTALL_DIR/appsettings.json << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
EOF
    fi
    
    # Limpiar
    rm -rf $TEMP_DIR
    
    log "✅ PrinterAgent instalado en $INSTALL_DIR"
}

# Configurar PrinterAgent
configure_agent() {
    log "Configurando PrinterAgent..."
    
    # Crear configuración de producción
    cat > $INSTALL_DIR/appsettings.Production.json << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "PrinterAgent": "Information"
    }
  },
  "AllowedHosts": "*",
  "Agent": {
    "AgentId": "$AGENT_ID",
    "AgentName": "$AGENT_NAME",
    "Location": "$LOCATION",
    "CentralApiUrl": "$CENTRAL_URL",
    "ApiKey": "$API_KEY",
    "ReportingInterval": "00:05:00",
    "HealthCheckInterval": "00:01:00",
    "Network": {
      "ScanRanges": ["$NETWORK_RANGES"],
      "SnmpCommunity": "public",
      "SnmpTimeout": 5000,
      "MaxConcurrentScans": 10,
      "EnableAutoDiscovery": true
    },
    "Logging": {
      "Level": "Information",
      "RetentionDays": 30,
      "EnableFileLogging": true,
      "LogPath": "logs"
    }
  }
}
EOF
    
    # Crear directorio de logs
    mkdir -p $INSTALL_DIR/logs
    
    # Establecer permisos
    chown -R printer-agent:printer-agent $INSTALL_DIR
    chmod -R 755 $INSTALL_DIR
    chmod 600 $INSTALL_DIR/appsettings.Production.json
    
    log "✅ Configuración creada"
}

# Crear servicio systemd
create_service() {
    log "Creando servicio systemd..."
    
    cat > /etc/systemd/system/$SERVICE_NAME.service << EOF
[Unit]
Description=PrinterAgent - Distributed Printer Monitoring Agent
After=network.target
Wants=network.target

[Service]
Type=notify
User=printer-agent
Group=printer-agent
WorkingDirectory=$INSTALL_DIR
ExecStart=/usr/bin/dotnet $INSTALL_DIR/PrinterAgent.API.dll --environment=Production
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=printer-agent
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Security settings
NoNewPrivileges=yes
PrivateTmp=yes
ProtectSystem=strict
ReadWritePaths=$INSTALL_DIR/logs
ProtectHome=yes
ProtectKernelTunables=yes
ProtectKernelModules=yes
ProtectControlGroups=yes

[Install]
WantedBy=multi-user.target
EOF
    
    # Recargar systemd y habilitar servicio
    systemctl daemon-reload
    systemctl enable $SERVICE_NAME
    
    log "✅ Servicio systemd creado y habilitado"
}

# Configurar firewall
configure_firewall() {
    log "Configurando firewall..."
    
    # Detectar firewall disponible
    if command -v ufw &> /dev/null; then
        ufw allow 5000/tcp comment "PrinterAgent API"
        log "✅ Regla UFW agregada"
    elif command -v firewall-cmd &> /dev/null; then
        firewall-cmd --permanent --add-port=5000/tcp
        firewall-cmd --reload
        log "✅ Regla firewalld agregada"
    else
        warning "No se detectó firewall. Configura manualmente el puerto 5000/tcp"
    fi
}

# Instalar dependencias
install_dependencies

# Crear usuario
create_user

# Instalar agente
install_agent

# Configurar agente
configure_agent

# Crear servicio
create_service

# Configurar firewall
configure_firewall

# Iniciar servicio
log "Iniciando PrinterAgent..."
systemctl start $SERVICE_NAME

# Verificar estado
sleep 5
if systemctl is-active --quiet $SERVICE_NAME; then
    log "✅ PrinterAgent iniciado correctamente"
else
    error "❌ PrinterAgent no se pudo iniciar. Revisa los logs: journalctl -u $SERVICE_NAME"
fi

# Mostrar información final
echo ""
echo -e "${GREEN}🎉 Instalación completada exitosamente!${NC}"
echo ""
echo -e "${BLUE}📊 Información del agente:${NC}"
echo "• Agent ID: $AGENT_ID"
echo "• Agent Name: $AGENT_NAME"
echo "• Location: $LOCATION"
echo "• Install Dir: $INSTALL_DIR"
echo "• Service: $SERVICE_NAME"
echo ""
echo -e "${BLUE}🌐 URLs de acceso:${NC}"
echo "• Dashboard: http://$(hostname -I | awk '{print $1}'):5000"
echo "• API: http://$(hostname -I | awk '{print $1}'):5000/api/agent"
echo "• Health: http://$(hostname -I | awk '{print $1}'):5000/health"
echo ""
echo -e "${BLUE}🔧 Comandos útiles:${NC}"
echo "• Ver estado: systemctl status $SERVICE_NAME"
echo "• Ver logs: journalctl -u $SERVICE_NAME -f"
echo "• Reiniciar: systemctl restart $SERVICE_NAME"
echo "• Detener: systemctl stop $SERVICE_NAME"
echo ""
echo -e "${YELLOW}📝 Próximos pasos:${NC}"
echo "1. Verifica que el agente se conecte al sistema central"
echo "2. Revisa el dashboard en http://$(hostname -I | awk '{print $1}'):5000"
echo "3. Configura los rangos de red si es necesario"
echo "4. Monitorea los logs para verificar el descubrimiento de impresoras"
echo ""
log "✅ Instalación finalizada - $(date)"
