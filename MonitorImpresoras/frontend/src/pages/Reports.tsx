import React, { useState } from 'react'
import { DocumentChartBarIcon, CalculatorIcon, ChartBarIcon, CogIcon } from '@heroicons/react/24/outline'
import CostCalculator from '../components/CostCalculator'
import CostSimulation from '../components/CostSimulation'

interface ReportConfig {
  id: string
  name: string
  description: string
  schedule: string
  format: 'PDF' | 'Excel' | 'CSV'
  recipients: string[]
  enabled: boolean
}

export default function Reports() {
  const [reports, setReports] = useState<ReportConfig[]>([
    {
      id: '1',
      name: 'Reporte Diario de Impresión',
      description: 'Resumen diario de uso de impresoras y costos',
      schedule: 'Diario - 8:00 AM',
      format: 'PDF',
      recipients: ['admin@empresa.com', 'it@empresa.com'],
      enabled: true
    },
    {
      id: '2',
      name: 'Reporte Mensual de Costos',
      description: 'Análisis mensual de costos por departamento',
      schedule: 'Mensual - 1ro de cada mes',
      format: 'Excel',
      recipients: ['gerencia@empresa.com'],
      enabled: true
    },
    {
      id: '3',
      name: 'Reporte de Alertas',
      description: 'Resumen de alertas y su resolución',
      schedule: 'Semanal - Lunes 9:00 AM',
      format: 'PDF',
      recipients: ['soporte@empresa.com'],
      enabled: false
    }
  ])

  const [activeTab, setActiveTab] = useState<'reports' | 'calculator' | 'simulation' | 'rates'>('reports')
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingReport, setEditingReport] = useState<ReportConfig | null>(null)

  const handleToggleReport = (id: string) => {
    setReports(prev =>
      prev.map(report =>
        report.id === id ? { ...report, enabled: !report.enabled } : report
      )
    )
  }

  const handleDeleteReport = (id: string) => {
    setReports(prev => prev.filter(report => report.id !== id))
  }

  const handleGenerateNow = (id: string) => {
    console.log('Generando reporte inmediatamente:', id)
    // Aquí iría la lógica para generar el reporte
  }

  const tabs = [
    { id: 'reports', name: 'Reportes Programados', icon: DocumentChartBarIcon },
    { id: 'calculator', name: 'Calculadora de Costos', icon: CalculatorIcon },
    { id: 'simulation', name: 'Simulación de Escenarios', icon: ChartBarIcon },
    { id: 'rates', name: 'Configurar Tarifas', icon: CogIcon }
  ]

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Reportes y Costos</h1>
          <p className="text-gray-600">Gestiona reportes automáticos y calcula costos de impresión</p>
        </div>
        {activeTab === 'reports' && (
          <button
            onClick={() => setShowCreateForm(true)}
            className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
          >
            Crear Reporte
          </button>
        )}
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
      <div className="tab-content">
        {activeTab === 'reports' && (
          <div className="space-y-6">
            {/* Report Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {reports.map(report => (
                <div key={report.id} className="bg-white rounded-lg shadow-md p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex-1">
                      <h3 className="text-lg font-semibold text-gray-900">{report.name}</h3>
                      <p className="text-sm text-gray-600 mt-1">{report.description}</p>
                    </div>
                    <div className="flex items-center space-x-2">
                      <button
                        onClick={() => handleToggleReport(report.id)}
                        className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                          report.enabled ? 'bg-green-600' : 'bg-gray-200'
                        }`}
                      >
                        <span
                          className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                            report.enabled ? 'translate-x-6' : 'translate-x-1'
                          }`}
                        />
                      </button>
                    </div>
                  </div>

                  <div className="space-y-3 mb-4">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Programación:</span>
                      <span className="font-medium">{report.schedule}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Formato:</span>
                      <span className="font-medium">{report.format}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Destinatarios:</span>
                      <span className="font-medium">{report.recipients.length}</span>
                    </div>
                  </div>

                  <div className="flex space-x-2">
                    <button
                      onClick={() => handleGenerateNow(report.id)}
                      className="flex-1 bg-blue-500 hover:bg-blue-600 text-white text-sm font-medium py-2 px-4 rounded"
                    >
                      Generar Ahora
                    </button>
                    <button
                      onClick={() => setEditingReport(report)}
                      className="bg-gray-500 hover:bg-gray-600 text-white text-sm font-medium py-2 px-4 rounded"
                    >
                      Editar
                    </button>
                    <button
                      onClick={() => handleDeleteReport(report.id)}
                      className="bg-red-500 hover:bg-red-600 text-white text-sm font-medium py-2 px-4 rounded"
                    >
                      Eliminar
                    </button>
                  </div>
                </div>
              ))}
            </div>

            {/* Quick Stats */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div className="bg-white rounded-lg shadow-md p-6">
                <div className="text-sm text-gray-500 mb-1">Reportes Activos</div>
                <div className="text-2xl font-bold text-gray-900">
                  {reports.filter(r => r.enabled).length}
                </div>
                <div className="text-xs text-gray-400">de {reports.length} totales</div>
              </div>
              <div className="bg-white rounded-lg shadow-md p-6">
                <div className="text-sm text-gray-500 mb-1">Reportes Generados Hoy</div>
                <div className="text-2xl font-bold text-gray-900">3</div>
                <div className="text-xs text-gray-400">últimas 24h</div>
              </div>
              <div className="bg-white rounded-lg shadow-md p-6">
                <div className="text-sm text-gray-500 mb-1">Próximo Reporte</div>
                <div className="text-lg font-bold text-gray-900">8:00 AM</div>
                <div className="text-xs text-gray-400">Reporte Diario</div>
              </div>
            </div>

            {/* Recent Reports History */}
            <div className="bg-white rounded-lg shadow-md">
              <div className="px-6 py-4 border-b border-gray-200">
                <h3 className="text-lg font-semibold text-gray-900">Historial Reciente</h3>
              </div>
              <div className="divide-y divide-gray-200">
                {[
                  { name: 'Reporte Diario', date: '2024-01-15 08:00', status: 'Completado', size: '2.4 MB' },
                  { name: 'Reporte Mensual', date: '2024-01-01 09:00', status: 'Completado', size: '1.8 MB' },
                  { name: 'Reporte de Alertas', date: '2024-01-14 10:30', status: 'Completado', size: '856 KB' }
                ].map((report, index) => (
                  <div key={index} className="px-6 py-4 flex items-center justify-between">
                    <div>
                      <div className="font-medium text-gray-900">{report.name}</div>
                      <div className="text-sm text-gray-500">{report.date}</div>
                    </div>
                    <div className="flex items-center space-x-4">
                      <span className="text-sm text-gray-500">{report.size}</span>
                      <span className="px-2 py-1 text-xs bg-green-100 text-green-800 rounded-full">
                        {report.status}
                      </span>
                      <button className="text-blue-600 hover:text-blue-800 text-sm font-medium">
                        Descargar
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {activeTab === 'calculator' && <CostCalculator />}

        {activeTab === 'simulation' && <CostSimulation />}

        {activeTab === 'rates' && (
          <div className="bg-white rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Configurar Tarifas</h3>
            <div className="text-center py-12 text-gray-500">
              <CogIcon className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>Configuración de tarifas estará disponible próximamente</p>
              <p className="text-sm">Esta funcionalidad permitirá configurar:</p>
              <ul className="text-sm mt-2 space-y-1">
                <li>• Tarifas base por página</li>
                <li>• Cargos por impresión a color</li>
                <li>• Descuentos por departamento</li>
                <li>• Políticas de costo especiales</li>
              </ul>
            </div>
          </div>
        )}
      </div>

      {/* Create/Edit Form Modal would go here */}
      {showCreateForm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold mb-4">Crear Nuevo Reporte</h3>
            <p className="text-gray-600 mb-4">Esta funcionalidad estará disponible próximamente.</p>
            <button
              onClick={() => setShowCreateForm(false)}
              className="w-full bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded"
            >
              Cerrar
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
