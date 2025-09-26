import React from 'react'
import { PrinterStatus, Alert, ConsumableStatus } from '../services/signalRService'

interface StatsOverviewProps {
  printers: PrinterStatus[]
  alerts: Alert[]
  consumables: ConsumableStatus[]
}

export default function StatsOverview({ printers, alerts, consumables }: StatsOverviewProps) {
  const onlinePrinters = printers.filter(p => p.isOnline).length
  const offlinePrinters = printers.length - onlinePrinters
  const activeAlerts = alerts.filter(a => a.status === 'Active').length
  const lowConsumables = consumables.filter(c => c.status === 'Low' || c.status === 'Critical').length

  const stats = [
    {
      name: 'Impresoras Totales',
      value: printers.length.toString(),
      icon: '🖨️',
      color: 'blue'
    },
    {
      name: 'En Línea',
      value: onlinePrinters.toString(),
      icon: '🟢',
      color: 'green'
    },
    {
      name: 'Fuera de Línea',
      value: offlinePrinters.toString(),
      icon: '🔴',
      color: 'red'
    },
    {
      name: 'Alertas Activas',
      value: activeAlerts.toString(),
      icon: '⚠️',
      color: 'yellow'
    },
    {
      name: 'Consumibles Bajos',
      value: lowConsumables.toString(),
      icon: '📉',
      color: 'orange'
    }
  ]

  return (
    <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-5">
      {stats.map((stat) => (
        <div
          key={stat.name}
          className={`relative overflow-hidden rounded-lg bg-white px-4 py-5 shadow sm:px-6 sm:pt-6 ${
            stat.color === 'blue' ? 'border-l-4 border-blue-500' :
            stat.color === 'green' ? 'border-l-4 border-green-500' :
            stat.color === 'red' ? 'border-l-4 border-red-500' :
            stat.color === 'yellow' ? 'border-l-4 border-yellow-500' :
            'border-l-4 border-orange-500'
          }`}
        >
          <dt className="text-sm font-medium text-gray-500 truncate">
            {stat.name}
          </dt>
          <dd className="mt-1 text-3xl font-semibold text-gray-900">
            <div className="flex items-center">
              <span className="text-2xl mr-2">{stat.icon}</span>
              {stat.value}
            </div>
          </dd>
        </div>
      ))}
    </div>
  )
}
