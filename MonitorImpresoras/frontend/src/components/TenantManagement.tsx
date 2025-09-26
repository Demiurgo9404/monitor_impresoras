import React, { useState, useEffect } from 'react'
import {
  BuildingOfficeIcon,
  PlusIcon,
  PencilIcon,
  TrashIcon,
  EyeIcon,
  UserGroupIcon,
  PrinterIcon,
  ChartBarIcon,
  CogIcon
} from '@heroicons/react/24/outline'

interface Tenant {
  id: number
  name: string
  companyName: string
  databaseName: string
  isActive: boolean
  createdAt: string
  lastAccess: string
  adminEmail: string
  timezone: string
  currency: string
  maxPrinters: number
  maxUsers: number
  maxPolicies: number
  maxStorageBytes: number
  currentPrinters: number
  currentUsers: number
  currentStorageBytes: number
  currentSubscription: {
    planName: string
    status: string
    daysRemaining: number
    isTrial: boolean
  } | null
}

export default function TenantManagement() {
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingTenant, setEditingTenant] = useState<Tenant | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [formData, setFormData] = useState({
    name: '',
    companyName: '',
    adminEmail: '',
    timezone: 'America/Mexico_City',
    currency: 'MXN'
  })

  useEffect(() => {
    loadTenants()
  }, [statusFilter])

  const loadTenants = async () => {
    try {
      setLoading(true)
      const params = new URLSearchParams()
      if (statusFilter !== 'all') {
        params.append('status', statusFilter)
      }

      const response = await fetch(`/api/tenants?${params}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (response.ok) {
        const data = await response.json()
        setTenants(data)
      }
    } catch (error) {
      console.error('Error loading tenants:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleCreateTenant = async () => {
    try {
      const response = await fetch('/api/tenants', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(formData)
      })

      if (response.ok) {
        await loadTenants()
        setShowCreateForm(false)
        resetForm()
      }
    } catch (error) {
      console.error('Error creating tenant:', error)
    }
  }

  const handleToggleStatus = async (tenantId: number, isActive: boolean) => {
    try {
      const response = await fetch(`/api/tenants/${tenantId}/status`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ isActive })
      })

      if (response.ok) {
        await loadTenants()
      }
    } catch (error) {
      console.error('Error toggling tenant status:', error)
    }
  }

  const handleDeleteTenant = async (tenantId: number) => {
    if (!confirm('¿Está seguro de que desea eliminar este tenant? Esta acción no se puede deshacer.')) {
      return
    }

    try {
      const response = await fetch(`/api/tenants/${tenantId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (response.ok) {
        await loadTenants()
      }
    } catch (error) {
      console.error('Error deleting tenant:', error)
    }
  }

  const resetForm = () => {
    setFormData({
      name: '',
      companyName: '',
      adminEmail: '',
      timezone: 'America/Mexico_City',
      currency: 'MXN'
    })
  }

  const filteredTenants = tenants.filter(tenant =>
    tenant.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    tenant.companyName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    tenant.adminEmail.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const getStatusColor = (isActive: boolean) => {
    return isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
  }

  const getSubscriptionStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'bg-green-100 text-green-800'
      case 'trial':
        return 'bg-blue-100 text-blue-800'
      case 'cancelled':
        return 'bg-red-100 text-red-800'
      case 'expired':
        return 'bg-gray-100 text-gray-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getUsagePercentage = (current: number, max: number) => {
    if (max === 0) return 0
    return Math.min((current / max) * 100, 100)
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
          <BuildingOfficeIcon className="h-8 w-8 text-blue-600 mr-3" />
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Gestión de Tenants</h2>
            <p className="text-sm text-gray-600">Administra todos los clientes y sus suscripciones</p>
          </div>
        </div>
        <button
          onClick={() => setShowCreateForm(true)}
          className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg flex items-center"
        >
          <PlusIcon className="h-4 w-4 mr-2" />
          Crear Tenant
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Buscar
            </label>
            <input
              type="text"
              placeholder="Buscar por nombre, empresa o email..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Estado del Tenant
            </label>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">Todos</option>
              <option value="active">Activos</option>
              <option value="inactive">Inactivos</option>
            </select>
          </div>

          <div className="flex items-end">
            <button
              onClick={loadTenants}
              className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-lg w-full"
            >
              Actualizar
            </button>
          </div>
        </div>
      </div>

      {/* Tenants Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredTenants.map(tenant => (
          <div key={tenant.id} className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-start justify-between mb-4">
              <div className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900">{tenant.name}</h3>
                <p className="text-sm text-gray-600">{tenant.companyName}</p>
                <p className="text-xs text-gray-500">{tenant.adminEmail}</p>
              </div>
              <div className="flex items-center space-x-2">
                <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(tenant.isActive)}`}>
                  {tenant.isActive ? 'Activo' : 'Inactivo'}
                </span>
              </div>
            </div>

            {/* Subscription Status */}
            {tenant.currentSubscription && (
              <div className="mb-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">Plan</span>
                  <span className="text-sm text-gray-900">{tenant.currentSubscription.planName}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">Estado</span>
                  <span className={`px-2 py-1 text-xs font-medium rounded-full ${getSubscriptionStatusColor(tenant.currentSubscription.status)}`}>
                    {tenant.currentSubscription.status}
                  </span>
                </div>
                {!tenant.currentSubscription.isTrial && (
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-700">Días restantes</span>
                    <span className="text-sm text-gray-900">{tenant.currentSubscription.daysRemaining}</span>
                  </div>
                )}
              </div>
            )}

            {/* Usage Stats */}
            <div className="space-y-3 mb-4">
              <div>
                <div className="flex justify-between text-sm mb-1">
                  <span className="flex items-center">
                    <UserGroupIcon className="h-4 w-4 mr-1" />
                    Usuarios
                  </span>
                  <span>{tenant.currentUsers} / {tenant.maxUsers}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-blue-600 h-2 rounded-full"
                    style={{ width: `${getUsagePercentage(tenant.currentUsers, tenant.maxUsers)}%` }}
                  />
                </div>
              </div>

              <div>
                <div className="flex justify-between text-sm mb-1">
                  <span className="flex items-center">
                    <PrinterIcon className="h-4 w-4 mr-1" />
                    Impresoras
                  </span>
                  <span>{tenant.currentPrinters} / {tenant.maxPrinters}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-green-600 h-2 rounded-full"
                    style={{ width: `${getUsagePercentage(tenant.currentPrinters, tenant.maxPrinters)}%` }}
                  />
                </div>
              </div>

              <div>
                <div className="flex justify-between text-sm mb-1">
                  <span className="flex items-center">
                    <ChartBarIcon className="h-4 w-4 mr-1" />
                    Almacenamiento
                  </span>
                  <span>{(tenant.currentStorageBytes / (1024 * 1024)).toFixed(1)} / {tenant.maxStorageBytes / (1024 * 1024)} MB</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-purple-600 h-2 rounded-full"
                    style={{ width: `${getUsagePercentage(tenant.currentStorageBytes, tenant.maxStorageBytes)}%` }}
                  />
                </div>
              </div>
            </div>

            {/* Last Access */}
            <div className="text-xs text-gray-500 mb-4">
              Creado: {new Date(tenant.createdAt).toLocaleDateString()}
              {tenant.lastAccess && (
                <div>Último acceso: {new Date(tenant.lastAccess).toLocaleDateString()}</div>
              )}
            </div>

            {/* Actions */}
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <button
                  onClick={() => handleToggleStatus(tenant.id, !tenant.isActive)}
                  className={`p-2 rounded-lg ${tenant.isActive ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600'}`}
                  title={tenant.isActive ? 'Desactivar tenant' : 'Activar tenant'}
                >
                  <CogIcon className="h-4 w-4" />
                </button>
                <button
                  className="p-2 bg-gray-100 text-gray-600 rounded-lg"
                  title="Ver detalles"
                >
                  <EyeIcon className="h-4 w-4" />
                </button>
                <button
                  onClick={() => handleDeleteTenant(tenant.id)}
                  className="p-2 bg-red-100 text-red-600 rounded-lg"
                  title="Eliminar tenant"
                >
                  <TrashIcon className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {filteredTenants.length === 0 && (
        <div className="text-center py-12">
          <BuildingOfficeIcon className="h-12 w-12 mx-auto mb-4 text-gray-400" />
          <p className="text-gray-500">No se encontraron tenants</p>
          <button
            onClick={() => setShowCreateForm(true)}
            className="mt-4 bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
          >
            Crear Primer Tenant
          </button>
        </div>
      )}

      {/* Create Tenant Modal */}
      {showCreateForm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold mb-4">Crear Nuevo Tenant</h3>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre del Tenant *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Ej: empresa-cliente"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre de la Empresa
                </label>
                <input
                  type="text"
                  value={formData.companyName}
                  onChange={(e) => setFormData({ ...formData, companyName: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Nombre de la empresa"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email del Administrador *
                </label>
                <input
                  type="email"
                  value={formData.adminEmail}
                  onChange={(e) => setFormData({ ...formData, adminEmail: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="admin@empresa.com"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Zona Horaria
                </label>
                <select
                  value={formData.timezone}
                  onChange={(e) => setFormData({ ...formData, timezone: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="America/Mexico_City">Ciudad de México</option>
                  <option value="America/New_York">Nueva York</option>
                  <option value="Europe/London">Londres</option>
                  <option value="Asia/Tokyo">Tokio</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Moneda
                </label>
                <select
                  value={formData.currency}
                  onChange={(e) => setFormData({ ...formData, currency: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="MXN">Peso Mexicano (MXN)</option>
                  <option value="USD">Dólar Americano (USD)</option>
                  <option value="EUR">Euro (EUR)</option>
                </select>
              </div>
            </div>

            <div className="mt-6 flex space-x-2">
              <button
                onClick={() => {
                  setShowCreateForm(false)
                  resetForm()
                }}
                className="flex-1 bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded"
              >
                Cancelar
              </button>
              <button
                onClick={handleCreateTenant}
                className="flex-1 bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded"
              >
                Crear Tenant
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
