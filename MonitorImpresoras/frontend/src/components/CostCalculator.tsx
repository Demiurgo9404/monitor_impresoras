import React, { useState } from 'react'
import { CalculatorIcon, CurrencyDollarIcon, ChartBarIcon } from '@heroicons/react/24/outline'

interface CostEstimateRequest {
  pages: number
  isColor: boolean
  isDuplex: boolean
  paperSize: string
  paperType: string
  userId: string
  department: string
  coveragePercentage: number
}

interface CostEstimateResponse {
  totalCost: number
  baseCost: number
  colorSurcharge: number
  duplexDiscount: number
  paperSurcharge: number
  departmentDiscount: number
  userDiscount: number
  appliedPolicies: string[]
  currency: string
}

export default function CostCalculator() {
  const [request, setRequest] = useState<CostEstimateRequest>({
    pages: 1,
    isColor: false,
    isDuplex: false,
    paperSize: 'Carta',
    paperType: 'Normal',
    userId: '',
    department: '',
    coveragePercentage: 1.0
  })

  const [result, setResult] = useState<CostEstimateResponse | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleInputChange = (field: keyof CostEstimateRequest, value: any) => {
    setRequest(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const calculateCost = async () => {
    if (request.pages <= 0) {
      setError('El número de páginas debe ser mayor a 0')
      return
    }

    setLoading(true)
    setError('')

    try {
      const response = await fetch('/api/cost/estimate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(request)
      })

      if (!response.ok) {
        throw new Error('Error al calcular el costo')
      }

      const data = await response.json()
      setResult(data)
    } catch (err) {
      setError('Error al calcular el costo. Verifica tu conexión.')
      console.error('Error:', err)
    } finally {
      setLoading(false)
    }
  }

  const resetCalculator = () => {
    setResult(null)
    setError('')
  }

  const paperSizes = ['Carta', 'Oficio', 'A4', 'A3', 'Legal']
  const paperTypes = ['Normal', 'Brillante', 'Mate', 'Reciclado']
  const departments = ['Ventas', 'Marketing', 'IT', 'RRHH', 'Finanzas', 'Otro']

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center mb-6">
        <CalculatorIcon className="h-8 w-8 text-blue-600 mr-3" />
        <div>
          <h2 className="text-xl font-semibold text-gray-900">Calculadora de Costos</h2>
          <p className="text-sm text-gray-600">Calcula el costo estimado de tus impresiones</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Input Form */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Parámetros de Impresión</h3>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Número de Páginas
            </label>
            <input
              type="number"
              min="1"
              max="10000"
              value={request.pages}
              onChange={(e) => handleInputChange('pages', parseInt(e.target.value))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex items-center">
            <input
              type="checkbox"
              id="isColor"
              checked={request.isColor}
              onChange={(e) => handleInputChange('isColor', e.target.checked)}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label htmlFor="isColor" className="ml-2 text-sm font-medium text-gray-700">
              Impresión a Color
            </label>
          </div>

          <div className="flex items-center">
            <input
              type="checkbox"
              id="isDuplex"
              checked={request.isDuplex}
              onChange={(e) => handleInputChange('isDuplex', e.target.checked)}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label htmlFor="isDuplex" className="ml-2 text-sm font-medium text-gray-700">
              Impresión Dúplex (Doble Cara)
            </label>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tamaño de Papel
            </label>
            <select
              value={request.paperSize}
              onChange={(e) => handleInputChange('paperSize', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {paperSizes.map(size => (
                <option key={size} value={size}>{size}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tipo de Papel
            </label>
            <select
              value={request.paperType}
              onChange={(e) => handleInputChange('paperType', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {paperTypes.map(type => (
                <option key={type} value={type}>{type}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Departamento
            </label>
            <select
              value={request.department}
              onChange={(e) => handleInputChange('department', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Seleccionar departamento</option>
              {departments.map(dept => (
                <option key={dept} value={dept}>{dept}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Usuario ID (opcional)
            </label>
            <input
              type="text"
              value={request.userId}
              onChange={(e) => handleInputChange('userId', e.target.value)}
              placeholder="ID de usuario específico"
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Cobertura de Tinta (%) - {request.coveragePercentage * 100}%
            </label>
            <input
              type="range"
              min="0.5"
              max="2"
              step="0.1"
              value={request.coveragePercentage}
              onChange={(e) => handleInputChange('coveragePercentage', parseFloat(e.target.value))}
              className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
            />
            <div className="flex justify-between text-xs text-gray-500 mt-1">
              <span>Baja</span>
              <span>Normal</span>
              <span>Alta</span>
            </div>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          <div className="flex space-x-2">
            <button
              onClick={calculateCost}
              disabled={loading}
              className="flex-1 bg-blue-500 hover:bg-blue-600 disabled:bg-blue-300 text-white py-2 px-4 rounded-lg font-medium"
            >
              {loading ? 'Calculando...' : 'Calcular Costo'}
            </button>
            <button
              onClick={resetCalculator}
              className="bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded-lg font-medium"
            >
              Limpiar
            </button>
          </div>
        </div>

        {/* Results */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Resultado del Cálculo</h3>

          {result ? (
            <div className="space-y-4">
              {/* Total Cost */}
              <div className="bg-green-50 border-2 border-green-200 rounded-lg p-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <CurrencyDollarIcon className="h-6 w-6 text-green-600 mr-2" />
                    <span className="text-sm font-medium text-green-700">Costo Total</span>
                  </div>
                  <span className="text-2xl font-bold text-green-800">
                    ${result.totalCost.toFixed(2)} {result.currency}
                  </span>
                </div>
              </div>

              {/* Cost Breakdown */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h4 className="text-sm font-medium text-gray-900 mb-3">Desglose de Costos</h4>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Costo base ({request.pages} páginas):</span>
                    <span className="font-medium">${result.baseCost.toFixed(2)}</span>
                  </div>

                  {result.colorSurcharge > 0 && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Cargo por color:</span>
                      <span className="font-medium text-orange-600">+${result.colorSurcharge.toFixed(2)}</span>
                    </div>
                  )}

                  {result.paperSurcharge !== 0 && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Cargo por papel:</span>
                      <span className="font-medium text-blue-600">
                        {result.paperSurcharge > 0 ? '+' : ''}${result.paperSurcharge.toFixed(2)}
                      </span>
                    </div>
                  )}

                  {result.duplexDiscount > 0 && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Descuento dúplex:</span>
                      <span className="font-medium text-green-600">-${result.duplexDiscount.toFixed(2)}</span>
                    </div>
                  )}

                  {result.departmentDiscount > 0 && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Descuento departamento:</span>
                      <span className="font-medium text-green-600">-${result.departmentDiscount.toFixed(2)}</span>
                    </div>
                  )}

                  {result.userDiscount > 0 && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Descuento usuario:</span>
                      <span className="font-medium text-green-600">-${result.userDiscount.toFixed(2)}</span>
                    </div>
                  )}
                </div>

                <div className="border-t border-gray-300 mt-3 pt-3">
                  <div className="flex justify-between text-lg font-semibold">
                    <span>Total:</span>
                    <span>${result.totalCost.toFixed(2)} {result.currency}</span>
                  </div>
                </div>
              </div>

              {/* Applied Policies */}
              {result.appliedPolicies.length > 0 && (
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <h4 className="text-sm font-medium text-blue-900 mb-2">Políticas Aplicadas</h4>
                  <ul className="text-sm text-blue-800 space-y-1">
                    {result.appliedPolicies.map((policy, index) => (
                      <li key={index} className="flex items-center">
                        <ChartBarIcon className="h-4 w-4 mr-2" />
                        {policy}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          ) : (
            <div className="text-center py-12 text-gray-500">
              <CalculatorIcon className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>Ingresa los parámetros y calcula el costo de tu impresión</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
