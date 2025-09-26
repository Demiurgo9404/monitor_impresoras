import React from 'react'
import { PrinterStatus } from '../services/signalRService'

interface PrinterCardProps {
  printer: PrinterStatus
}

export default function PrinterCard({ printer }: PrinterCardProps) {
  const getStatusColor = (status: string, isOnline: boolean) => {
    if (!isOnline) return 'bg-red-500'
    switch (status.toLowerCase()) {
      case 'idle':
      case 'ready':
        return 'bg-green-500'
      case 'printing':
        return 'bg-blue-500'
      case 'error':
      case 'offline':
        return 'bg-red-500'
      default:
        return 'bg-yellow-500'
    }
  }

  const getStatusText = (status: string, isOnline: boolean) => {
    if (!isOnline) return 'Fuera de línea'
    switch (status.toLowerCase()) {
      case 'idle':
      case 'ready':
        return 'Lista'
      case 'printing':
        return 'Imprimiendo'
      case 'error':
        return 'Error'
      case 'offline':
        return 'Fuera de línea'
      default:
        return status
    }
  }

  const formatLastSeen = (lastSeen: string) => {
    const date = new Date(lastSeen)
    const now = new Date()
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60))

    if (diffInMinutes < 1) return 'Justo ahora'
    if (diffInMinutes < 60) return `Hace ${diffInMinutes} min`

    const diffInHours = Math.floor(diffInMinutes / 60)
    if (diffInHours < 24) return `Hace ${diffInHours}h`

    const diffInDays = Math.floor(diffInHours / 24)
    return `Hace ${diffInDays} días`
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center">
          <div className={`w-3 h-3 rounded-full mr-3 ${getStatusColor(printer.status, printer.isOnline)}`}></div>
          <h3 className="text-lg font-semibold text-gray-900">{printer.name}</h3>
        </div>
        <span className={`px-2 py-1 text-xs font-medium rounded-full ${
          printer.isOnline
            ? 'bg-green-100 text-green-800'
            : 'bg-red-100 text-red-800'
        }`}>
          {printer.isOnline ? 'En línea' : 'Fuera de línea'}
        </span>
      </div>

      <div className="space-y-2">
        <div className="flex justify-between">
          <span className="text-sm text-gray-500">Estado:</span>
          <span className="text-sm font-medium text-gray-900">
            {getStatusText(printer.status, printer.isOnline)}
          </span>
        </div>

        <div className="flex justify-between">
          <span className="text-sm text-gray-500">Última actividad:</span>
          <span className="text-sm text-gray-900">
            {formatLastSeen(printer.lastSeen)}
          </span>
        </div>
      </div>

      <div className="mt-4 pt-4 border-t border-gray-200">
        <div className="flex items-center justify-between">
          <span className="text-xs text-gray-500">ID: {printer.id}</span>
          <button
            onClick={() => console.log('Ver detalles de impresora:', printer.id)}
            className="text-xs text-blue-600 hover:text-blue-800 font-medium"
          >
            Ver detalles →
          </button>
        </div>
      </div>
    </div>
  )
}
