import React, { useState, useEffect } from 'react'
import {
  CalendarIcon,
  ClockIcon,
  DocumentChartBarIcon,
  PlayIcon,
  PauseIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  EyeIcon,
  DownloadIcon,
  CogIcon,
  CheckCircleIcon,
  XCircleIcon,
  ExclamationTriangleIcon,
  ArrowPathIcon
} from '@heroicons/react/24/outline'

interface ScheduledReport {
  id: number
  tenantId: number
  name: string
  description: string
  reportType: string
  format: string
  scheduleType: string
  frequency: string
  nextRunDate: string
  lastRunDate: string
  isActive: boolean
  createdAt: string
  totalRuns: number
  successfulRuns: number
  failedRuns: number
  lastError: string
  lastExecution: {
    executionId: number
    startDate: string
    endDate: string
    status: string
    statusMessage: string
    filePath: string
    fileSizeBytes: number
    recordCount: number
    executionTime: number
  } | null
  nextExecutions: Array<{
    scheduledDate: string
    description: string
  }>
}

export default function ScheduledReportsManagement() {
  const [reports, setReports] = useState<ScheduledReport[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingReport, setEditingReport] = useState<ScheduledReport | null>(null)
  const [showExecuteModal, setShowExecuteModal] = useState(false)
  const [selectedReport, setSelectedReport] = useState<ScheduledReport | null>(null)
  const [activeTab, setActiveTab] = useState<'list' | 'create' | 'executions'>('list')
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    reportType: 'PrinterUsage',
    format: 'PDF',
    scheduleType: 'Recurring',
    frequency: 'Daily',
    startTime: '08:00',
    dayOfMonth: 1,
    daysOfWeek: [] as string[],
    monthsOfYear: [] as string[],
    nextRunDate: '',
    emailRecipients: [] as string[],
    webhookUrls: [] as string[],
    retentionDays: 30,
    sendOnFailure: true
  })

  const reportTypes = [
    'PrinterUsage',
    'PrintJobs',
    'CostAnalysis',
    'ConsumableUsage',
    'AlertSummary',
    'UserActivity',
    'DepartmentUsage',
    'PolicyViolations',
    'SystemPerformance',
    'Custom'
  ]

  const formats = ['PDF', 'Excel', 'CSV', 'JSON']
  const frequencies = ['Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly']
  const daysOfWeek = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo']

  useEffect(() => {
    loadReports()
  }, [])

  const loadReports = async () => {
    try {
      setLoading(true)
      const response = await fetch('/api/scheduledreports?tenantId=1', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (response.ok) {
        const data = await response.json()
        setReports(data)
      }
    } catch (error) {
      console.error('Error loading reports:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleCreateReport = async () => {
    try {
      const requestData = {
        ...formData,
        tenantId: 1,
        daysOfWeek: formData.daysOfWeek.map(day => daysOfWeek.indexOf(day)),
        monthsOfYear: formData.monthsOfYear.map(month => parseInt(month))
      }

      const response = await fetch('/api/scheduledreports', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(requestData)
      })

      if (response.ok) {
        await loadReports()
        setShowCreateForm(false)
        resetForm()
      }
    } catch (error) {
      console.error('Error creating report:', error)
    }
  }

  const handleToggleStatus = async (reportId: number, isActive: boolean) => {
    try {
      const response = await fetch(`/api/scheduledreports/${reportId}/status`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ isActive })
      })

      if (response.ok) {
        await loadReports()
      }
    } catch (error) {
      console.error('Error toggling report status:', error)
    }
  }

  const handleExecuteReport = async (reportId: number) => {
    try {
      const response = await fetch('/api/scheduledreports/execute', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ scheduledReportId: reportId })
      })

      if (response.ok) {
        const result = await response.json()
        alert(`Ejecución iniciada. ID: ${result.executionId}`)
        await loadReports()
      }
    } catch (error) {
      console.error('Error executing report:', error)
    }
  }

  const handleDeleteReport = async (reportId: number) => {
    if (!confirm('¿Está seguro de que desea eliminar este reporte programado?')) return

    try {
      const response = await fetch(`/api/scheduledreports/${reportId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (response.ok) {
        await loadReports()
      }
    } catch (error) {
      console.error('Error deleting report:', error)
    }
  }

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      reportType: 'PrinterUsage',
      format: 'PDF',
      scheduleType: 'Recurring',
      frequency: 'Daily',
      startTime: '08:00',
      dayOfMonth: 1,
      daysOfWeek: [],
      monthsOfYear: [],
      nextRunDate: '',
      emailRecipients: [],
      webhookUrls: [],
      retentionDays: 30,
      sendOnFailure: true
    })
  }

  const getStatusColor = (isActive: boolean) => {
    return isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
  }

  const getExecutionStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'completed':
        return 'bg-green-100 text-green-800'
      case 'running':
        return 'bg-blue-100 text-blue-800'
      case 'failed':
        return 'bg-red-100 text-red-800'
      case 'cancelled':
        return 'bg-gray-100 text-gray-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getExecutionStatusIcon = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'completed':
        return <CheckCircleIcon className="h-4 w-4" />
      case 'running':
        return <ArrowPathIcon className="h-4 w-4 animate-spin" />
      case 'failed':
        return <XCircleIcon className="h-4 w-4" />
      case 'cancelled':
        return <XCircleIcon className="h-4 w-4" />
      default:
        return <ClockIcon className="h-4 w-4" />
    }
  }

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-96">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500"></div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <CalendarIcon className="h-8 w-8 text-blue-600 mr-3" />
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Reportes Programados</h2>
            <p className="text-sm text-gray-600">Gestiona reportes automáticos y programados</p>
          </div>
        </div>
        <button
          onClick={() => setShowCreateForm(true)}
          className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg flex items-center"
        >
          <PlusIcon className="h-4 w-4 mr-2" />
          Crear Reporte
        </button>
      </div>

      {/* Navigation Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('list')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'list'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Lista de Reportes
          </button>
          <button
            onClick={() => setActiveTab('create')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'create'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            {editingReport ? 'Editar Reporte' : 'Crear Nuevo'}
          </button>
          <button
            onClick={() => setActiveTab('executions')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'executions'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Ejecuciones Recientes
          </button>
        </nav>
      </div>

      {/* Content */}
      {activeTab === 'list' && (
        <div className="space-y-4">
          {reports.length === 0 ? (
            <div className="text-center py-12">
              <CalendarIcon className="h-12 w-12 mx-auto mb-4 text-gray-400" />
              <p className="text-gray-500">No hay reportes programados</p>
              <button
                onClick={() => setShowCreateForm(true)}
                className="mt-4 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
              >
                Crear Primer Reporte
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {reports.map(report => (
                <div key={report.id} className="bg-white rounded-lg shadow-md p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex-1">
                      <h3 className="text-lg font-semibold text-gray-900">{report.name}</h3>
                      <p className="text-sm text-gray-600 mt-1">{report.description}</p>
                      <p className="text-xs text-gray-500">{report.reportType} • {report.format}</p>
                    </div>
                    <div className="flex items-center space-x-2">
                      <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(report.isActive)}`}>
                        {report.isActive ? 'Activo' : 'Inactivo'}
                      </span>
                    </div>
                  </div>

                  {/* Schedule Info */}
                  <div className="space-y-2 mb-4">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Frecuencia:</span>
                      <span className="font-medium">{report.frequency}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Próxima ejecución:</span>
                      <span className="font-medium">{new Date(report.nextRunDate).toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Total ejecuciones:</span>
                      <span className="font-medium">{report.totalRuns}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Tasa de éxito:</span>
                      <span className="font-medium">
                        {report.totalRuns > 0 ? Math.round((report.successfulRuns / report.totalRuns) * 100) : 0}%
                      </span>
                    </div>
                  </div>

                  {/* Last Execution */}
                  {report.lastExecution && (
                    <div className="mb-4 p-3 bg-gray-50 rounded-lg">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium text-gray-700">Última ejecución</span>
                        <div className="flex items-center space-x-1">
                          {getExecutionStatusIcon(report.lastExecution.status)}
                          <span className={`px-2 py-1 text-xs font-medium rounded-full ${getExecutionStatusColor(report.lastExecution.status)}`}>
                            {report.lastExecution.status}
                          </span>
                        </div>
                      </div>
                      <div className="text-xs text-gray-600">
                        <div>Registros: {report.lastExecution.recordCount}</div>
                        <div>Duración: {report.lastExecution.executionTime}ms</div>
                        <div>Tamaño: {formatFileSize(report.lastExecution.fileSizeBytes || 0)}</div>
                      </div>
                    </div>
                  )}

                  {/* Actions */}
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <button
                        onClick={() => handleToggleStatus(report.id, !report.isActive)}
                        className={`p-2 rounded-lg ${report.isActive ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600'}`}
                        title={report.isActive ? 'Desactivar reporte' : 'Activar reporte'}
                      >
                        {report.isActive ? <PauseIcon className="h-4 w-4" /> : <PlayIcon className="h-4 w-4" />}
                      </button>
                      <button
                        onClick={() => handleExecuteReport(report.id)}
                        className="p-2 bg-blue-100 text-blue-600 rounded-lg"
                        title="Ejecutar ahora"
                      >
                        <PlayIcon className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => {
                          setEditingReport(report)
                          setFormData({
                            name: report.name,
                            description: report.description,
                            reportType: report.reportType,
                            format: report.format,
                            scheduleType: report.scheduleType,
                            frequency: report.frequency,
                            startTime: '08:00',
                            dayOfMonth: 1,
                            daysOfWeek: [],
                            monthsOfYear: [],
                            nextRunDate: report.nextRunDate,
                            emailRecipients: report.emailRecipients || [],
                            webhookUrls: report.webhookUrls || [],
                            retentionDays: 30,
                            sendOnFailure: true
                          })
                          setActiveTab('create')
                        }}
                        className="p-2 bg-gray-100 text-gray-600 rounded-lg"
                        title="Editar reporte"
                      >
                        <PencilIcon className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDeleteReport(report.id)}
                        className="p-2 bg-red-100 text-red-600 rounded-lg"
                        title="Eliminar reporte"
                      >
                        <TrashIcon className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {activeTab === 'create' && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">
            {editingReport ? 'Editar Reporte Programado' : 'Crear Nuevo Reporte Programado'}
          </h3>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Basic Information */}
            <div className="space-y-4">
              <h4 className="text-md font-medium text-gray-900">Información Básica</h4>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre del Reporte *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Ej: Reporte Diario de Impresión"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Descripción
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Describe el contenido del reporte"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Tipo de Reporte *
                </label>
                <select
                  value={formData.reportType}
                  onChange={(e) => setFormData({ ...formData, reportType: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {reportTypes.map(type => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Formato de Exportación *
                </label>
                <select
                  value={formData.format}
                  onChange={(e) => setFormData({ ...formData, format: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {formats.map(format => (
                    <option key={format} value={format}>{format}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Frecuencia *
                </label>
                <select
                  value={formData.frequency}
                  onChange={(e) => setFormData({ ...formData, frequency: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {frequencies.map(freq => (
                    <option key={freq} value={freq}>{freq}</option>
                  ))}
                </select>
              </div>

              {formData.frequency === 'Daily' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Hora de Ejecución
                  </label>
                  <input
                    type="time"
                    value={formData.startTime}
                    onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              )}

              {formData.frequency === 'Weekly' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Días de la Semana
                  </label>
                  <div className="grid grid-cols-2 gap-2">
                    {daysOfWeek.map(day => (
                      <label key={day} className="flex items-center">
                        <input
                          type="checkbox"
                          checked={formData.daysOfWeek.includes(day)}
                          onChange={(e) => {
                            const updated = e.target.checked
                              ? [...formData.daysOfWeek, day]
                              : formData.daysOfWeek.filter(d => d !== day)
                            setFormData({ ...formData, daysOfWeek: updated })
                          }}
                          className="mr-2"
                        />
                        {day}
                      </label>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Recipients and Configuration */}
            <div className="space-y-4">
              <h4 className="text-md font-medium text-gray-900">Destinatarios y Configuración</h4>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Destinatarios de Email
                </label>
                <textarea
                  value={formData.emailRecipients.join('\n')}
                  onChange={(e) => setFormData({
                    ...formData,
                    emailRecipients: e.target.value.split('\n').filter(email => email.trim())
                  })}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="email1@empresa.com&#10;email2@empresa.com"
                />
                <p className="text-xs text-gray-500 mt-1">Una dirección por línea</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  URLs de Webhook
                </label>
                <textarea
                  value={formData.webhookUrls.join('\n')}
                  onChange={(e) => setFormData({
                    ...formData,
                    webhookUrls: e.target.value.split('\n').filter(url => url.trim())
                  })}
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="https://api.ejemplo.com/webhook&#10;https://otro-ejemplo.com/hook"
                />
                <p className="text-xs text-gray-500 mt-1">Una URL por línea</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Días de Retención
                </label>
                <input
                  type="number"
                  min="1"
                  max="365"
                  value={formData.retentionDays}
                  onChange={(e) => setFormData({ ...formData, retentionDays: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">Días para mantener los reportes generados</p>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="sendOnFailure"
                  checked={formData.sendOnFailure}
                  onChange={(e) => setFormData({ ...formData, sendOnFailure: e.target.checked })}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded mr-2"
                />
                <label htmlFor="sendOnFailure" className="text-sm font-medium text-gray-700">
                  Enviar notificaciones en caso de error
                </label>
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="mt-6 pt-6 border-t border-gray-200 flex justify-end space-x-2">
            <button
              onClick={() => {
                setActiveTab('list')
                setEditingReport(null)
                resetForm()
              }}
              className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-lg"
            >
              Cancelar
            </button>
            <button
              onClick={editingReport ? () => {} : handleCreateReport}
              className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
            >
              {editingReport ? 'Actualizar' : 'Crear'} Reporte
            </button>
          </div>
        </div>
      )}

      {activeTab === 'executions' && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Ejecuciones Recientes</h3>
          <div className="text-center py-12 text-gray-500">
            <ClockIcon className="h-12 w-12 mx-auto mb-4 opacity-50" />
            <p>Historial de ejecuciones estará disponible próximamente</p>
            <p className="text-sm">Esta sección mostrará:</p>
            <ul className="text-sm mt-2 space-y-1">
              <li>• Estado de ejecuciones recientes</li>
              <li>• Archivos generados disponibles para descarga</li>
              <li>• Historial de errores y problemas</li>
              <li>• Estadísticas de rendimiento</li>
            </ul>
          </div>
        </div>
      )}
    </div>
  )
}
