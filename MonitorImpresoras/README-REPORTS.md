# 📊 **QOPIQ - Sistema de Reportes Automatizados**

## 🎯 **Sistema Completo de Reportes Multi-Tenant**

El sistema de reportes de QOPIQ permite generar, programar y distribuir reportes automáticos de monitoreo de impresoras con soporte completo multi-tenant.

---

## 🚀 **Características Principales**

### **📄 Generación de Reportes**
- ✅ **Múltiples formatos**: PDF, Excel, CSV
- ✅ **Templates profesionales**: HTML con estilos CSS modernos
- ✅ **Datos completos**: Estadísticas, consumibles, costos, alertas
- ✅ **Gráficos y tablas**: Visualización clara de datos
- ✅ **Períodos flexibles**: Diario, semanal, mensual, trimestral, anual, personalizado

### **⏰ Reportes Programados**
- ✅ **Scheduler automático**: Servicio de background con cron expressions
- ✅ **Configuración flexible**: Horarios personalizables por proyecto
- ✅ **Ejecución multi-tenant**: Aislamiento completo por tenant
- ✅ **Gestión de estados**: Activo/inactivo, última ejecución, próxima ejecución

### **📧 Distribución por Email**
- ✅ **SMTP configurado**: Soporte para Gmail, Outlook, servidores personalizados
- ✅ **Templates HTML**: Emails profesionales con branding
- ✅ **Adjuntos automáticos**: Reportes incluidos en el email
- ✅ **Múltiples destinatarios**: Distribución a listas de correo

### **🔐 Seguridad Multi-Tenant**
- ✅ **Autorización por roles**: Admin, ProjectManager, Viewer
- ✅ **Aislamiento de datos**: Cada tenant solo ve sus reportes
- ✅ **Descarga segura**: Validación de acceso en cada descarga
- ✅ **Auditoría completa**: Logs de generación y acceso

---

## 📋 **Endpoints Disponibles**

### **Reportes Principales**
```http
POST   /api/report/generate          # Generar nuevo reporte
GET    /api/report                   # Lista paginada de reportes
GET    /api/report/{id}              # Obtener reporte específico
GET    /api/report/{id}/download     # Descargar archivo de reporte
DELETE /api/report/{id}              # Eliminar reporte (SuperAdmin)
POST   /api/report/{id}/resend       # Reenviar por email
GET    /api/report/project/{id}      # Reportes de un proyecto
GET    /api/report/quick/{projectId} # Generar reporte rápido
GET    /api/report/types             # Tipos y formatos disponibles
GET    /api/report/stats             # Estadísticas de reportes
```

### **Reportes Programados**
```http
POST   /api/scheduledreport          # Crear reporte programado
GET    /api/scheduledreport          # Lista de reportes programados
GET    /api/scheduledreport/{id}     # Obtener reporte programado
PUT    /api/scheduledreport/{id}     # Actualizar reporte programado
DELETE /api/scheduledreport/{id}     # Eliminar reporte programado
PATCH  /api/scheduledreport/{id}/toggle # Activar/desactivar
POST   /api/scheduledreport/{id}/execute # Ejecutar manualmente
GET    /api/scheduledreport/schedule-templates # Plantillas de horarios
GET    /api/scheduledreport/stats    # Estadísticas de programación
```

---

## 🎯 **Guía de Uso Completa**

### **1. Generar Reporte Manual**

```http
POST /api/report/generate
Authorization: Bearer {token}
X-Tenant-Id: demo
Content-Type: application/json

{
  "projectId": "project-guid-here",
  "reportType": "Monthly",
  "periodStart": "2025-09-01T00:00:00Z",
  "periodEnd": "2025-09-30T23:59:59Z",
  "format": "PDF",
  "title": "Reporte Mensual Septiembre 2025",
  "includeCounters": true,
  "includeConsumables": true,
  "includeCosts": true,
  "includeCharts": true,
  "sendByEmail": true,
  "emailRecipients": ["manager@empresa.com", "admin@empresa.com"]
}
```

