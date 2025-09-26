import React, { useState, useEffect } from 'react'
import { signalRService, PrinterStatus } from '../services/signalRService'

export default function Printers() {
  const [printers, setPrinters] = useState<PrinterStatus[]>([])
  const [loading, setLoading] = useState(true)
  const [filter, setFilter] = useState<'all' | 'online' | 'offline'>('all')

  useEffect(() => {
    initializePrinters()

    return () => {
      signalRService.off('printerStatusUpdate', handlePrinterUpdate)
    }
  }, [])

  const initializePrinters = async () => {
    try {
      setLoading(true)
      await signalRService.startConnection()

      signalRService.on('printerStatusUpdate', handlePrinterUpdate)

      // Request initial data
      await signalRService.requestPrinterSummary()

      // Subscribe to ongoing updates
      await signalRService.subscribeToAllPrinters()

    } catch (error) {
      console.error('Error initializing printers:', error)
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

  const filteredPrinters = printers.filter(printer => {
    if (filter === 'online') return printer.isOnline
    if (filter === 'offline') return !printer.isOnline
    return true
  })

  const getStatusBadge = (status: string, isOnline: boolean) => {
    if (!isOnline) {
      return <span className="px-2 py-1 text-xs bg-red-100 text-red-800 rounded-full">Fuera de línea</span>
    }

    switch (status.toLowerCase()) {
      case 'idle':
      case 'ready':
        return <span className="px-2 py-1 text-xs bg-green-100 text-green-800 rounded-full">Lista</span>
      case 'printing':
        return <span className="px-2 py-1 text-xs bg-blue-100 text-blue-800 rounded-full">Imprimiendo</span>
      case 'error':
        return <span className="px-2 py-1 text-xs bg-red-100 text-red-800 rounded-full">Error</span>
      default:
        return <span className="px-2 py-1 text-xs bg-gray-100 text-gray-800 rounded-full">{status}</span>
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
          <h1 className="text-2xl font-bold text-gray-900">Impresoras</h1>
          <p className="text-gray-600">Gestiona y monitorea todas tus impresoras</p>
        </div>
        <button
          onClick={() => signalRService.requestPrinterSummary()}
          className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
        >
          Actualizar
        </button>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-1 bg-gray-100 p-1 rounded-lg w-fit">
        {[
          { key: 'all', label: 'Todas', count: printers.length },
          { key: 'online', label: 'En línea', count: printers.filter(p => p.isOnline).length },
          { key: 'offline', label: 'Fuera de línea', count: printers.filter(p => !p.isOnline).length }
        ].map(({ key, label, count }) => (
          <button
            key={key}
            onClick={() => setFilter(key as 'all' | 'online' | 'offline')}
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

      {/* Printers Grid */}
      {filteredPrinters.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500">No hay impresoras que coincidan con el filtro seleccionado</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {filteredPrinters.map(printer => (
            <div key={printer.id} className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center">
                  <div className={`w-3 h-3 rounded-full mr-3 ${
                    printer.isOnline ? 'bg-green-500' : 'bg-red-500'
                  }`}></div>
                  <h3 className="text-lg font-semibold text-gray-900">{printer.name}</h3>
                </div>
                {getStatusBadge(printer.status, printer.isOnline)}
              </div>

              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-500">Estado:</span>
                  <span className="font-medium">{printer.status}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">Última actividad:</span>
                  <span className="font-medium">
                    {new Date(printer.lastSeen).toLocaleString()}
                  </span>
                </div>
              </div>

              <div className="mt-4 pt-4 border-t border-gray-200">
                <div className="flex justify-between items-center">
                  <span className="text-xs text-gray-500">ID: {printer.id}</span>
                  <div className="flex space-x-2">
                    <button
                      onClick={() => console.log('Ver detalles:', printer.id)}
                      className="text-xs bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded"
                    >
                      Detalles
                    </button>
                    <button
                      onClick={() => console.log('Configurar:', printer.id)}
                      className="text-xs bg-gray-500 hover:bg-gray-600 text-white px-3 py-1 rounded"
                    >
                      Configurar
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
