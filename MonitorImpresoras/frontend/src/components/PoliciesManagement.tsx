import React, { useState, useEffect } from 'react'
import {
  ShieldCheckIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  EyeIcon,
  ClockIcon,
  CurrencyDollarIcon,
  ExclamationTriangleIcon,
  DocumentChartBarIcon
} from '@heroicons/react/24/outline'

interface Policy {
  id: number
  name: string
  description: string
  type: string
  isActive: boolean
  priority: number
  usageCount: number
  violationCount: number
  createdAt: string
}

interface PolicyFormData {
  name: string
  description: string
  type: string
  isActive: boolean
  priority: number
  conditions: {
    departments: string[]
    maxPages?: number
    allowColor?: boolean
    startTime?: string
    endTime?: string
    allowedDays: string[]
  }
  actions: {
    action: string
    customMessage?: string
    costMultiplier?: number
  }
}

export default function PoliciesManagement() {
  const [policies, setPolicies] = useState<Policy[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingPolicy, setEditingPolicy] = useState<Policy | null>(null)
  const [activeTab, setActiveTab] = useState<'list' | 'create' | 'statistics'>('list')
  const [formData, setFormData] = useState<PolicyFormData>({
    name: '',
    description: '',
    type: 'PageLimit',
    isActive: true,
    priority: 100,
    conditions: {
      departments: [],
      allowedDays: []
    },
    actions: {
      action: 'Allow'
    }
  })

  const policyTypes = [
    'PageLimit',
    'ColorRestriction',
    'TimeRestriction',
    'CostControl',
    'DepartmentQuota',
    'UserQuota',
    'PrinterAccess',
    'PaperRestriction',
    'ApprovalRequired'
  ]

  const actions = [
    'Allow',
    'Block',
    'Warn',
    'RequireApproval',
    'ModifyCost'
  ]

  const departments = [
    'Ventas',
    'Marketing',
    'IT',
    'RRHH',
    'Finanzas',
    'Otro'
  ]

  const daysOfWeek = [
    'Lunes',
    'Martes',
    'Miércoles',
    'Jueves',
    'Viernes',
    'Sábado',
    'Domingo'
  ]

  useEffect(() => {
    loadPolicies()
  }, [])

  const loadPolicies = async () => {
    try {
      setLoading(true)
      const response = await fetch('/api/policies', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (response.ok) {
        const data = await response.json()
        setPolicies(data)
      }
    } catch (error) {
      console.error('Error loading policies:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleCreatePolicy = async () => {
    try {
      const response = await fetch('/api/policies', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(formData)
      })

      if (response.ok) {
        await loadPolicies()
        setShowCreateForm(false)
        resetForm()
      }
    } catch (error) {
      console.error('Error creating policy:', error)
    }
  }

  const handleUpdatePolicy = async () => {
    if (!editingPolicy) return

    try {
      const response = await fetch(`/api/policies/${editingPolicy.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(formData)
      })

      if (response.ok) {
        await loadPolicies()
        setEditingPolicy(null)
        resetForm()
      }
    } catch (error) {
      console.error('Error updating policy:', error)
    }
  }

  const handleDeletePolicy = async (id: number) => {
    if (!confirm('¿Está seguro de que desea eliminar esta política?')) return

    try {
      const response = await fetch(`/api/policies/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (response.ok) {
        await loadPolicies()
      }
    } catch (error) {
      console.error('Error deleting policy:', error)
    }
  }

  const handleToggleStatus = async (id: number, isActive: boolean) => {
    try {
      const response = await fetch(`/api/policies/${id}/status`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ isActive })
      })

      if (response.ok) {
        await loadPolicies()
      }
    } catch (error) {
      console.error('Error toggling policy status:', error)
    }
  }

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      type: 'PageLimit',
      isActive: true,
      priority: 100,
      conditions: {
        departments: [],
        allowedDays: []
      },
      actions: {
        action: 'Allow'
      }
    })
  }

  const startEdit = (policy: Policy) => {
    setEditingPolicy(policy)
    setFormData({
      name: policy.name,
      description: policy.description,
      type: policy.type,
      isActive: policy.isActive,
      priority: policy.priority,
      conditions: {
        departments: [],
        allowedDays: []
      },
      actions: {
        action: 'Allow'
      }
    })
  }

  const getPolicyTypeIcon = (type: string) => {
    switch (type) {
      case 'PageLimit':
        return <DocumentChartBarIcon className="h-5 w-5" />
      case 'ColorRestriction':
        return <DocumentChartBarIcon className="h-5 w-5" />
      case 'TimeRestriction':
        return <ClockIcon className="h-5 w-5" />
      case 'CostControl':
        return <CurrencyDollarIcon className="h-5 w-5" />
      case 'ApprovalRequired':
        return <ExclamationTriangleIcon className="h-5 w-5" />
      default:
        return <ShieldCheckIcon className="h-5 w-5" />
    }
  }

  const getPolicyTypeColor = (type: string) => {
    switch (type) {
      case 'PageLimit':
        return 'bg-blue-100 text-blue-800'
      case 'ColorRestriction':
        return 'bg-purple-100 text-purple-800'
      case 'TimeRestriction':
        return 'bg-orange-100 text-orange-800'
      case 'CostControl':
        return 'bg-green-100 text-green-800'
      case 'ApprovalRequired':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
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
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <ShieldCheckIcon className="h-8 w-8 text-blue-600 mr-3" />
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Políticas de Impresión</h2>
            <p className="text-sm text-gray-600">Gestiona reglas y restricciones de impresión</p>
          </div>
        </div>
        <div className="flex space-x-2">
          <button
            onClick={() => setActiveTab('statistics')}
            className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-lg"
          >
            Estadísticas
          </button>
          <button
            onClick={() => setShowCreateForm(true)}
            className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
          >
            Crear Política
          </button>
        </div>
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
            Lista de Políticas
          </button>
          <button
            onClick={() => setActiveTab('create')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'create'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            {editingPolicy ? 'Editar Política' : 'Crear Nueva'}
          </button>
          <button
            onClick={() => setActiveTab('statistics')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'statistics'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Estadísticas
          </button>
        </nav>
      </div>

      {/* Content */}
      {activeTab === 'list' && (
        <div className="space-y-4">
          {policies.length === 0 ? (
            <div className="text-center py-12">
              <ShieldCheckIcon className="h-12 w-12 mx-auto mb-4 text-gray-400" />
              <p className="text-gray-500">No hay políticas configuradas</p>
              <button
                onClick={() => setShowCreateForm(true)}
                className="mt-4 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
              >
                Crear Primera Política
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {policies.map(policy => (
                <div key={policy.id} className="bg-white rounded-lg shadow-md p-6">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex items-center">
                      {getPolicyTypeIcon(policy.type)}
                      <div className="ml-3">
                        <h3 className="text-lg font-semibold text-gray-900">{policy.name}</h3>
                        <p className="text-sm text-gray-600">{policy.description}</p>
                      </div>
                    </div>
                    <span className={`px-2 py-1 text-xs font-medium rounded-full ${getPolicyTypeColor(policy.type)}`}>
                      {policy.type}
                    </span>
                  </div>

                  <div className="space-y-2 mb-4">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Estado:</span>
                      <span className={`font-medium ${policy.isActive ? 'text-green-600' : 'text-red-600'}`}>
                        {policy.isActive ? 'Activa' : 'Inactiva'}
                      </span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Prioridad:</span>
                      <span className="font-medium">{policy.priority}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Uso:</span>
                      <span className="font-medium">{policy.usageCount}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-500">Violaciones:</span>
                      <span className="font-medium text-red-600">{policy.violationCount}</span>
                    </div>
                  </div>

                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <button
                        onClick={() => handleToggleStatus(policy.id, !policy.isActive)}
                        className={`p-2 rounded-lg ${policy.isActive ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600'}`}
                      >
                        {policy.isActive ? 'Desactivar' : 'Activar'}
                      </button>
                      <button
                        onClick={() => startEdit(policy)}
                        className="p-2 bg-blue-100 text-blue-600 rounded-lg"
                      >
                        <PencilIcon className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDeletePolicy(policy.id)}
                        className="p-2 bg-red-100 text-red-600 rounded-lg"
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
            {editingPolicy ? 'Editar Política' : 'Crear Nueva Política'}
          </h3>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Basic Information */}
            <div className="space-y-4">
              <h4 className="text-md font-medium text-gray-900">Información Básica</h4>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre de la Política
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Ej: Límite de páginas por usuario"
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
                  placeholder="Describe el propósito de esta política"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Tipo de Política
                </label>
                <select
                  value={formData.type}
                  onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {policyTypes.map(type => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Prioridad
                </label>
                <input
                  type="number"
                  min="1"
                  max="1000"
                  value={formData.priority}
                  onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">Mayor número = mayor prioridad</p>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={formData.isActive}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label htmlFor="isActive" className="ml-2 text-sm font-medium text-gray-700">
                  Política Activa
                </label>
              </div>
            </div>

            {/* Conditions */}
            <div className="space-y-4">
              <h4 className="text-md font-medium text-gray-900">Condiciones</h4>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Departamentos Aplicables
                </label>
                <div className="grid grid-cols-2 gap-2">
                  {departments.map(dept => (
                    <label key={dept} className="flex items-center">
                      <input
                        type="checkbox"
                        checked={formData.conditions.departments.includes(dept)}
                        onChange={(e) => {
                          const updated = e.target.checked
                            ? [...formData.conditions.departments, dept]
                            : formData.conditions.departments.filter(d => d !== dept)
                          setFormData({
                            ...formData,
                            conditions: { ...formData.conditions, departments: updated }
                          })
                        }}
                        className="mr-2"
                      />
                      {dept}
                    </label>
                  ))}
                </div>
              </div>

              {formData.type === 'PageLimit' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Límite de Páginas
                  </label>
                  <input
                    type="number"
                    value={formData.conditions.maxPages || ''}
                    onChange={(e) => setFormData({
                      ...formData,
                      conditions: { ...formData.conditions, maxPages: parseInt(e.target.value) || undefined }
                    })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="Máximo de páginas por trabajo"
                  />
                </div>
              )}

              {formData.type === 'TimeRestriction' && (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Hora de Inicio
                    </label>
                    <input
                      type="time"
                      value={formData.conditions.startTime || ''}
                      onChange={(e) => setFormData({
                        ...formData,
                        conditions: { ...formData.conditions, startTime: e.target.value }
                      })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Hora de Fin
                    </label>
                    <input
                      type="time"
                      value={formData.conditions.endTime || ''}
                      onChange={(e) => setFormData({
                        ...formData,
                        conditions: { ...formData.conditions, endTime: e.target.value }
                      })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Días Permitidos
                    </label>
                    <div className="grid grid-cols-2 gap-2">
                      {daysOfWeek.map(day => (
                        <label key={day} className="flex items-center">
                          <input
                            type="checkbox"
                            checked={formData.conditions.allowedDays.includes(day)}
                            onChange={(e) => {
                              const updated = e.target.checked
                                ? [...formData.conditions.allowedDays, day]
                                : formData.conditions.allowedDays.filter(d => d !== day)
                              setFormData({
                                ...formData,
                                conditions: { ...formData.conditions, allowedDays: updated }
                              })
                            }}
                            className="mr-2"
                          />
                          {day}
                        </label>
                      ))}
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Actions */}
          <div className="mt-6 pt-6 border-t border-gray-200">
            <h4 className="text-md font-medium text-gray-900 mb-4">Acciones</h4>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Acción por Defecto
                </label>
                <select
                  value={formData.actions.action}
                  onChange={(e) => setFormData({
                    ...formData,
                    actions: { ...formData.actions, action: e.target.value }
                  })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {actions.map(action => (
                    <option key={action} value={action}>{action}</option>
                  ))}
                </select>
              </div>

              {formData.actions.action === 'ModifyCost' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Multiplicador de Costo (%)
                  </label>
                  <input
                    type="number"
                    min="0"
                    max="500"
                    value={formData.actions.costMultiplier || ''}
                    onChange={(e) => setFormData({
                      ...formData,
                      actions: { ...formData.actions, costMultiplier: parseInt(e.target.value) || undefined }
                    })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="100 = costo normal, 200 = doble costo"
                  />
                </div>
              )}

              <div className="lg:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Mensaje Personalizado (opcional)
                </label>
                <textarea
                  value={formData.actions.customMessage || ''}
                  onChange={(e) => setFormData({
                    ...formData,
                    actions: { ...formData.actions, customMessage: e.target.value }
                  })}
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Mensaje que se mostrará al usuario"
                />
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="mt-6 pt-6 border-t border-gray-200 flex justify-end space-x-2">
            <button
              onClick={() => {
                setEditingPolicy(null)
                resetForm()
              }}
              className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-lg"
            >
              Cancelar
            </button>
            <button
              onClick={editingPolicy ? handleUpdatePolicy : handleCreatePolicy}
              className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
            >
              {editingPolicy ? 'Actualizar' : 'Crear'} Política
            </button>
          </div>
        </div>
      )}

      {activeTab === 'statistics' && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">Estadísticas de Políticas</h3>
          <div className="text-center py-12 text-gray-500">
            <ShieldCheckIcon className="h-12 w-12 mx-auto mb-4 opacity-50" />
            <p>Las estadísticas estarán disponibles próximamente</p>
            <p className="text-sm">Esta sección mostrará:</p>
            <ul className="text-sm mt-2 space-y-1">
              <li>• Total de evaluaciones realizadas</li>
              <li>• Políticas más utilizadas</li>
              <li>• Violaciones más comunes</li>
              <li>• Uso por departamento</li>
              <li>• Tasa de aprobación/denegación</li>
            </ul>
          </div>
        </div>
      )}

      {/* Modals would go here for create/edit forms */}
    </div>
  )
}
