import React, { useState } from 'react'
import { CogIcon, ShieldCheckIcon } from '@heroicons/react/24/outline'
import PoliciesManagement from '../components/PoliciesManagement'

interface SystemSettings {
  companyName: string
  contactEmail: string
  timezone: string
  currency: string
  lowConsumableThreshold: number
  criticalConsumableThreshold: number
  enableEmailNotifications: boolean
  enableSmsNotifications: boolean
  maintenanceMode: boolean
}

export default function Settings() {
  const [settings, setSettings] = useState<SystemSettings>({
    companyName: 'Mi Empresa S.A.',
    contactEmail: 'admin@empresa.com',
    timezone: 'America/Mexico_City',
    currency: 'MXN',
    lowConsumableThreshold: 20,
    criticalConsumableThreshold: 10,
    enableEmailNotifications: true,
    enableSmsNotifications: false,
    maintenanceMode: false
  })

  const [saving, setSaving] = useState(false)
  const [activeTab, setActiveTab] = useState<'general' | 'policies' | 'notifications'>('general')

  const handleSave = async () => {
    setSaving(true)
    try {
      // Aquí iría la llamada a la API para guardar la configuración
      await new Promise(resolve => setTimeout(resolve, 1000)) // Simular API call
      console.log('Configuración guardada:', settings)
    } catch (error) {
      console.error('Error al guardar configuración:', error)
    } finally {
      setSaving(false)
    }
  }

  const handleInputChange = (field: keyof SystemSettings, value: any) => {
    setSettings(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const tabs = [
    { id: 'general', name: 'Configuración General', icon: CogIcon },
    { id: 'policies', name: 'Políticas de Impresión', icon: ShieldCheckIcon },
    { id: 'notifications', name: 'Notificaciones', icon: CogIcon }
  ]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Configuración del Sistema</h1>
        <p className="text-gray-600">Gestiona la configuración general del sistema de monitoreo</p>
      </div>

      {/* Navigation Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          {tabs.map((tab) => {
            const Icon = tab.icon
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id as any)}
                className={`flex items-center py-2 px-1 border-b-2 font-medium text-sm ${
                  activeTab === tab.id
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <Icon className="h-4 w-4 mr-2" />
                {tab.name}
              </button>
            )
          })}
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'general' && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* General Settings */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Configuración General</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre de la Empresa
                </label>
                <input
                  type="text"
                  value={settings.companyName}
                  onChange={(e) => handleInputChange('companyName', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email de Contacto
                </label>
                <input
                  type="email"
                  value={settings.contactEmail}
                  onChange={(e) => handleInputChange('contactEmail', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Zona Horaria
                </label>
                <select
                  value={settings.timezone}
                  onChange={(e) => handleInputChange('timezone', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="America/Mexico_City">Ciudad de México</option>
                  <option value="America/New_York">Nueva York</option>
                  <option value="Europe/London">Londres</option>
                  <option value="Asia/Tokyo">Tokio</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Moneda
                </label>
                <select
                  value={settings.currency}
                  onChange={(e) => handleInputChange('currency', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="MXN">Peso Mexicano (MXN)</option>
                  <option value="USD">Dólar Americano (USD)</option>
                  <option value="EUR">Euro (EUR)</option>
                  <option value="GBP">Libra Esterlina (GBP)</option>
                </select>
              </div>
            </div>
          </div>

          {/* Notification Settings */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Notificaciones</h2>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm font-medium text-gray-900">Notificaciones por Email</div>
                  <div className="text-sm text-gray-500">Recibir alertas por correo electrónico</div>
                </div>
                <button
                  onClick={() => handleInputChange('enableEmailNotifications', !settings.enableEmailNotifications)}
                  className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                    settings.enableEmailNotifications ? 'bg-green-600' : 'bg-gray-200'
                  }`}
                >
                  <span
                    className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                      settings.enableEmailNotifications ? 'translate-x-6' : 'translate-x-1'
                    }`}
                  />
                </button>
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm font-medium text-gray-900">Notificaciones por SMS</div>
                  <div className="text-sm text-gray-500">Recibir alertas críticas por SMS</div>
                </div>
                <button
                  onClick={() => handleInputChange('enableSmsNotifications', !settings.enableSmsNotifications)}
                  className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                    settings.enableSmsNotifications ? 'bg-green-600' : 'bg-gray-200'
                  }`}
                >
                  <span
                    className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                      settings.enableSmsNotifications ? 'translate-x-6' : 'translate-x-1'
                    }`}
                  />
                </button>
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm font-medium text-gray-900">Modo de Mantenimiento</div>
                  <div className="text-sm text-gray-500">Desactivar notificaciones automáticas</div>
                </div>
                <button
                  onClick={() => handleInputChange('maintenanceMode', !settings.maintenanceMode)}
                  className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                    settings.maintenanceMode ? 'bg-green-600' : 'bg-gray-200'
                  }`}
                >
                  <span
                    className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                      settings.maintenanceMode ? 'translate-x-6' : 'translate-x-1'
                    }`}
                  />
                </button>
              </div>
            </div>
          </div>

          {/* Threshold Settings */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Umbrales de Consumibles</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Umbral Bajo (%)
                </label>
                <input
                  type="number"
                  min="1"
                  max="50"
                  value={settings.lowConsumableThreshold}
                  onChange={(e) => handleInputChange('lowConsumableThreshold', parseInt(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Generar alerta cuando el consumible esté por debajo de este porcentaje
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Umbral Crítico (%)
                </label>
                <input
                  type="number"
                  min="1"
                  max="30"
                  value={settings.criticalConsumableThreshold}
                  onChange={(e) => handleInputChange('criticalConsumableThreshold', parseInt(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Generar alerta crítica cuando el consumible esté por debajo de este porcentaje
                </p>
              </div>
            </div>
          </div>

          {/* System Status */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Estado del Sistema</h2>
            <div className="space-y-3">
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Versión del Sistema</span>
                <span className="text-sm font-medium text-gray-900">v1.0.0</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Última Actualización</span>
                <span className="text-sm font-medium text-gray-900">2024-01-15</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">Base de Datos</span>
                <span className="text-sm font-medium text-green-600">Conectada</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-sm text-gray-600">SignalR</span>
                <span className="text-sm font-medium text-green-600">Conectado</span>
              </div>
            </div>

            <div className="mt-6 pt-4 border-t border-gray-200">
              <button
                onClick={() => console.log('Ver logs del sistema')}
                className="w-full bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded"
              >
                Ver Logs del Sistema
              </button>
            </div>
          </div>
        </div>
      )}

      {activeTab === 'policies' && <PoliciesManagement />}

      {activeTab === 'notifications' && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Configuración de Notificaciones</h3>
          <div className="text-center py-12 text-gray-500">
            <CogIcon className="h-12 w-12 mx-auto mb-4 opacity-50" />
            <p>Configuración de notificaciones estará disponible próximamente</p>
            <p className="text-sm">Esta funcionalidad permitirá configurar:</p>
            <ul className="text-sm mt-2 space-y-1">
              <li>• Destinatarios de alertas por email</li>
              <li>• Configuración de SMS</li>
              <li>• Webhooks para integraciones</li>
              <li>• Plantillas de notificación</li>
            </ul>
          </div>
        </div>
      )}

      {/* Save Button - Only show for general settings */}
      {activeTab === 'general' && (
        <div className="flex justify-end">
          <button
            onClick={handleSave}
            disabled={saving}
            className="bg-blue-500 hover:bg-blue-600 disabled:bg-blue-300 text-white px-6 py-2 rounded-lg font-medium"
          >
            {saving ? 'Guardando...' : 'Guardar Configuración'}
          </button>
        </div>
      )}
    </div>
  )
}
