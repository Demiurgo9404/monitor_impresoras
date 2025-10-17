# 👥 QOPIQ Monitor - Manual de Usuario

## 🎯 Introducción

**QOPIQ Monitor** es un sistema avanzado de monitoreo de impresoras en tiempo real que permite a las empresas supervisar, gestionar y mantener su flota de impresoras de manera eficiente.

---

## 🚀 Primeros Pasos

### 1. Acceso al Sistema

**URL de Acceso**: `http://localhost:5000` (desarrollo) o `https://tu-dominio.com` (producción)

### 2. Inicio de Sesión

**Credenciales por Defecto:**
- **Email**: `admin@qopiq.com`
- **Contraseña**: `Admin@123`

⚠️ **Importante**: Cambiar estas credenciales en el primer acceso.

### 3. Navegación Principal

El sistema cuenta con las siguientes secciones:
- 📊 **Dashboard**: Vista general del sistema
- 🖨️ **Impresoras**: Gestión de impresoras
- 👥 **Usuarios**: Administración de usuarios
- ⚙️ **Configuración**: Ajustes del sistema

---

## 📊 Dashboard Principal

### Vista General
El dashboard proporciona una vista completa del estado de todas las impresoras:

#### Tarjetas de Resumen
- **🟢 Activas**: Impresoras funcionando correctamente
- **🟡 Con Advertencias**: Impresoras con alertas menores
- **🔴 Inactivas/Error**: Impresoras con problemas críticos
- **📊 Total**: Número total de impresoras registradas

#### Gráfico de Estado
- **Gráfico circular**: Distribución visual del estado de impresoras
- **Actualización automática**: Los datos se actualizan en tiempo real
- **Colores intuitivos**: Verde (OK), Amarillo (Advertencia), Rojo (Error)

#### Lista de Impresoras Recientes
- **Últimas actualizaciones**: Impresoras con cambios recientes
- **Estado en tiempo real**: Indicadores visuales del estado actual
- **Acceso rápido**: Enlaces directos a detalles de cada impresora

### Funciones Interactivas
- **🔄 Actualizar**: Botón para refrescar datos manualmente
- **🔍 Búsqueda**: Campo de búsqueda en tiempo real
- **📱 Responsive**: Adaptable a dispositivos móviles

---

## 🖨️ Gestión de Impresoras

### Agregar Nueva Impresora

1. **Navegar** a la sección "Impresoras"
2. **Hacer clic** en "Agregar Impresora"
3. **Completar** el formulario:
   - **Nombre**: Identificador único de la impresora
   - **Modelo**: Marca y modelo del dispositivo
   - **Dirección IP**: IP de red de la impresora
   - **Ubicación**: Localización física (opcional)
   - **Descripción**: Información adicional (opcional)

4. **Guardar** para registrar la impresora

### Editar Impresora Existente

1. **Localizar** la impresora en la lista
2. **Hacer clic** en "Ver Detalles" o "Editar"
3. **Modificar** los campos necesarios
4. **Guardar cambios**

### Estados de Impresoras

| Estado | Descripción | Color |
|--------|-------------|-------|
| **🟢 Online** | Funcionando correctamente | Verde |
| **🟡 Warning** | Advertencias menores (papel bajo, tóner bajo) | Amarillo |
| **🔴 Offline** | No responde o error crítico | Rojo |
| **⚫ Unknown** | Estado desconocido | Gris |

### Monitoreo en Tiempo Real

- **Actualización automática**: El estado se actualiza cada 30 segundos
- **Notificaciones**: Alertas visuales cuando cambia el estado
- **Historial**: Registro de cambios de estado
- **Métricas**: Estadísticas de disponibilidad y rendimiento

---

## 👥 Administración de Usuarios

### Roles de Usuario

#### **Administrador**
- ✅ Acceso completo al sistema
- ✅ Gestión de usuarios y impresoras
- ✅ Configuración del sistema
- ✅ Reportes y estadísticas

#### **Usuario**
- ✅ Visualización de impresoras
- ✅ Monitoreo en tiempo real
- ✅ Reportes básicos
- ❌ No puede modificar configuraciones

#### **Visor**
- ✅ Solo lectura del dashboard
- ✅ Visualización de estados
- ❌ No puede realizar cambios

### Gestión de Cuentas

#### Crear Nuevo Usuario
1. **Ir** a "Usuarios" → "Agregar Usuario"
2. **Completar** información:
   - Email (será el nombre de usuario)
   - Contraseña temporal
   - Rol asignado
   - Empresa (si aplica)
3. **Enviar** invitación por email

#### Modificar Usuario
- **Cambiar rol**: Actualizar permisos
- **Resetear contraseña**: Generar nueva contraseña temporal
- **Activar/Desactivar**: Habilitar o deshabilitar acceso

---

## 🔍 Búsqueda y Filtros

### Búsqueda Avanzada
- **Por nombre**: Buscar impresoras por nombre
- **Por modelo**: Filtrar por marca/modelo
- **Por IP**: Localizar por dirección IP
- **Por ubicación**: Filtrar por localización
- **Por estado**: Mostrar solo impresoras con estado específico

### Filtros Rápidos
- **Solo activas**: Mostrar únicamente impresoras online
- **Con problemas**: Filtrar impresoras con errores o advertencias
- **Recientes**: Impresoras agregadas recientemente
- **Por empresa**: Filtrar por organización (multi-tenant)

---

## 📊 Reportes y Estadísticas

### Tipos de Reportes

#### **Reporte de Estado**
- Estado actual de todas las impresoras
- Estadísticas de disponibilidad
- Tiempo de actividad/inactividad
- Exportable a PDF/Excel

