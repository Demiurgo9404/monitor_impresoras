# Monitor de Impresoras API - Servicios Simplificados

## 📋 Descripción General

Este proyecto implementa un sistema de monitoreo de impresoras con servicios simplificados y endpoints de API RESTful. Los servicios están diseñados para ser fáciles de usar y mantener, con implementaciones robustas que incluyen logging, manejo de errores y documentación completa.

## 🚀 Servicios Implementados

### 1. **PrinterService**
- **Obtener lista de impresoras**: `GET /api/services/printers`
- **Obtener estado de una impresora**: `GET /api/services/printers/{printerId}/status`

### 2. **ConsumableService**
- **Verificar consumibles de una impresora**: `POST /api/services/consumables/check/{printerId}`

### 3. **AlertService**
- **Enviar alerta de prueba**: `POST /api/services/alerts/test`

### 4. **AlertEngineService**
- **Procesar todas las alertas del sistema**: `POST /api/services/alerts/process`

## 📚 Documentación de API

### Endpoints Disponibles

#### 🔧 Gestión de Impresoras

##### Obtener Lista de Impresoras
```http
GET /api/services/printers
Authorization: Bearer {token}
```

**Respuesta exitosa (200):**
```json
[
  "HP LaserJet Pro MFP",
  "Epson WorkForce Pro",
  "Canon PIXMA TR"
]
```

##### Obtener Estado de una Impresora
```http
GET /api/services/printers/{printerId}/status
Authorization: Bearer {token}
```

**Parámetros:**
- `printerId` (GUID): ID único de la impresora

**Respuesta exitosa (200):**
```json
{
  "printerId": "12345678-1234-1234-1234-123456789abc",
  "status": "Online"
}
```

**Códigos de error:**
- `404`: Impresora no encontrada
- `400`: ID de impresora inválido
- `500`: Error interno del servidor

#### ⚠️ Gestión de Alertas

##### Enviar Alerta de Prueba
```http
POST /api/services/alerts/test
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "La impresora HP LaserJet está experimentando problemas de conexión",
  "severity": "High"
}
```

**Cuerpo de la solicitud:**
```json
{
  "message": "string (required)",
  "severity": "string (optional: Low/Medium/High, default: Info)"
}
```

**Respuesta exitosa (200):**
```json
{
  "message": "Alert sent successfully"
}
```

##### Procesar Alertas del Sistema
```http
POST /api/services/alerts/process
Authorization: Bearer {token}
```

**Respuesta exitosa (200):**
```json
{
  "message": "Alert processing completed"
}
```

**Descripción:** Este endpoint ejecuta todas las reglas de alertas activas del sistema:
- Detección de impresoras offline
- Verificación de consumibles bajos
- Detección de errores en impresoras

#### 🖨️ Gestión de Consumibles

##### Verificar Consumibles de una Impresora
```http
POST /api/services/consumables/check/{printerId}
Authorization: Bearer {token}
```

**Parámetros:**
- `printerId` (GUID): ID único de la impresora

**Respuesta exitosa (200):**
```json
{
  "message": "Consumables check completed for printer 12345678-1234-1234-1234-123456789abc"
}
```

**Códigos de error:**
- `404`: Impresora no encontrada
- `400`: ID de impresora inválido
- `500`: Error interno del servidor

## 🔐 Autenticación y Autorización

Todos los endpoints requieren autenticación JWT. Los roles soportados son:
- **Admin**: Acceso completo a todos los endpoints
- **Technician**: Acceso a endpoints de monitoreo y alertas
- **User**: Acceso limitado a consultas básicas

### Obtener Token
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}
```

## 🚀 Inicio Rápido

### 1. Compilar el Proyecto
```bash
dotnet clean
dotnet build
```

### 2. Ejecutar la Aplicación
```bash
cd MonitorImpresoras.API
dotnet run
```

### 3. Acceder a Swagger UI
Abre tu navegador y ve a: `https://localhost:5001/swagger`

### 4. Probar los Endpoints

#### Ejemplo 1: Obtener Lista de Impresoras
```bash
curl -X GET "https://localhost:5001/api/services/printers" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Ejemplo 2: Enviar Alerta de Prueba
```bash
curl -X POST "https://localhost:5001/api/services/alerts/test" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Alerta de prueba desde API",
    "severity": "Medium"
  }'
```

#### Ejemplo 3: Verificar Consumibles
```bash
curl -X POST "https://localhost:5001/api/services/consumables/check/12345678-1234-1234-1234-123456789abc" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📊 Monitoreo y Logs

Los servicios incluyen logging estructurado:

- **Información**: Operaciones exitosas
- **Advertencias**: Consumibles bajos, impresoras offline
- **Errores**: Fallos en operaciones críticas

### Ver Logs
```bash
# En desarrollo
tail -f logs/log-{date}.txt

# Usando dotnet logs
dotnet run --logger:console
```

## 🛠️ Configuración

### Variables de Entorno
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=MonitorImpresoras;Trusted_Connection=True;
Jwt__Secret=your-super-secret-jwt-key-here
Jwt__Issuer=MonitorImpresoras.API
Jwt__Audience=MonitorImpresoras.Client
```

## 🔧 Arquitectura

```
├── MonitorImpresoras.API/
│   ├── Controllers/
│   │   └── ServicesController.cs     # Endpoints principales
│   ├── Program.cs                     # Configuración DI y middleware
│   └── appsettings.json               # Configuración
├── MonitorImpresoras.Application/
│   ├── Services/
│   │   ├── Interfaces/                # Interfaces de servicios
│   │   ├── PrinterService.cs          # Lógica de impresoras
│   │   ├── ConsumableService.cs       # Lógica de consumibles
│   │   ├── AlertService.cs            # Lógica de alertas
│   │   └── AlertEngineService.cs      # Motor de alertas
│   └── DTOs/                          # Data Transfer Objects
└── MonitorImpresoras.Infrastructure/
    ├── Data/                          # Contexto de BD y migraciones
    └── Repositories/                  # Repositorios de datos
```

## 🧪 Testing

### Ejecutar Tests
```bash
dotnet test
```

### Tests Disponibles
- Tests unitarios para cada servicio
- Tests de integración para endpoints
- Tests de carga para servicios críticos

## 📈 Métricas y Salud

### Health Check
```http
GET /health
```

### Métricas
```http
GET /metrics
```

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Notas de Desarrollo

- Los servicios están diseñados para ser **extensibles** y **mantenibles**
- Se implementa **inyección de dependencias** para facilitar testing
- **Logging estructurado** para debugging y monitoreo
- **Manejo robusto de errores** con códigos HTTP apropiados
- **Documentación completa** con Swagger/OpenAPI

## 📞 Soporte

Para soporte técnico o preguntas:
- Crear un issue en GitHub
- Contactar al equipo de desarrollo
- Revisar la documentación en `/docs`

---

**Desarrollado con ❤️ para el monitoreo eficiente de impresoras**