**Respuesta:**
```json
{
  "id": "report-guid",
  "projectId": "project-guid",
  "projectName": "Oficina Central",
  "companyName": "Mi Empresa S.A.",
  "title": "Reporte Mensual Septiembre 2025",
  "reportType": "Monthly",
  "periodStart": "2025-09-01T00:00:00Z",
  "periodEnd": "2025-09-30T23:59:59Z",
  "generatedAt": "2025-10-03T20:00:00Z",
  "status": "Generated",
  "fileFormat": "PDF",
  "fileName": "reporte_Oficina_Central_Monthly_20251003_200000.pdf",
  "fileSizeBytes": 245760,
  "emailSent": true,
  "emailSentAt": "2025-10-03T20:00:15Z",
  "totalPrinters": 5,
  "activePrinters": 4,
  "totalPrintsBW": 2500,
  "totalPrintsColor": 800,
  "totalScans": 450,
  "totalCostBW": 50.00,
  "totalCostColor": 64.00
}
```

### **2. Programar Reporte Automático**

```http
POST /api/scheduledreport
Authorization: Bearer {token}
X-Tenant-Id: demo
Content-Type: application/json

{
  "projectId": "project-guid-here",
  "name": "Reporte Mensual Automático",
  "reportType": "Monthly",
  "schedule": "0 0 1 * *",
  "format": "Both",
  "emailRecipients": ["gerencia@empresa.com", "contabilidad@empresa.com"],
  "includeCounters": true,
  "includeConsumables": true,
  "includeCosts": true,
  "includeCharts": true
}
```

### **3. Descargar Reporte**

```http
GET /api/report/{report-id}/download
Authorization: Bearer {token}
X-Tenant-Id: demo
```

**Respuesta:** Archivo binario (PDF/Excel/CSV) con headers apropiados.

### **4. Reporte Rápido**

```http
GET /api/report/quick/{project-id}?reportType=Weekly
Authorization: Bearer {token}
X-Tenant-Id: demo
```

**Respuesta:** Descarga directa del PDF del último período.

---

## ⚙️ **Configuración de Email**

### **appsettings.json**
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password",
    "FromEmail": "noreply@tuempresa.com",
    "FromName": "QOPIQ Sistema",
    "EnableSsl": true
  }
}
```

### **Variables de Entorno (Producción)**
```bash
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__Username=sistema@tuempresa.com
Email__Password=tu-password-seguro
Email__FromEmail=noreply@tuempresa.com
Email__FromName=QOPIQ Sistema
Email__EnableSsl=true
```

---

## 📊 **Tipos de Reportes**

### **Por Período**
- **Daily**: Reporte diario (último día)
- **Weekly**: Reporte semanal (últimos 7 días)
- **Monthly**: Reporte mensual (último mes completo)
- **Quarterly**: Reporte trimestral (último trimestre)
- **Yearly**: Reporte anual (último año)
- **Custom**: Período personalizado

### **Por Formato**
- **PDF**: Reporte visual con gráficos y tablas
- **Excel**: Datos estructurados en múltiples hojas
- **CSV**: Datos simples para análisis
- **Both**: PDF + Excel en el mismo reporte

### **Por Contenido**
- **Executive Summary**: Resumen ejecutivo
- **Detailed Printer Report**: Detalle por impresora
- **Consumables Report**: Estado de consumibles
- **Cost Analysis**: Análisis de costos

---

## 🕐 **Programación con Cron**

### **Expresiones Comunes**
```bash
"0 0 * * *"     # Diario a medianoche
"0 0 * * 1"     # Lunes a medianoche
"0 0 1 * *"     # Primer día del mes
"0 0 1 1,4,7,10 *" # Cada trimestre
"0 0 1 1 *"     # Primer día del año
"0 */6 * * *"   # Cada 6 horas
"0 0 * * 1-5"   # Lunes a viernes
```

### **Plantillas Predefinidas**
```http
GET /api/scheduledreport/schedule-templates
```

```json
{
  "scheduleTemplates": [
    {
      "name": "Diario",
      "cron": "0 0 * * *",
      "description": "Todos los días a medianoche"
    },
    {
      "name": "Semanal (Lunes)",
      "cron": "0 0 * * 1",
      "description": "Todos los lunes a medianoche"
    },
    {
      "name": "Mensual",
      "cron": "0 0 1 * *",
      "description": "El primer día de cada mes"
    }
  ]
}
```

---

## 🎨 **Personalización de Templates**

### **PDF Template (HTML + CSS)**
Los reportes PDF se generan usando HTML con CSS moderno:

```html
<div class="header">
  <h1>QOPIQ - Sistema de Monitoreo</h1>
  <h2>Reporte de {ProjectName}</h2>
