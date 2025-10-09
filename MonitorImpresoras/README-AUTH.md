# 🔐 **QOPIQ - Autenticación Multi-Tenant**

## 🎯 **Guía Completa de Autenticación JWT**

Esta guía te mostrará cómo usar la autenticación multi-tenant de QOPIQ paso a paso.

---

## 📋 **Usuarios de Ejemplo**

| Tenant | Email | Password | Rol | Permisos |
|--------|-------|----------|-----|----------|
| `demo` | `admin@demo.com` | `Password123!` | CompanyAdmin | Crear/Editar empresas y proyectos |
| `demo` | `user@demo.com` | `Password123!` | Viewer | Solo lectura |
| `contoso` | `admin@contoso.com` | `Password123!` | CompanyAdmin | Crear/Editar empresas y proyectos |
| `contoso` | `user@contoso.com` | `Password123!` | Viewer | Solo lectura |

---

## 🚀 **Flujo de Autenticación Completo**

### **Paso 1: Login**

```http
POST /api/auth/login
Content-Type: application/json
X-Tenant-Id: demo

{
  "email": "admin@demo.com",
  "password": "Password123!"
}
```

**Respuesta exitosa:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64RefreshToken==",
  "expiresAt": "2025-10-04T14:30:00Z",
  "user": {
    "id": "user-guid",
    "email": "admin@demo.com",
    "firstName": "Admin",
    "lastName": "User",
    "fullName": "Admin User",
    "role": "CompanyAdmin",
    "permissions": [
      "read:printers",
      "write:printers",
      "delete:printers",
      "read:projects",
      "write:projects",
      "delete:projects",
      "read:companies",
      "write:companies",
      "read:users",
      "write:users",
      "read:reports",
      "write:reports",
      "delete:reports"
    ],
    "companyId": "company-guid",
    "companyName": "Demo Company - Main Office"
  },
  "tenant": {
    "tenantId": "demo",
    "name": "Demo Company",
    "companyName": "Demo Rentals Inc.",
    "isActive": true,
    "tier": "Free",
    "maxPrinters": 5,
    "maxUsers": 3
  }
}
```

### **Paso 2: Usar el Token**

Para todas las peticiones posteriores, incluye:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Tenant-Id: demo
```

---

## 🏢 **Ejemplos de CRUD - Empresas**

### **Crear Empresa (Solo Admin)**

```http
POST /api/company
Authorization: Bearer {token}
X-Tenant-Id: demo
Content-Type: application/json

{
  "name": "Nueva Empresa de Renta",
  "taxId": "RFC123456789",
  "address": "Av. Reforma 123",
  "city": "Ciudad de México",
  "state": "CDMX",
  "postalCode": "01000",
  "country": "Mexico",
  "phone": "+52-55-1234-5678",
  "email": "contacto@nuevaempresa.com",
  "contactPerson": "Juan Pérez",
  "maxPrinters": 50,
  "maxUsers": 20,
  "maxProjects": 10,
  "subscriptionPlan": "Professional"
}
```

### **Obtener Empresas (Todos los roles)**

```http
GET /api/company?pageNumber=1&pageSize=10&searchTerm=Nueva
Authorization: Bearer {token}
X-Tenant-Id: demo
```

### **Obtener Empresa por ID**

```http
GET /api/company/{company-id}
Authorization: Bearer {token}
X-Tenant-Id: demo
```

### **Actualizar Empresa (Solo Admin)**

```http
PUT /api/company/{company-id}
Authorization: Bearer {token}
X-Tenant-Id: demo
Content-Type: application/json

{
  "name": "Empresa Actualizada",
  "taxId": "RFC123456789",
  "address": "Nueva Dirección 456",
  "city": "Guadalajara",
  "state": "Jalisco",
  "postalCode": "44100",
  "country": "Mexico",
  "phone": "+52-33-9876-5432",
  "email": "nuevo@empresa.com",
  "contactPerson": "María García",
  "isActive": true,
  "maxPrinters": 75,
  "maxUsers": 30,
  "maxProjects": 15,
  "subscriptionPlan": "Enterprise"
}
```

---

## 📂 **Ejemplos de CRUD - Proyectos**

### **Crear Proyecto (Admin y ProjectManager)**

```http
POST /api/project
Authorization: Bearer {token}
X-Tenant-Id: demo
Content-Type: application/json

{
  "name": "Proyecto Oficina Central",
  "companyId": "company-guid-here",
  "description": "Monitoreo de impresoras en oficina central",
  "clientName": "Cliente Importante S.A.",
  "address": "Polanco 789",
  "city": "Ciudad de México",
  "state": "CDMX",
  "postalCode": "11560",
  "contactPerson": "Ana López",
  "contactPhone": "+52-55-5555-1234",
  "contactEmail": "ana@cliente.com",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-12-31T23:59:59Z",
  "monitoringIntervalMinutes": 5,
  "enableRealTimeAlerts": true,
  "enableAutomaticReports": true,
  "notes": "Proyecto crítico con SLA de 99.9%"
}
```

