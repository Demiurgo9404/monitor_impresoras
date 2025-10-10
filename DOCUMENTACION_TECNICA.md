# 📚 Documentación Técnica QOPIQ

## 🏗️ Arquitectura del Sistema

### Estructura del Proyecto
```
QOPIQ/
├── API/                  # Capa de presentación (Web API)
├── Application/         # Lógica de negocio y casos de uso
├── Domain/              # Entidades y contratos
├── Infrastructure/      # Implementaciones concretas
└── Tests/               # Pruebas unitarias y de integración
```

### Tecnologías Clave
- **Backend**: .NET 6.0
- **Base de Datos**: PostgreSQL 14
- **Autenticación**: JWT + Identity
- **Logging**: Serilog
- **Documentación**: Swagger/OpenAPI
- **Contenedores**: Docker + Docker Compose

## 🔐 Seguridad

### Autenticación JWT
```csharp
// Configuración en Program.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Configuration["Jwt:Issuer"],
            ValidAudience = Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
        };
    });
```

### Políticas de Acceso
```csharp
// En Program.cs
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("EditPrinterPolicy", policy =>
        policy.RequireClaim("permission", "printers.edit"));
});
```

## 🌐 API Endpoints

### Impresoras
- `GET /api/printers` - Listar todas las impresoras
- `POST /api/printers` - Crear nueva impresora
- `GET /api/printers/{id}` - Obtener impresora por ID
- `PUT /api/printers/{id}` - Actualizar impresora
- `DELETE /api/printers/{id}` - Eliminar impresora
- `GET /api/printers/{id}/status` - Obtener estado

## 🚀 Despliegue

### Requisitos
- Docker 20.10+
- Docker Compose 1.29+
- 2GB RAM mínimo

### Variables de Entorno
```ini
# Base de datos
ConnectionStrings__DefaultConnection=Host=db;Database=qopiq;Username=postgres;Password=YourStrongPass123!

# JWT
JWT__Key=your-256-bit-secret
JWT__Issuer=QOPIQ-API
JWT__Audience=QOPIQ-Client

# Configuración general
ASPNETCORE_ENVIRONMENT=Production
```

### Comandos de Despliegue
```bash
# Construir y levantar contenedores
docker-compose up -d --build

# Ver logs
docker-compose logs -f

# Ejecutar migraciones
docker-compose exec api dotnet ef database update
```

## 🧪 Pruebas

### Ejecutar Pruebas
```bash
# Todas las pruebas
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Generar reporte HTML
reportgenerator -reports:TestResults/*/coverage.cobertura.xml -targetdir:coverage
```

## 📊 Monitoreo

### Health Checks
- `GET /health` - Estado de salud de la API
- `GET /health/db` - Estado de la base de datos
- `GET /health/disk` - Uso de disco

### Métricas
- `GET /metrics` - Métricas en formato Prometheus
- `GET /debug/metrics` - Métricas detalladas

## 🔄 CI/CD

### GitHub Actions
```yaml
name: Build and Deploy

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '6.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"
```

## 📞 Soporte

### Canales de Soporte
- **Soporte Técnico**: soporte@qopiq.com
- **Emergencias**: +1 (555) 123-4567
- **Documentación**: [docs.qopiq.com](https://docs.qopiq.com)
- **Estado del Servicio**: [status.qopiq.com](https://status.qopiq.com)

### Horario de Soporte
- **Lunes a Viernes**: 9:00 AM - 6:00 PM (EST)
- **Emergencias**: 24/7

## 📝 Licencia
Este proyecto está bajo la licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.