</div>

<div class="summary-grid">
  <div class="summary-item">
    <span class="label">Total Impresoras:</span>
    <span class="value">{TotalPrinters}</span>
  </div>
</div>
```

### **Excel Template (Múltiples Hojas)**
- **Hoja 1**: Resumen Ejecutivo
- **Hoja 2**: Detalle de Impresoras
- **Hoja 3**: Estado de Consumibles
- **Hoja 4**: Análisis de Costos

---

## 🔍 **Monitoreo y Logs**

### **Logs del Scheduler**
```
[15:00:00 INF] Report Scheduler Service started
[15:05:00 INF] Found 3 scheduled reports to execute
[15:05:01 INF] Executing scheduled report: abc-123 - Reporte Mensual
[15:05:15 INF] Scheduled report executed successfully: abc-123
[15:05:16 INF] Report email sent successfully for report def-456
```

### **Métricas Disponibles**
```http
GET /api/report/stats
```

```json
{
  "totalReports": 45,
  "reportsByType": {
    "Monthly": 20,
    "Weekly": 15,
    "Daily": 10
  },
  "reportsByStatus": {
    "Generated": 40,
    "Sent": 35,
    "Failed": 5
  },
  "reportsByFormat": {
    "PDF": 25,
    "Excel": 15,
    "Both": 5
  }
}
```

---

## 🚨 **Troubleshooting**

### **Error: Email no enviado**
```bash
# Verificar configuración SMTP
curl -X GET "http://localhost:5278/api/auth/test" \
  -H "Authorization: Bearer {token}" \
  -H "X-Tenant-Id: demo"

# Revisar logs
docker logs qopiq-api | grep "Email"
```

### **Error: Reporte no generado**
```bash
# Verificar permisos
curl -X GET "http://localhost:5278/api/report/test-auth" \
  -H "Authorization: Bearer {token}" \
  -H "X-Tenant-Id: demo"

# Verificar datos del proyecto
curl -X GET "http://localhost:5278/api/project/{project-id}" \
  -H "Authorization: Bearer {token}" \
  -H "X-Tenant-Id: demo"
```

### **Error: Scheduler no ejecuta**
```bash
# Verificar servicio de background
docker logs qopiq-api | grep "Report Scheduler"

# Verificar reportes programados
curl -X GET "http://localhost:5278/api/scheduledreport" \
  -H "Authorization: Bearer {token}" \
  -H "X-Tenant-Id: demo"
```

---

## 🎯 **Próximos Pasos**

### **Día 4: Frontend Multi-Tenant**
- Dashboard de reportes interactivo
- Generador visual de reportes
- Calendario de reportes programados
- Visualización de estadísticas

### **Mejoras Futuras**
- Templates personalizables por tenant
- Integración con Power BI / Tableau
- Reportes en tiempo real
- API webhooks para notificaciones
- Exportación a Google Drive / OneDrive

**¡El sistema de reportes automatizados está 100% funcional y listo para uso en producción! 🚀**