#### **Reporte de Actividad**
- Historial de cambios de estado
- Eventos y alertas generadas
- Tendencias de rendimiento
- Análisis temporal

#### **Reporte de Inventario**
- Lista completa de impresoras
- Información técnica detallada
- Ubicaciones y responsables
- Estado de mantenimiento

### Exportación de Datos
- **PDF**: Reportes formateados para impresión
- **Excel**: Datos estructurados para análisis
- **CSV**: Formato simple para importación
- **Email**: Envío automático de reportes

---

## ⚙️ Configuración del Sistema

### Configuración General

#### **Intervalos de Monitoreo**
- **Frecuencia de verificación**: 30 segundos (configurable)
- **Timeout de conexión**: 5 segundos
- **Reintentos**: 3 intentos antes de marcar como offline

#### **Notificaciones**
- **Email**: Configurar SMTP para alertas
- **Tiempo real**: Notificaciones en dashboard
- **Umbrales**: Definir cuándo generar alertas

#### **Seguridad**
- **Tiempo de sesión**: Duración de tokens JWT
- **Políticas de contraseña**: Requisitos de seguridad
- **Auditoría**: Registro de acciones de usuarios

### Configuración de Empresa (Multi-tenant)

#### **Información Corporativa**
- Nombre de la empresa
- Dominio personalizado
- Logo y branding
- Configuraciones específicas

#### **Límites y Cuotas**
- Número máximo de impresoras
- Usuarios permitidos
- Espacio de almacenamiento
- Funciones habilitadas

---

## 🔔 Alertas y Notificaciones

### Tipos de Alertas

#### **Críticas** 🔴
- Impresora completamente offline
- Error de conexión persistente
- Falla de hardware detectada

#### **Advertencias** 🟡
- Nivel bajo de tóner/tinta
- Papel agotándose
- Mantenimiento requerido

#### **Informativas** 🔵
- Nueva impresora agregada
- Cambio de configuración
- Reporte generado

### Configuración de Alertas
- **Umbrales personalizables**: Definir cuándo activar alertas
- **Canales de notificación**: Email, dashboard, móvil
- **Horarios**: Configurar horarios de notificación
- **Escalamiento**: Alertas progresivas según severidad

---

## 📱 Acceso Móvil

### Características Móviles
- **Diseño responsive**: Adaptado a smartphones y tablets
- **Dashboard móvil**: Vista optimizada para pantallas pequeñas
- **Notificaciones push**: Alertas en tiempo real
- **Acceso offline**: Visualización de datos en caché

### Navegación Móvil
- **Menú hamburguesa**: Navegación compacta
- **Gestos táctiles**: Deslizar para actualizar
- **Búsqueda rápida**: Campo de búsqueda prominente
- **Acciones rápidas**: Botones de acción accesibles

---

## 🆘 Solución de Problemas

### Problemas Comunes

#### **No puedo iniciar sesión**
1. Verificar credenciales
2. Comprobar conexión a internet
3. Limpiar caché del navegador
4. Contactar al administrador

#### **Las impresoras no se actualizan**
1. Verificar conexión de red
2. Comprobar configuración de firewall
3. Validar direcciones IP
4. Reiniciar el servicio de monitoreo

#### **No recibo notificaciones**
1. Verificar configuración de email
2. Comprobar carpeta de spam
3. Validar configuración SMTP
4. Revisar permisos de usuario

### Códigos de Error

| Código | Descripción | Solución |
|--------|-------------|----------|
| **E001** | Error de conexión a impresora | Verificar IP y conectividad |
| **E002** | Timeout de respuesta | Aumentar tiempo de espera |
| **E003** | Credenciales inválidas | Verificar usuario/contraseña |
| **E004** | Servicio no disponible | Contactar soporte técnico |

---

## 📞 Soporte Técnico

### Información de Contacto
- **Email**: soporte@qopiq.com
- **Teléfono**: +1 (555) 123-4567
- **Horario**: Lunes a Viernes, 9:00 AM - 6:00 PM

### Información del Sistema
Para reportar problemas, incluir:
- **Versión del sistema**: QOPIQ Monitor v1.0
- **Navegador utilizado**: Chrome, Firefox, Safari, etc.
- **Descripción del problema**: Pasos para reproducir
- **Capturas de pantalla**: Si es aplicable

### Recursos Adicionales
- **Base de conocimientos**: FAQ y guías
- **Videos tutoriales**: Demostraciones paso a paso
- **Foro de usuarios**: Comunidad de usuarios
- **Actualizaciones**: Notas de versión y mejoras

---

## 🎯 Mejores Prácticas

### Organización de Impresoras
- **Nomenclatura consistente**: Usar convención de nombres clara
- **Ubicaciones descriptivas**: Incluir piso, oficina, departamento
- **Agrupación lógica**: Organizar por área o función
- **Documentación**: Mantener información actualizada

### Monitoreo Efectivo
- **Revisión regular**: Verificar dashboard diariamente
- **Configuración de alertas**: Ajustar umbrales según necesidades
- **Mantenimiento preventivo**: Actuar sobre advertencias tempranas
- **Reportes periódicos**: Generar reportes mensuales

### Seguridad
- **Contraseñas fuertes**: Usar contraseñas complejas y únicas
- **Acceso limitado**: Asignar roles apropiados
- **Sesiones seguras**: Cerrar sesión al terminar
- **Actualizaciones**: Mantener el sistema actualizado

---

**👥 ¡Bienvenido a QOPIQ Monitor! Su solución completa para monitoreo de impresoras.**
