import React, { useState, useEffect } from 'react'
import { signalRService, Alert } from '../services/signalRService'
import AlertCard from '../components/AlertCard'

export default function Alerts() {
  const [alerts, setAlerts] = useState<Alert[]>([])
  const [loading, setLoading] = useState(true)
  const [filter, setFilter] = useState<'all' | 'active' | 'acknowledged' | 'resolved'>('all')

  useEffect(() => {
    initializeAlerts()

    return () => {
      signalRService.off('newAlert', handleNewAlert)
      signalRService.off('alertAcknowledged', handleAlertAcknowledged)
      signalRService.off('alertResolved', handleAlertResolved)
    }
  }, [])

  const initializeAlerts = async () => {
    try {
      setLoading(true)
      await signalRService.startConnection()

      signalRService.on('newAlert', handleNewAlert)
      signalRService.on('alertAcknowledged', handleAlertAcknowledged)
      signalRService.on('alertResolved', handleAlertResolved)

      // Request initial data
      await signalRService.requestAlertsSummary()

      // Subscribe to ongoing updates
      await signalRService.subscribeToAlerts()

    } catch (error) {
      console.error('Error initializing alerts:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleNewAlert = (alert: Alert) => {
    setAlerts(prev => [alert, ...prev])
  }

  const handleAlertAcknowledged = (alertId: number) => {
    setAlerts(prev =>
      prev.map(alert =>
        alert.id === alertId ? { ...alert, status: 'Acknowledged' as const } : alert
      )
    )
  }

  const handleAlertResolved = (alertId: number) => {
    setAlerts(prev =>
      prev.map(alert =>
        alert.id === alertId ? { ...alert, status: 'Resolved' as const } : alert
      )
    )
  }

  const filteredAlerts = alerts.filter(alert => {
    if (filter === 'active') return alert.status === 'Active'
    if (filter === 'acknowledged') return alert.status === 'Acknowledged'
    if (filter === 'resolved') return alert.status === 'Resolved'
    return true
  })

  const acknowledgeAlert = async (alertId: number) => {
    try {
      // Aquí iría la llamada a la API para reconocer la alerta
      console.log('Reconociendo alerta:', alertId)
      // Simular actualización inmediata
      handleAlertAcknowledged(alertId)
    } catch (error) {
      console.error('Error al reconocer alerta:', error)
    }
  }

  const resolveAlert = async (alertId: number) => {
    try {
      // Aquí iría la llamada a la API para resolver la alerta
      console.log('Resolviendo alerta:', alertId)
      // Simular actualización inmediata
      handleAlertResolved(alertId)
    } catch (error) {
      console.error('Error al resolver alerta:', error)
    }
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
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Alertas</h1>
          <p className="text-gray-600">Monitorea y gestiona las alertas del sistema</p>
        </div>
        <button
          onClick={() => signalRService.requestAlertsSummary()}
          className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
        >
          Actualizar
        </button>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-1 bg-gray-100 p-1 rounded-lg w-fit">
        {[
          { key: 'all', label: 'Todas', count: alerts.length },
          { key: 'active', label: 'Activas', count: alerts.filter(a => a.status === 'Active').length },
          { key: 'acknowledged', label: 'Reconocidas', count: alerts.filter(a => a.status === 'Acknowledged').length },
          { key: 'resolved', label: 'Resueltas', count: alerts.filter(a => a.status === 'Resolved').length }
        ].map(({ key, label, count }) => (
          <button
            key={key}
            onClick={() => setFilter(key as 'all' | 'active' | 'acknowledged' | 'resolved')}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              filter === key
                ? 'bg-white text-gray-900 shadow'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            {label} ({count})
          </button>
        ))}
      </div>

      {/* Alerts List */}
      {filteredAlerts.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500">No hay alertas que coincidan con el filtro seleccionado</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredAlerts.map(alert => (
            <div key={alert.id} className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <div className="flex items-center mb-2">
                    <h3 className="text-lg font-semibold text-gray-900">
                      {alert.printerName}
                    </h3>
                  </div>
                  <p className="text-gray-700 mb-3">
                    {alert.message}
                  </p>
                  <div className="flex items-center justify-between text-sm text-gray-500 mb-3">
                    <span>{new Date(alert.createdAt).toLocaleString()}</span>
                    <span className="text-xs">ID: {alert.id}</span>
                  </div>
                </div>
                <div className="ml-4">
                  <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                    alert.status === 'Active' ? 'bg-red-100 text-red-800' :
                    alert.status === 'Acknowledged' ? 'bg-yellow-100 text-yellow-800' :
                    'bg-green-100 text-green-800'
                  }`}>
                    {alert.status === 'Active' ? 'Activa' :
                     alert.status === 'Acknowledged' ? 'Reconocida' : 'Resuelta'}
                  </span>
                </div>
              </div>

              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Tipo:</span>
                  <span className="font-medium">{alert.type}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Severidad:</span>
                  <span className={`font-medium ${
                    alert.severity === 'Critical' ? 'text-red-600' :
                    alert.severity === 'High' ? 'text-orange-600' :
                    alert.severity === 'Medium' ? 'text-yellow-600' : 'text-blue-600'
                  }`}>
                    {alert.severity}
                  </span>
                </div>
              </div>

              <div className="mt-4 pt-4 border-t border-gray-200">
                <div className="flex space-x-2">
                  {alert.status === 'Active' && (
                    <button
                      onClick={() => acknowledgeAlert(alert.id)}
                      className="flex-1 bg-yellow-500 hover:bg-yellow-600 text-white text-sm font-medium py-2 px-4 rounded"
                    >
                      Reconocer
                    </button>
                  )}
                  <button
                    onClick={() => resolveAlert(alert.id)}
                    className="flex-1 bg-green-500 hover:bg-green-600 text-white text-sm font-medium py-2 px-4 rounded"
                  >
                    Resolver
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
