import React, { useState, useEffect } from 'react'
import {
  CreditCardIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  ArrowUpIcon,
  ArrowDownIcon
} from '@heroicons/react/24/outline'

interface Plan {
  id: number
  name: string
  description: string
  type: string
  monthlyPrice: number
  yearlyPrice: number
  currency: string
  maxPrinters: number
  maxUsers: number
  maxPolicies: number
  maxStorageMB: number
  hasCostCalculation: boolean
  hasAdvancedPolicies: boolean
  hasScheduledReports: boolean
  hasApiAccess: boolean
  hasCustomReports: boolean
  hasWhiteLabel: boolean
  hasPrioritySupport: boolean
  features: Array<{
    featureName: string
    featureDescription: string
    isEnabled: boolean
  }>
}

interface Subscription {
  id: number
  tenantId: number
  tenantName: string
  planId: number
  planName: string
  status: string
  startDate: string
  endDate: string
  trialEndDate: string
  isTrial: boolean
  daysRemaining: number
  price: number
  currency: string
  billingCycle: string
  createdAt: string
  cancelledAt: string
  cancellationReason: string
  recentPayments: Array<{
    amount: number
    currency: string
    status: string
    paymentDate: string
    paymentMethod: string
  }>
}

interface Tenant {
  id: number
  name: string
  companyName: string
  currentUsers: number
  maxUsers: number
  currentPrinters: number
  maxPrinters: number
  currentStorageMB: number
  maxStorageMB: number
  subscription?: {
    planName: string
    status: string
    daysRemaining: number
    isTrial: boolean
  }
}

