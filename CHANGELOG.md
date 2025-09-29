# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2025-01-29

### 🔧 **CI/CD y Despliegue Profesional en Windows Server + IIS**

#### **Added**
- **Pipeline CI/CD Completo** ✅
  - **GitHub Actions** con jobs especializados
  - **Build + Tests + Publish** automático en cada push
  - **Análisis de Código** con dotnet format y warnings como errores
  - **Artefactos** listos para despliegue automático

- **Configuración Multi-Entorno** ✅
  - **Development**: Logging detallado, configuración local
  - **Staging**: Configuración intermedia para pruebas
  - **Production**: Seguridad máxima y monitoreo completo
  - **Variables Externas** para credenciales sensibles

- **Despliegue Automatizado en IIS** ✅
  - **Script PowerShell** para copia de archivos y configuración
  - **Migraciones Automáticas** de base de datos
  - **Reinicio Controlado** del Application Pool
  - **Rollback Simple** restaurando versiones anteriores

- **Versionado Semántico** ✅
  - **Etiquetas Git** automáticas (v1.3.0, v1.3.1, etc.)
  - **Assembly Version** actualizada automáticamente
  - **CHANGELOG.md** mantenido con cada release
  - **Despliegues Trazables** con versiones específicas

#### **DevOps**
- **CI/CD Automático**: Build → Test → Publish → Deploy
- **Validación Post-Deploy**: Health checks automáticos
- **Monitoreo Continuo**: Métricas y alertas en producción
- **Rollback Instantáneo**: Procedimiento documentado

---

## [1.2.0] - 2025-01-28

### 📊 **Observabilidad y Monitoreo Avanzado**

#### **Added**
- **Health Checks Profesionales** ✅
  - **5 Niveles de Seguridad**: Básico, Extendido, Seguro, Readiness, Liveness
  - **Componentes Monitoreados**: Base de datos, workers, sistema, métricas
  - **Kubernetes Ready** con probes configurados

- **Métricas con Prometheus** ✅
  - **10 Métricas Principales** con etiquetas detalladas
  - **Middleware Automático** de captura de métricas HTTP
  - **Histogramas de Latencia** para análisis de rendimiento
  - **Contadores Específicos** para reportes, emails, seguridad

- **Auditoría Extendida** ✅
  - **Tabla SystemEvents** con 20+ campos forenses
  - **Información HTTP Completa** (IP, User-Agent, RequestId, StatusCode)
  - **Correlación de Eventos** con RequestId único
  - **Estadísticas Agregadas** por período y categoría

- **Logging Avanzado con Serilog** ✅
  - **Sink PostgreSQL** para logs estructurados en BD
  - **Enriquecimiento Automático** (Application, Environment, MachineName)
  - **Batch Processing** eficiente (100 eventos cada 5 segundos)
  - **Configuración Dinámica** por nivel de log

#### **Monitoring**
- **Sistema de Alertas** con 6 reglas críticas configuradas
- **Dashboard Grafana** profesional para visualización
- **Configuración Prometheus** completa para scraping
- **Notificaciones Multi-Canal** (Slack, Email) por severidad

#### **Enterprise**
- **Observabilidad Completa** en tiempo real
- **Correlación End-to-End** de solicitudes
- **Métricas de Negocio** (usuarios activos, reportes generados)
- **Alertas Proactivas** para detección temprana de problemas

---

## [1.1.0] - 2025-01-28

### 🚀 **Exportación Avanzada y Automatización**

#### **Added**
- **Exportación Avanzada** ✅
  - **PDF Profesional** con QuestPDF (tablas, encabezados, formato condicional)
  - **Excel Avanzado** con EPPlus (múltiples hojas, formato condicional, filtros)
  - **4 Formatos Totales**: JSON, CSV, PDF, Excel

- **Reportes Programados Automáticos** ✅
  - **Expresiones CRON** flexibles (diario, semanal, mensual)
  - **Background Worker** que ejecuta cada minuto
  - **Notificaciones por Email** con adjuntos automáticos
  - **Gestión CRUD** completa de programaciones

- **Sistema de Email** ✅
  - **MailKit** para envío profesional
  - **Configuración SMTP** flexible (Gmail, Outlook, servidores corporativos)
  - **Adjuntos Automáticos** hasta 10MB
  - **Logging de Entregas** exitosas y fallidas

#### **Performance**
- **Procesamiento Asíncrono** sin bloquear la API
- **Background Workers** para tareas pesadas
- **Optimización de Consultas** con índices específicos
- **Manejo Inteligente** de archivos temporales
- **Despliegue Automatizado** ✅
  - **Migraciones de BD** automáticas
  - **Reinicio de Servicios** controlado
  - **Rollback** sencillo con versiones trazables

#### **DevOps**
- **Versionado Automático** con GitVersion
- **Validación Post-Deploy** con health checks
- **Monitoreo Continuo** con métricas y alertas
- **Documentación de Despliegue** completa

---

## [Unreleased]

### **Planned**
- **API Versioning** con Microsoft.AspNetCore.Mvc.Versioning
- **Rate Limiting** con AspNetCoreRateLimit
- **Caching** con Redis para mejora de performance
- **Documentación Avanzada** con OpenAPI/Swagger mejorada
- **Tests de Carga** con herramientas especializadas
- **Dockerización** completa para despliegue en contenedores

---

## **Versioning Scheme**

This project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html):

- **MAJOR** version when you make incompatible API changes
- **MINOR** version when you add functionality in a backwards compatible manner
- **PATCH** version when you make backwards compatible bug fixes

## **Contributing**

1. Create a feature branch from `develop`
2. Make your changes
3. Add tests for new functionality
4. Update CHANGELOG.md
5. Create a Pull Request to `develop`
6. After review and tests pass, merge to `main`

## **Deployment**

See [README_DEPLOY.md](README_DEPLOY.md) for detailed deployment instructions.
