import React from 'react'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement,
} from 'chart.js'
import { Bar, Doughnut, Line } from 'react-chartjs-2'

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  PointElement,
  LineElement
)

export default function CostChart() {
  // Mock data - en un entorno real esto vendría de la API
  const costData = {
    labels: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'],
    datasets: [
      {
        label: 'Costo Total ($)',
        data: [1200, 1900, 1500, 2100, 1800, 2400],
        backgroundColor: 'rgba(59, 130, 246, 0.5)',
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 1,
      },
    ],
  }

  const departmentData = {
    labels: ['Ventas', 'Marketing', 'IT', 'RRHH', 'Finanzas'],
    datasets: [
      {
        label: 'Costo por Departamento',
        data: [450, 320, 280, 190, 150],
        backgroundColor: [
          'rgba(255, 99, 132, 0.5)',
          'rgba(54, 162, 235, 0.5)',
          'rgba(255, 205, 86, 0.5)',
          'rgba(75, 192, 192, 0.5)',
          'rgba(153, 102, 255, 0.5)',
        ],
        borderColor: [
          'rgb(255, 99, 132)',
          'rgb(54, 162, 235)',
          'rgb(255, 205, 86)',
          'rgb(75, 192, 192)',
          'rgb(153, 102, 255)',
        ],
        borderWidth: 1,
      },
    ],
  }

  const trendData = {
    labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
    datasets: [
      {
        label: 'Páginas impresas',
        data: [120, 150, 180, 200, 170, 140, 90],
        borderColor: 'rgb(75, 192, 192)',
        backgroundColor: 'rgba(75, 192, 192, 0.2)',
        tension: 0.1,
      },
    ],
  }

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: true,
        text: 'Análisis de Costos',
      },
    },
  }

  const doughnutOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'bottom' as const,
      },
      title: {
        display: true,
        text: 'Distribución por Departamento',
      },
    },
  }

  const trendOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: true,
        text: 'Tendencia de Impresión (Última Semana)',
      },
    },
  }

  return (
    <div className="bg-white p-6 rounded-lg shadow">
      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">
          Análisis de Costos de Impresión
        </h2>
        <div className="text-sm text-gray-600">
          <p>Resumen del período actual</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Cost Chart */}
        <div className="lg:col-span-1">
          <Bar options={options} data={costData} />
        </div>

        {/* Department Distribution */}
        <div className="lg:col-span-1">
          <Doughnut options={doughnutOptions} data={departmentData} />
        </div>

        {/* Trend Chart */}
        <div className="lg:col-span-1">
          <Line options={trendOptions} data={trendData} />
        </div>
      </div>

      {/* Summary Stats */}
      <div className="mt-6 grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-blue-50 p-4 rounded-lg">
          <div className="text-sm text-blue-600 font-medium">Costo Total</div>
          <div className="text-2xl font-bold text-blue-900">$2,400</div>
          <div className="text-xs text-blue-500">Este mes</div>
        </div>
        <div className="bg-green-50 p-4 rounded-lg">
          <div className="text-sm text-green-600 font-medium">Páginas Impresas</div>
          <div className="text-2xl font-bold text-green-900">1,050</div>
          <div className="text-xs text-green-500">Este mes</div>
        </div>
        <div className="bg-yellow-50 p-4 rounded-lg">
          <div className="text-sm text-yellow-600 font-medium">Costo por Página</div>
          <div className="text-2xl font-bold text-yellow-900">$2.29</div>
          <div className="text-xs text-yellow-500">Promedio</div>
        </div>
        <div className="bg-purple-50 p-4 rounded-lg">
          <div className="text-sm text-purple-600 font-medium">Ahorro Potencial</div>
          <div className="text-2xl font-bold text-purple-900">$180</div>
          <div className="text-xs text-purple-500">Optimizando</div>
        </div>
      </div>
    </div>
  )
}
