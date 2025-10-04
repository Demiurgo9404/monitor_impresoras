# ADR: Arquitectura Multi-Tenant para QOPIQ

## Estado
Aceptado

## Contexto
QOPIQ necesita soportar múltiples empresas de renta de impresoras, cada una con aislamiento completo de datos y configuración independiente.

## Decisión

### Estrategia: Shared Database, Shared Schema con Row-Level Security
- **Identificación**: Header HTTP `X-Tenant-Id`
- **Filtrado**: Query Filters automáticos en Entity Framework Core
- **Fallback**: Error 403 Forbidden si tenant inválido o ausente

### Componentes Principales:
1. **TenantResolver**: Extrae tenant del header HTTP
2. **TenantAccessor**: Proporciona acceso al tenant en toda la aplicación
3. **TenantMiddleware**: Middleware que resuelve y valida tenant
4. **Query Filters**: Filtros automáticos en DbContext

### Flujo de Petición:
```
Request → TenantMiddleware → TenantResolver → TenantAccessor → DbContext (filtered)
```

## Consecuencias

### Positivas:
- ✅ Aislamiento completo de datos por tenant
- ✅ Configuración simple (un solo header)
- ✅ Performance óptima (shared resources)
- ✅ Escalabilidad horizontal

### Negativas:
- ⚠️ Requiere disciplina en queries (siempre filtrar por tenant)
- ⚠️ Backup/restore más complejo
- ⚠️ Debugging puede ser más difícil

## Implementación

### Headers Requeridos:
```http
X-Tenant-Id: contoso
```

### Respuestas de Error:
- `403 Forbidden`: Tenant inválido o ausente
- `404 Not Found`: Recurso no existe en el tenant

### Pruebas Unitarias:
- [ ] TenantResolver extrae header correctamente
- [ ] TenantAccessor mantiene estado del tenant
- [ ] Middleware valida tenant existente
- [ ] Query filters aplican automáticamente
- [ ] Error 403 cuando tenant inválido

## Alternativas Consideradas

1. **Database per Tenant**: Descartado por complejidad operacional
2. **Schema per Tenant**: Descartado por limitaciones de EF Core
3. **Subdomain Strategy**: Descartado por simplicidad del header

## Fecha
2025-10-03

## Revisores
- Equipo QOPIQ
