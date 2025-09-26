import React, { useState } from 'react'
import { ChartBarIcon, PlayIcon, PlusIcon } from '@heroicons/react/24/outline'

interface CostSimulationRequest {
  scenarios: Array<{
    pages: number
    isColor: boolean
    isDuplex: boolean
    paperSize: string
    paperType: string
    department: string
    coveragePercentage: number
  }>
  includeComparison: boolean
  comparisonMetric: string
}

interface CostSimulationResult {
  results: Array<{
    totalCost: number
    baseCost: number
    colorSurcharge: number
    duplexDiscount: number
    paperSurcharge: number
    departmentDiscount: number
    userDiscount: number
    currency: string
  }>
  comparison: {
    metric: string
    items: Array<{
      scenarioIndex: number
      scenarioDescription: string
      value: number
      isBest: boolean
      isWorst: boolean
    }>
    bestValue: {
      scenarioIndex: number
      scenarioDescription: string
      value: number
    }
    worstValue: {
      scenarioIndex: number
      scenarioDescription: string
      value: number
    }
  }
  averageCost: number
  minimumCost: number
  maximumCost: number
  simulatedAt: string
}

export default function CostSimulation() {
  const [scenarios, setScenarios] = useState<Array<{
    name: string
    pages: number
    isColor: boolean
    isDuplex: boolean
    paperSize: string
    paperType: string
    department: string
    coveragePercentage: number
  }>>([
    {
      name: 'Escenario 1',
      pages: 10,
      isColor: false,
      isDuplex: false,
      paperSize: 'Carta',
      paperType: 'Normal',
      department: 'Ventas',
      coveragePercentage: 1.0
    }
  ])

  const [simulationResult, setSimulationResult] = useState<CostSimulationResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const paperSizes = ['Carta', 'Oficio', 'A4', 'A3', 'Legal']
  const paperTypes = ['Normal', 'Brillante', 'Mate', 'Reciclado']
  const departments = ['Ventas', 'Marketing', 'IT', 'RRHH', 'Finanzas', 'Otro']

  const addScenario = () => {
    const newScenario = {
      name: `Escenario ${scenarios.length + 1}`,
      pages: 10,
      isColor: false,
      isDuplex: false,
      paperSize: 'Carta',
      paperType: 'Normal',
      department: 'Ventas',
      coveragePercentage: 1.0
    }
    setScenarios([...scenarios, newScenario])
  }

  const updateScenario = (index: number, field: string, value: any) => {
    const updatedScenarios = scenarios.map((scenario, i) =>
      i === index ? { ...scenario, [field]: value } : scenario
    )
    setScenarios(updatedScenarios)
  }

  const removeScenario = (index: number) => {
    if (scenarios.length > 1) {
      setScenarios(scenarios.filter((_, i) => i !== index))
    }
  }

  const runSimulation = async () => {
    setLoading(true)
    setError('')

    try {
      const requestData: CostSimulationRequest = {
        scenarios: scenarios.map(s => ({
          pages: s.pages,
          isColor: s.isColor,
          isDuplex: s.isDuplex,
          paperSize: s.paperSize,
          paperType: s.paperType,
          department: s.department,
          coveragePercentage: s.coveragePercentage
        })),
        includeComparison: true,
        comparisonMetric: 'TotalCost'
      }

      const response = await fetch('/api/cost/simulate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(requestData)
      })

      if (!response.ok) {
        throw new Error('Error al ejecutar la simulación')
      }

      const data = await response.json()
      setSimulationResult(data)
    } catch (err) {
      setError('Error al ejecutar la simulación. Verifica tu conexión.')
      console.error('Error:', err)
    } finally {
      setLoading(false)
    }
  }

  const resetSimulation = () => {
    setSimulationResult(null)
    setError('')
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <ChartBarIcon className="h-8 w-8 text-blue-600 mr-3" />
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Simulación de Costos</h2>
            <p className="text-sm text-gray-600">Compara múltiples escenarios de impresión</p>
          </div>
        </div>
        <button
          onClick={addScenario}
          className="bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-lg font-medium flex items-center"
        >
          <PlusIcon className="h-4 w-4 mr-2" />
          Agregar Escenario
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Scenarios Configuration */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium text-gray-900">Escenarios de Simulación</h3>

          {scenarios.map((scenario, index) => (
            <div key={index} className="bg-gray-50 rounded-lg p-4">
              <div className="flex items-center justify-between mb-3">
                <h4 className="font-medium text-gray-900">{scenario.name}</h4>
                {scenarios.length > 1 && (
                  <button
                    onClick={() => removeScenario(index)}
                    className="text-red-500 hover:text-red-700 text-sm"
                  >
                    Eliminar
                  </button>
                )}
              </div>

              <div className="grid grid-cols-2 gap-3 text-sm">
                <div>
                  <label className="block text-gray-600 mb-1">Páginas</label>
                  <input
                    type="number"
                    min="1"
                    value={scenario.pages}
                    onChange={(e) => updateScenario(index, 'pages', parseInt(e.target.value))}
                    className="w-full px-2 py-1 border border-gray-300 rounded text-sm"
                  />
                </div>

                <div>
                  <label className="block text-gray-600 mb-1">Departamento</label>
                  <select
                    value={scenario.department}
                    onChange={(e) => updateScenario(index, 'department', e.target.value)}
                    className="w-full px-2 py-1 border border-gray-300 rounded text-sm"
                  >
                    {departments.map(dept => (
                      <option key={dept} value={dept}>{dept}</option>
                    ))}
                  </select>
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id={`color-${index}`}
                    checked={scenario.isColor}
                    onChange={(e) => updateScenario(index, 'isColor', e.target.checked)}
                    className="mr-2"
                  />
                  <label htmlFor={`color-${index}`} className="text-gray-600">Color</label>
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id={`duplex-${index}`}
                    checked={scenario.isDuplex}
                    onChange={(e) => updateScenario(index, 'isDuplex', e.target.checked)}
                    className="mr-2"
                  />
                  <label htmlFor={`duplex-${index}`} className="text-gray-600">Dúplex</label>
                </div>

                <div>
                  <label className="block text-gray-600 mb-1">Papel</label>
                  <select
                    value={scenario.paperSize}
                    onChange={(e) => updateScenario(index, 'paperSize', e.target.value)}
                    className="w-full px-2 py-1 border border-gray-300 rounded text-sm"
                  >
                    {paperSizes.map(size => (
                      <option key={size} value={size}>{size}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-gray-600 mb-1">Tipo</label>
                  <select
                    value={scenario.paperType}
                    onChange={(e) => updateScenario(index, 'paperType', e.target.value)}
                    className="w-full px-2 py-1 border border-gray-300 rounded text-sm"
                  >
                    {paperTypes.map(type => (
                      <option key={type} value={type}>{type}</option>
                    ))}
                  </select>
                </div>
              </div>
            </div>
          ))}

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          <div className="flex space-x-2">
            <button
              onClick={runSimulation}
              disabled={loading}
              className="flex-1 bg-blue-500 hover:bg-blue-600 disabled:bg-blue-300 text-white py-2 px-4 rounded-lg font-medium flex items-center justify-center"
            >
              {loading ? (
                'Ejecutando...'
              ) : (
                <>
                  <PlayIcon className="h-4 w-4 mr-2" />
                  Ejecutar Simulación
                </>
              )}
            </button>
            <button
              onClick={resetSimulation}
              className="bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded-lg font-medium"
            >
              Limpiar
            </button>
          </div>
        </div>

        {/* Results */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium text-gray-900">Resultados de Simulación</h3>

          {simulationResult ? (
            <div className="space-y-4">
              {/* Summary Stats */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h4 className="text-sm font-medium text-blue-900 mb-3">Resumen</h4>
                <div className="grid grid-cols-3 gap-4 text-sm">
                  <div className="text-center">
                    <div className="text-lg font-bold text-blue-900">${simulationResult.averageCost.toFixed(2)}</div>
                    <div className="text-blue-700">Promedio</div>
                  </div>
                  <div className="text-center">
                    <div className="text-lg font-bold text-green-600">${simulationResult.minimumCost.toFixed(2)}</div>
                    <div className="text-green-700">Mínimo</div>
                  </div>
                  <div className="text-center">
                    <div className="text-lg font-bold text-red-600">${simulationResult.maximumCost.toFixed(2)}</div>
                    <div className="text-red-700">Máximo</div>
                  </div>
                </div>
              </div>

              {/* Comparison Table */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h4 className="text-sm font-medium text-gray-900 mb-3">Comparación</h4>
                <div className="space-y-2">
                  {simulationResult.comparison.items.map((item, index) => (
                    <div
                      key={index}
                      className={`flex items-center justify-between p-2 rounded ${
                        item.isBest ? 'bg-green-100 border border-green-300' :
                        item.isWorst ? 'bg-red-100 border border-red-300' :
                        'bg-white border border-gray-200'
                      }`}
                    >
                      <div className="flex-1">
                        <div className="font-medium text-gray-900">{scenarios[item.scenarioIndex]?.name}</div>
                        <div className="text-sm text-gray-600">{item.scenarioDescription}</div>
                      </div>
                      <div className="text-right">
                        <div className={`font-bold ${
                          item.isBest ? 'text-green-600' :
                          item.isWorst ? 'text-red-600' : 'text-gray-900'
                        }`}>
                          ${item.value.toFixed(2)}
                        </div>
                        {item.isBest && <div className="text-xs text-green-600">★ Mejor opción</div>}
                        {item.isWorst && <div className="text-xs text-red-600">⚠ Peor opción</div>}
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Detailed Results */}
              <div className="space-y-2">
                {simulationResult.results.map((result, index) => (
                  <div key={index} className="bg-white border border-gray-200 rounded-lg p-3">
                    <div className="flex items-center justify-between">
                      <div>
                        <h5 className="font-medium text-gray-900">{scenarios[index]?.name}</h5>
                        <p className="text-sm text-gray-600">
                          {scenarios[index]?.pages} páginas, {scenarios[index]?.isColor ? 'Color' : 'B/N'}
                          {scenarios[index]?.isDuplex ? ', Dúplex' : ', Simplex'}
                        </p>
                      </div>
                      <div className="text-right">
                        <div className="text-lg font-bold text-blue-600">
                          ${result.totalCost.toFixed(2)}
                        </div>
                        <div className="text-xs text-gray-500">
                          Base: ${result.baseCost.toFixed(2)}
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className="text-center py-12 text-gray-500">
              <ChartBarIcon className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>Configura escenarios y ejecuta la simulación</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
