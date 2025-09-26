import React from 'react'
import { Alert } from '../services/signalRService'
import { format } from 'date-fns'
import { es } from 'date-fns/locale'

interface AlertCardProps {
  alert: Alert
}

export default function AlertCard({ alert }: AlertCardProps) {
  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'critical':
        return 'bg-red-500'
      case 'high':
        return 'bg-orange-500'
      case 'medium':
        return 'bg-yellow-500'
      case 'low':
        return 'bg-blue-500'
      default:
        return 'bg-gray-500'
    }
  }

  const getSeverityBgColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'critical':
        return 'bg-red-50 border-red-200'
      case 'high':
        return 'bg-orange-50 border-orange-200'
      case 'medium':
        return 'bg-yellow-50 border-yellow-200'
      case 'low':
        return 'bg-blue-50 border-blue-200'
      default:
        return 'bg-gray-50 border-gray-200'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
        return 'bg-red-100 text-red-800'
      case 'acknowledged':
        return 'bg-yellow-100 text-yellow-800'
      case 'resolved':
        return 'bg-green-100 text-green-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const formatDate = (dateString: string) => {
    try {
      return format(new Date(dateString), 'dd MMM yyyy, HH:mm', { locale: es })
    } catch {
      return dateString
    }
  }

  return (
    <div className={`rounded-lg border-2 p-4 ${getSeverityBgColor(alert.severity)}`}>
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center mb-2">
            <div className={`w-2 h-2 rounded-full mr-2 ${getSeverityColor(alert.severity)}`}></div>
            <h3 className="text-sm font-semibold text-gray-900">
              {alert.printerName}
            </h3>
          </div>

          <p className="text-sm text-gray-700 mb-3">
            {alert.message}
          </p>

          <div className="flex items-center justify-between text-xs text-gray-500">
            <span>{formatDate(alert.createdAt)}</span>
            <span className="text-xs text-gray-400">ID: {alert.id}</span>
          </div>
        </div>

        <div className="ml-4">
          <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(alert.status)}`}>
            {alert.status === 'Active' ? 'Activa' :
             alert.status === 'Acknowledged' ? 'Reconocida' :
             alert.status === 'Resolved' ? 'Resuelta' : alert.status}
          </span>
        </div>
      </div>

      <div className="mt-3 pt-3 border-t border-gray-200">
        <div className="flex items-center justify-between">
          <span className="text-xs text-gray-500">
            Tipo: {alert.type}
          </span>
          <div className="flex space-x-2">
            {alert.status === 'Active' && (
              <button
                onClick={() => console.log('Reconocer alerta:', alert.id)}
                className="text-xs bg-yellow-500 hover:bg-yellow-600 text-white px-2 py-1 rounded"
              >
                Reconocer
              </button>
            )}
            <button
              onClick={() => console.log('Resolver alerta:', alert.id)}
              className="text-xs bg-green-500 hover:bg-green-600 text-white px-2 py-1 rounded"
            >
              Resolver
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