export default function SubscriptionManagement() {
  const [tenant, setTenant] = useState<Tenant | null>(null)
  const [subscription, setSubscription] = useState<Subscription | null>(null)
  const [plans, setPlans] = useState<Plan[]>([])
  const [loading, setLoading] = useState(true)
  const [showChangePlan, setShowChangePlan] = useState(false)
  const [showCancelSubscription, setShowCancelSubscription] = useState(false)
  const [selectedPlan, setSelectedPlan] = useState<Plan | null>(null)
  const [billingCycle, setBillingCycle] = useState<'monthly' | 'yearly'>('monthly')

  useEffect(() => {
    loadTenantData()
    loadPlans()
  }, [])

  const loadTenantData = async () => {
    try {
      setLoading(true)
      // Obtener ID del tenant del usuario actual (en un sistema real, esto vendría del contexto de autenticación)
      const tenantId = 1 // Por simplicidad, usar tenant 1

      const [tenantResponse, subscriptionResponse] = await Promise.all([
        fetch(`/api/tenants/${tenantId}`, {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }),
        fetch(`/api/subscriptions/tenant/${tenantId}`, {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        })
      ])

      if (tenantResponse.ok) {
        const tenantData = await tenantResponse.json()
        setTenant(tenantData)
      }

      if (subscriptionResponse.ok) {
        const subscriptionData = await subscriptionResponse.json()
        setSubscription(subscriptionData)
      }
    } catch (error) {
      console.error('Error loading tenant data:', error)
    } finally {
      setLoading(false)
    }
  }

  const loadPlans = async () => {
    try {
      const response = await fetch('/api/plans', {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (response.ok) {
        const plansData = await response.json()
        setPlans(plansData)
      }
    } catch (error) {
      console.error('Error loading plans:', error)
    }
  }

  const handleChangePlan = async () => {
    if (!selectedPlan || !subscription) return

    try {
      const response = await fetch('/api/subscriptions/change-plan', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({
          subscriptionId: subscription.id,
          newPlanId: selectedPlan.id,
          newBillingCycle: billingCycle
        })
      })

      if (response.ok) {
        await loadTenantData()
        setShowChangePlan(false)
        setSelectedPlan(null)
      }
    } catch (error) {
      console.error('Error changing plan:', error)
    }
  }

  const handleCancelSubscription = async (reason: string, immediate: boolean = false) => {
    if (!subscription) return

    try {
      const response = await fetch('/api/subscriptions/cancel', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({
          subscriptionId: subscription.id,
          reason: reason,
          immediate: immediate
        })
      })

      if (response.ok) {
        await loadTenantData()
        setShowCancelSubscription(false)
      }
    } catch (error) {
      console.error('Error cancelling subscription:', error)
    }
  }

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'bg-green-100 text-green-800'
      case 'trial':
        return 'bg-blue-100 text-blue-800'
      case 'cancelled':
        return 'bg-red-100 text-red-800'
      case 'expired':
        return 'bg-gray-100 text-gray-800'
      case 'past_due':
        return 'bg-yellow-100 text-yellow-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'active':
        return <CheckCircleIcon className="h-5 w-5" />
      case 'trial':
        return <ClockIcon className="h-5 w-5" />
      case 'cancelled':
        return <XCircleIcon className="h-5 w-5" />
      case 'expired':
        return <ExclamationTriangleIcon className="h-5 w-5" />
      default:
        return <ClockIcon className="h-5 w-5" />
    }
  }

  const calculateYearlyPrice = (monthlyPrice: number) => {
    return monthlyPrice * 12 * 0.8 // 20% discount for yearly
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
          <CreditCardIcon className="h-8 w-8 text-blue-600 mr-3" />
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Suscripción y Planes</h2>
            <p className="text-sm text-gray-600">Gestiona tu suscripción y planes disponibles</p>
          </div>
        </div>
        {subscription?.status === 'active' && (
          <button
            onClick={() => setShowCancelSubscription(true)}
            className="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg"
          >
            Cancelar Suscripción
          </button>
        )}
      </div>

      {/* Current Subscription */}
      {subscription && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Suscripción Actual</h3>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Plan Actual</span>
                <span className="text-lg font-bold text-gray-900">{subscription.planName}</span>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Estado</span>
                <div className="flex items-center space-x-2">
                  {getStatusIcon(subscription.status)}
                  <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(subscription.status)}`}>
                    {subscription.status.charAt(0).toUpperCase() + subscription.status.slice(1)}
                  </span>
                </div>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Ciclo de Facturación</span>
                <span className="text-sm text-gray-900">{subscription.billingCycle}</span>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Precio</span>
                <span className="text-lg font-bold text-gray-900">
                  ${subscription.price} {subscription.currency}
                </span>
              </div>

              {subscription.isTrial && (
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">Días de Prueba Restantes</span>
                  <span className="text-lg font-bold text-orange-600">{subscription.daysRemaining}</span>
                </div>
              )}

              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Fecha de Inicio</span>
                <span className="text-sm text-gray-900">{new Date(subscription.startDate).toLocaleDateString()}</span>
              </div>

              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Próxima Renovación</span>
                <span className="text-sm text-gray-900">{new Date(subscription.endDate).toLocaleDateString()}</span>
              </div>
            </div>

            <div className="space-y-4">
              <h4 className="text-md font-medium text-gray-900">Uso Actual vs Límites</h4>

              {tenant && (
                <div className="space-y-3">
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span>Usuarios</span>
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
                      <span>Impresoras</span>
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
                      <span>Almacenamiento</span>
                      <span>{tenant.currentStorageMB} / {tenant.maxStorageMB} MB</span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-purple-600 h-2 rounded-full"
                        style={{ width: `${getUsagePercentage(tenant.currentStorageMB, tenant.maxStorageMB)}%` }}
                      />
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>

          <div className="mt-6 pt-6 border-t border-gray-200 flex justify-end space-x-2">
            <button
              onClick={() => setShowChangePlan(true)}
              className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg"
            >
              Cambiar Plan
            </button>
            {subscription.status === 'cancelled' && (
              <button
                onClick={() => handleCancelSubscription('Reactivando', false)}
                className="bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-lg"
              >
                Reactivar Suscripción
              </button>
            )}
          </div>
        </div>
      )}

      {/* Available Plans */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Planes Disponibles</h3>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {plans.map(plan => (
            <div
              key={plan.id}
              className={`border rounded-lg p-6 ${
                subscription?.planName === plan.name
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-200 hover:border-gray-300'
              }`}
            >
              <div className="text-center mb-4">
                <h4 className="text-lg font-semibold text-gray-900">{plan.name}</h4>
                <p className="text-sm text-gray-600 mt-1">{plan.description}</p>
              </div>

              <div className="text-center mb-6">
                <div className="flex items-center justify-center">
                  <span className="text-3xl font-bold text-gray-900">
                    ${plan.monthlyPrice}
                  </span>
                  <span className="text-gray-600 ml-1">/mes</span>
                </div>
                <div className="text-sm text-gray-500">
                  ${calculateYearlyPrice(plan.monthlyPrice)}/año (20% ahorro)
                </div>
              </div>

              <div className="space-y-2 mb-6">
                <div className="flex justify-between text-sm">
                  <span>Impresoras</span>
                  <span>{plan.maxPrinters}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span>Usuarios</span>
                  <span>{plan.maxUsers}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span>Almacenamiento</span>
                  <span>{plan.maxStorageMB} MB</span>
                </div>
              </div>

              <div className="space-y-2 mb-6">
                {plan.features.map((feature, index) => (
                  <div key={index} className="flex items-center text-sm">
                    <CheckCircleIcon className="h-4 w-4 text-green-500 mr-2" />
                    <span>{feature.featureDescription}</span>
                  </div>
                ))}
              </div>

              {subscription?.planName !== plan.name && (
                <button
                  onClick={() => {
                    setSelectedPlan(plan)
                    setShowChangePlan(true)
                  }}
                  className="w-full bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded-lg"
                >
                  {subscription ? 'Cambiar a este Plan' : 'Seleccionar Plan'}
                </button>
              )}

              {subscription?.planName === plan.name && (
                <div className="w-full bg-green-100 text-green-800 py-2 px-4 rounded-lg text-center font-medium">
                  Plan Actual
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Change Plan Modal */}
      {showChangePlan && selectedPlan && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold mb-4">Cambiar a {selectedPlan.name}</h3>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Ciclo de Facturación
                </label>
                <div className="flex space-x-4">
                  <label className="flex items-center">
                    <input
                      type="radio"
                      value="monthly"
                      checked={billingCycle === 'monthly'}
                      onChange={(e) => setBillingCycle(e.target.value as 'monthly')}
                      className="mr-2"
                    />
                    Mensual - ${selectedPlan.monthlyPrice}
                  </label>
                  <label className="flex items-center">
                    <input
                      type="radio"
                      value="yearly"
                      checked={billingCycle === 'yearly'}
                      onChange={(e) => setBillingCycle(e.target.value as 'yearly')}
                      className="mr-2"
                    />
                    Anual - ${calculateYearlyPrice(selectedPlan.monthlyPrice)}
                  </label>
                </div>
              </div>

              <div className="bg-gray-50 p-4 rounded-lg">
                <h4 className="font-medium text-gray-900 mb-2">Resumen del Cambio</h4>
                <div className="space-y-1 text-sm">
                  <div className="flex justify-between">
                    <span>Plan Anterior:</span>
                    <span>{subscription?.planName}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Plan Nuevo:</span>
                    <span>{selectedPlan.name}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Ciclo:</span>
                    <span>{billingCycle === 'monthly' ? 'Mensual' : 'Anual'}</span>
                  </div>
                  <div className="flex justify-between font-medium">
                    <span>Nuevo Precio:</span>
                    <span>
                      ${billingCycle === 'monthly' ? selectedPlan.monthlyPrice : calculateYearlyPrice(selectedPlan.monthlyPrice)}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <div className="mt-6 flex space-x-2">
              <button
                onClick={() => {
                  setShowChangePlan(false)
                  setSelectedPlan(null)
                }}
                className="flex-1 bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded"
              >
                Cancelar
              </button>
              <button
                onClick={handleChangePlan}
                className="flex-1 bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded"
              >
                Confirmar Cambio
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Cancel Subscription Modal */}
      {showCancelSubscription && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-semibold mb-4">Cancelar Suscripción</h3>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Razón de Cancelación
                </label>
                <select className="w-full px-3 py-2 border border-gray-300 rounded-md">
                  <option value="">Seleccionar razón</option>
                  <option value="cost">Costo muy alto</option>
                  <option value="features">No necesito todas las características</option>
                  <option value="competitor">Encontré una mejor alternativa</option>
                  <option value="temporary">Pausa temporal del negocio</option>
                  <option value="technical">Problemas técnicos</option>
                  <option value="other">Otra razón</option>
                </select>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="immediate"
                  className="mr-2"
                />
                <label htmlFor="immediate" className="text-sm">
                  Cancelar inmediatamente (perder acceso inmediato)
                </label>
              </div>

              <div className="bg-yellow-50 border border-yellow-200 p-4 rounded-lg">
                <div className="flex">
                  <ExclamationTriangleIcon className="h-5 w-5 text-yellow-400 mr-2" />
                  <div className="text-sm">
                    <p className="font-medium text-yellow-800">¿Estás seguro?</p>
                    <p className="text-yellow-700">
                      Si cancelas tu suscripción, perderás acceso a las características premium.
                      Puedes reactivar en cualquier momento.
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="mt-6 flex space-x-2">
              <button
                onClick={() => setShowCancelSubscription(false)}
                className="flex-1 bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded"
              >
                Mantener Suscripción
              </button>
              <button
                onClick={() => handleCancelSubscription('Razón seleccionada', false)}
                className="flex-1 bg-red-500 hover:bg-red-600 text-white py-2 px-4 rounded"
              >
                Cancelar Suscripción
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
