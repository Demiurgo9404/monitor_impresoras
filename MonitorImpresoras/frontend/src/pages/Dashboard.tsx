import React, { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { signalRService, PrinterStatus, Alert, ConsumableStatus } from '../services/signalRService'
import PrinterCard from '../components/PrinterCard'
import AlertCard from '../components/AlertCard'
import CostChart from '../components/CostChart'
import StatsOverview from '../components/StatsOverview'

interface PrinterSummary {
  totalPrinters: number
  onlinePrinters: number
  offlinePrinters: number
  printers: PrinterStatus[]
}

interface AlertsSummary {
  totalAlerts: number
  activeAlerts: number
  acknowledgedAlerts: number
  resolvedAlerts: number
  recentAlerts: Alert[]
}

interface ConsumablesSummary {
  stats: {
    totalConsumables: number
    lowConsumables: number
    criticalConsumables: number
  }
  lowConsumables: ConsumableStatus[]
}

export default function Dashboard() {
  const { user } = useAuth()
  const [printers, setPrinters] = useState<PrinterStatus[]>([])
  const [alerts, setAlerts] = useState<Alert[]>([])
  const [consumables, setConsumables] = useState<ConsumableStatus[]>([])
  const [printerSummary, setPrinterSummary] = useState<PrinterSummary | null>(null)
  const [alertsSummary, setAlertsSummary] = useState<AlertsSummary | null>(null)
  const [consumablesSummary, setConsumablesSummary] = useState<ConsumablesSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    initializeDashboard()

    return () => {
      signalRService.off('printerStatusUpdate', handlePrinterUpdate)
      signalRService.off('newAlert', handleNewAlert)
      signalRService.off('alertAcknowledged', handleAlertAcknowledged)
      signalRService.off('alertResolved', handleAlertResolved)
      signalRService.off('consumableStatusUpdate', handleConsumableUpdate)
      signalRService.off('error', handleError)
    }
  }, [])

  const initializeDashboard = async () => {
    try {
      setLoading(true)
      await signalRService.startConnection()

      // Subscribe to all relevant events
      signalRService.on('printerStatusUpdate', handlePrinterUpdate)
      signalRService.on('newAlert', handleNewAlert)
      signalRService.on('alertAcknowledged', handleAlertAcknowledged)
      signalRService.on('alertResolved', handleAlertResolved)
      signalRService.on('consumableStatusUpdate', handleConsumableUpdate)
      signalRService.on('error', handleError)

      // Request initial data
      await Promise.all([
        signalRService.requestPrinterSummary(),
        signalRService.requestAlertsSummary(),
        signalRService.requestConsumablesSummary()
      ])

      // Subscribe to ongoing updates
      await Promise.all([
        signalRService.subscribeToAllPrinters(),
        signalRService.subscribeToAlerts(),
        signalRService.subscribeToConsumables()
      ])

    } catch (error) {
      console.error('Error initializing dashboard:', error)
      setError('Error al conectar con el servidor')
    } finally {
      setLoading(false)
    }
  }

  const handlePrinterUpdate = (printer: PrinterStatus) => {
    setPrinters(prev => {
      const existingIndex = prev.findIndex(p => p.id === printer.id)
      if (existingIndex >= 0) {
        const updated = [...prev]
        updated[existingIndex] = printer
        return updated
      } else {
        return [...prev, printer]
      }
    })
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

  const handleConsumableUpdate = (consumable: ConsumableStatus) => {
    setConsumables(prev => {
      const existingIndex = prev.findIndex(c =>
        c.printerId === consumable.printerId && c.consumableType === consumable.consumableType
      )
      if (existingIndex >= 0) {
        const updated = [...prev]
        updated[existingIndex] = consumable
        return updated
      } else {
        return [...prev, consumable]
      }
    })
  }

  const handleError = (errorMessage: string) => {
    setError(errorMessage)
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500"></div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-red-600 mb-4">Error</h2>
          <p className="text-gray-600 mb-4">{error}</p>
          <button
            onClick={initializeDashboard}
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
          >
            Reintentar
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                Monitor de Impresoras
              </h1>
              <p className="mt-1 text-sm text-gray-500">
                Bienvenido, {user?.username}
              </p>
            </div>
            <div className="flex items-center space-x-4">
              <div className="text-sm text-gray-500">
                Estado: <span className="text-green-500 font-medium">Conectado</span>
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        {/* Stats Overview */}
        <div className="mb-8">
          <StatsOverview
            printers={printers}
            alerts={alerts}
            consumables={consumables}
          />
        </div>

        {/* Charts Row */}
        <div className="mb-8">
          <CostChart />
        </div>

        {/* Printers Grid */}
        <div className="mb-8">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-xl font-semibold text-gray-900">
              Estado de Impresoras
            </h2>
            <button
              onClick={() => signalRService.requestPrinterSummary()}
              className="bg-blue-500 hover:bg-blue-700 text-white text-sm font-medium py-2 px-4 rounded"
            >
              Actualizar
            </button>
          </div>

          {printers.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-500">No hay impresoras configuradas</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {printers.map(printer => (
                <PrinterCard key={printer.id} printer={printer} />
              ))}
            </div>
          )}
        </div>

        {/* Alerts Section */}
        <div className="mb-8">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-xl font-semibold text-gray-900">
              Alertas Recientes
            </h2>
            <button
              onClick={() => signalRService.requestAlertsSummary()}
              className="bg-yellow-500 hover:bg-yellow-700 text-white text-sm font-medium py-2 px-4 rounded"
            >
              Actualizar
            </button>
          </div>

          {alerts.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-500">No hay alertas activas</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {alerts.slice(0, 6).map(alert => (
                <AlertCard key={alert.id} alert={alert} />
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  )
}
