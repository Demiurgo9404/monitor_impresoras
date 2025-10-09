# Monitor de Impresoras - Frontend

Un dashboard moderno y responsive para monitorear impresoras en tiempo real, construido con React, TypeScript y Vite.

## 🚀 Características

- **Dashboard en tiempo real** con estado de impresoras
- **Sistema de alertas** con notificaciones automáticas
- **Análisis de costos** con gráficos interactivos
- **Autenticación JWT** con roles (Admin, Técnico, Usuario)
- **SignalR** para actualizaciones en tiempo real
- **Responsive design** con Tailwind CSS
- **Reportes automáticos** y bajo demanda

## 🛠️ Tecnologías

- **React 18** - Biblioteca de UI
- **TypeScript** - Tipado estático
- **Vite** - Build tool rápido
- **Tailwind CSS** - Framework de estilos
- **Chart.js** - Gráficos y visualizaciones
- **SignalR** - Comunicaciones en tiempo real
- **React Router** - Navegación SPA
- **Axios** - Cliente HTTP

## 📋 Prerrequisitos

- Node.js 18+
- npm o yarn
- Backend API ejecutándose en puerto 5000

## 🚀 Instalación

1. **Instalar dependencias:**
   ```bash
   cd frontend
   npm install
   ```

2. **Iniciar servidor de desarrollo:**
   ```bash
   npm run dev
   ```

3. **Abrir en navegador:**
   ```
   http://localhost:3000
   ```

## 📁 Estructura del Proyecto

```
src/
├── components/          # Componentes reutilizables
│   ├── AlertCard.tsx    # Tarjeta de alertas
│   ├── CostChart.tsx    # Gráficos de costos
│   ├── Layout.tsx       # Layout principal
│   ├── PrinterCard.tsx  # Tarjeta de impresoras
│   └── StatsOverview.tsx # Resumen de estadísticas
├── contexts/            # Context providers
│   └── AuthContext.tsx  # Autenticación
├── pages/               # Páginas principales
│   ├── Alerts.tsx       # Página de alertas
│   ├── Dashboard.tsx    # Dashboard principal
│   ├── Login.tsx        # Página de login
│   ├── Printers.tsx     # Página de impresoras
│   ├── Reports.tsx      # Página de reportes
│   └── Settings.tsx     # Página de configuración
├── services/            # Servicios y utilidades
│   ├── auth.ts          # Servicio de autenticación
│   └── signalRService.ts # Cliente SignalR
├── App.tsx              # Componente raíz
├── main.tsx             # Punto de entrada
└── index.css            # Estilos globales
```

## 🔧 Configuración

### Variables de Entorno

Crear archivo `.env` en la raíz:

```env
VITE_API_BASE_URL=http://localhost:5000
```

### Cuentas de Prueba

- **Admin:** `admin` / `admin123`
- **Técnico:** `tecnico` / `tecnico123`
- **Usuario:** `usuario` / `usuario123`

## 📱 Características del Dashboard

### Dashboard Principal
- Estado global de impresoras (online/offline)
- Alertas activas y recientes
- Gráficos de costos en tiempo real
- Estadísticas de consumibles

### Gestión de Impresoras
- Lista de todas las impresoras
- Estado en tiempo real
- Configuración individual
- Historial de actividad

### Sistema de Alertas
- Alertas por severidad (Baja, Media, Alta, Crítica)
- Gestión de alertas (reconocer/resolver)
- Historial completo de alertas
- Notificaciones automáticas

### Reportes
- Reportes automáticos programados
- Generación bajo demanda
- Múltiples formatos (PDF, Excel, CSV)
- Distribución por email

### Configuración
- Configuración general del sistema
- Umbrales de consumibles
- Configuración de notificaciones
- Gestión de usuarios y roles

## 🔌 API Integration

El frontend se conecta automáticamente con la API backend:

- **Autenticación:** `/api/auth/*`
- **Impresoras:** `/api/printers/*`
- **Alertas:** `/api/alerts/*`
- **Reportes:** `/api/reports/*`
- **SignalR:** `/hubs/printer`

## 🎨 Personalización

### Temas
El proyecto usa Tailwind CSS para estilos. Modificar `tailwind.config.js` para personalizar:

```javascript
module.exports = {
  theme: {
    extend: {
      colors: {
        primary: '#3B82F6',
        secondary: '#10B981',
      }
    }
  }
}
```

### Componentes
Los componentes están en `/src/components/` y usan TypeScript para type safety.

## 🚀 Despliegue

### Build de Producción
```bash
npm run build
```

### Preview del Build
```bash
npm run preview
```

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Notas de Desarrollo

- El proyecto usa **React 18** con **Strict Mode**
- **TypeScript** para type safety
- **ESLint** configurado para mantener calidad de código
- **Prettier** recomendado para formateo consistente
- **SignalR** para comunicaciones en tiempo real

## 🐛 Troubleshooting

### Error de CORS
Asegurarse que el backend tenga CORS configurado para `http://localhost:3000`

### SignalR no conecta
Verificar que el backend esté ejecutándose en puerto 5000

### Error de autenticación
Verificar que el token JWT sea válido y no haya expirado

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