### **Obtener Proyectos con Filtros**

```http
GET /api/project?pageNumber=1&pageSize=10&companyId={company-id}&status=Active&searchTerm=Oficina
Authorization: Bearer {token}
X-Tenant-Id: demo
```

### **Asignar Usuario a Proyecto (Admin y ProjectManager)**

```http
POST /api/project/{project-id}/users
Authorization: Bearer {token}
X-Tenant-Id: demo
Content-Type: application/json

{
  "userId": "user-guid-here",
  "role": "Technician",
  "canManagePrinters": true,
  "canViewReports": true,
  "canGenerateReports": false,
  "canManageUsers": false
}
```

---

## 🔑 **Estructura del JWT Token**

### **Claims Incluidos:**

```json
{
  "sub": "user-guid",
  "email": "admin@demo.com",
  "name": "Admin User",
  "tenantId": "demo",
  "companyId": "company-guid",
  "role": "CompanyAdmin",
  "permissions": "read:printers,write:printers,delete:printers,...",
  "fullName": "Admin User",
  "jti": "token-unique-id",
  "iat": 1696348800,
  "exp": 1696435200,
  "iss": "QopiqAPI",
  "aud": "QopiqClient"
}
```

---

## 👥 **Roles y Permisos**

### **SuperAdmin**
- ✅ Acceso global a todos los tenants
- ✅ Gestión completa de empresas, proyectos, usuarios
- ✅ Eliminación de recursos
- ✅ Configuración del sistema

### **CompanyAdmin**
- ✅ Gestión completa dentro de su empresa
- ✅ Crear/editar proyectos y usuarios
- ✅ Ver todos los reportes
- ❌ No puede eliminar empresas

### **ProjectManager**
- ✅ Gestión de proyectos asignados
- ✅ Asignar usuarios a proyectos
- ✅ Crear/editar impresoras
- ❌ No puede crear empresas

### **Technician**
- ✅ Gestión de impresoras
- ✅ Ver proyectos asignados
- ✅ Ver reportes
- ❌ No puede gestionar usuarios

### **Viewer**
- ✅ Solo lectura de recursos asignados
- ❌ No puede crear/editar/eliminar

---

## 🧪 **Testing con Swagger**

### **1. Abrir Swagger UI**
```
http://localhost:5278/swagger
```

### **2. Autenticar con Tenant**
1. Click en **"Authorize"**
2. En **"Tenant"**: Agregar `demo`
3. En **"Bearer"**: Dejar vacío por ahora

### **3. Hacer Login**
1. Expandir **POST /api/auth/login**
2. Click **"Try it out"**
3. Usar el JSON de ejemplo:
```json
{
  "email": "admin@demo.com",
  "password": "Password123!"
}
```
4. Click **"Execute"**
5. Copiar el `token` de la respuesta

### **4. Configurar Token**
1. Click en **"Authorize"** nuevamente
2. En **"Bearer"**: Pegar `Bearer {token-copiado}`
3. Click **"Authorize"**

### **5. Probar Endpoints**
Ahora puedes probar todos los endpoints protegidos.

---

## 🐛 **Troubleshooting**

### **Error 403 - Forbidden**
- ✅ Verificar header `X-Tenant-Id`
- ✅ Verificar que el token pertenezca al tenant correcto
- ✅ Verificar permisos del rol

### **Error 401 - Unauthorized**
- ✅ Verificar que el token esté incluido
- ✅ Verificar que el token no haya expirado
- ✅ Verificar formato: `Bearer {token}`

### **Error 400 - Bad Request**
- ✅ Verificar formato JSON
- ✅ Verificar campos requeridos
- ✅ Verificar tipos de datos

---

## 📝 **Ejemplos con cURL**

### **Login**
```bash
curl -X POST "http://localhost:5278/api/auth/login" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: demo" \
  -d '{
    "email": "admin@demo.com",
    "password": "Password123!"
  }'
```

### **Crear Empresa**
```bash
curl -X POST "http://localhost:5278/api/company" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: demo" \
  -H "Authorization: Bearer {tu-token-aqui}" \
  -d '{
    "name": "Mi Nueva Empresa",
    "email": "contacto@miempresa.com",
    "contactPerson": "Director General",
    "maxPrinters": 25,
    "maxUsers": 15,
    "maxProjects": 8
  }'
```

### **Obtener Proyectos**
```bash
curl -X GET "http://localhost:5278/api/project/my-projects" \
  -H "X-Tenant-Id: demo" \
  -H "Authorization: Bearer {tu-token-aqui}"
```

---

## 🎯 **Próximos Pasos**

1. **Día 3**: Sistema de reportes automatizados
2. **Día 4**: Frontend multi-tenant
3. **Día 5**: PrinterAgent avanzado
4. **Día 6**: Despliegue y testing

**¡El sistema de autenticación multi-tenant está completamente funcional! 🚀**

