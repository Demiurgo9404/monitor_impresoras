# ADR: JWT Multi-Tenant para QOPIQ

## Estado
Aceptado

## Contexto
Necesitamos autenticación que incluya información del tenant y roles para autorización granular en el sistema multi-tenant.

## Decisión

### Estructura del JWT Payload:
```json
{
  "sub": "user-guid",
  "email": "user@company.com",
  "name": "John Doe",
  "tenantId": "contoso",
  "companyId": "company-guid",
  "role": "Admin",
  "permissions": ["read:printers", "write:projects"],
  "iat": 1696348800,
  "exp": 1696435200,
  "iss": "QopiqAPI",
  "aud": "QopiqClient"
}
```

### Roles del Sistema:
- **SuperAdmin**: Acceso global (gestión de tenants)
- **CompanyAdmin**: Administrador de empresa (todos los proyectos)
- **ProjectManager**: Gestor de proyecto específico
- **Technician**: Técnico con acceso a impresoras
- **Viewer**: Solo lectura

### Claims Personalizados:
- `tenantId`: ID del tenant actual
- `companyId`: ID de la empresa del usuario
- `role`: Rol principal del usuario
- `permissions`: Array de permisos específicos

## Flujo de Autenticación:
```
1. POST /api/auth/login + X-Tenant-Id header
2. Validar credenciales + tenant
3. Generar JWT con claims del tenant
4. Cliente incluye JWT + X-Tenant-Id en requests
5. Middleware valida ambos coincidan
```

## Validaciones:
- ✅ TenantId en JWT debe coincidir con header
- ✅ Usuario debe pertenecer al tenant
- ✅ Token debe estar activo y no expirado
- ✅ Permisos deben coincidir con la acción

## Consecuencias

### Positivas:
- ✅ Seguridad robusta multi-tenant
- ✅ Autorización granular por roles
- ✅ Trazabilidad completa de acciones
- ✅ Escalabilidad horizontal

### Negativas:
- ⚠️ Tokens más grandes (más claims)
- ⚠️ Validación más compleja
- ⚠️ Gestión de permisos más elaborada

## Implementación

### Endpoints de Autenticación:
- `POST /api/auth/login` - Login con tenant
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/refresh` - Renovar token
- `POST /api/auth/logout` - Cerrar sesión

### Headers Requeridos:
```http
X-Tenant-Id: contoso
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Pruebas de Autorización:
- [ ] Login exitoso devuelve JWT válido
- [ ] Token inválido devuelve 401
- [ ] TenantId incorrecto devuelve 403
- [ ] Roles aplicados correctamente
- [ ] Permisos validados en endpoints

## Fecha
2025-10-03

## Revisores
- Equipo QOPIQ
