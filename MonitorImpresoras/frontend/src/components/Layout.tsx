import React from 'react'
import { Outlet, Link, useLocation } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import {
  HomeIcon,
  PrinterIcon,
  ExclamationTriangleIcon,
  DocumentChartBarIcon,
  CogIcon,
  ArrowRightOnRectangleIcon
} from '@heroicons/react/24/outline'

export default function Layout() {
  const { user, logout } = useAuth()
  const location = useLocation()

  const navigation = [
    { name: 'Dashboard', href: '/dashboard', icon: HomeIcon },
    { name: 'Impresoras', href: '/printers', icon: PrinterIcon },
    { name: 'Alertas', href: '/alerts', icon: ExclamationTriangleIcon },
    { name: 'Reportes', href: '/reports', icon: DocumentChartBarIcon },
    { name: 'Configuración', href: '/settings', icon: CogIcon },
  ]

  const handleLogout = async () => {
    await logout()
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Sidebar */}
      <div className="fixed inset-y-0 left-0 z-50 w-64 bg-white shadow-lg">
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div className="flex items-center h-16 px-6 bg-blue-600">
            <PrinterIcon className="h-8 w-8 text-white" />
            <span className="ml-3 text-xl font-bold text-white">
              Monitor
            </span>
          </div>

          {/* Navigation */}
          <nav className="flex-1 px-4 py-6 space-y-2">
            {navigation.map((item) => {
              const isActive = location.pathname === item.href
              return (
                <Link
                  key={item.name}
                  to={item.href}
                  className={`flex items-center px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
                    isActive
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                  }`}
                >
                  <item.icon className="mr-3 h-5 w-5" />
                  {item.name}
                </Link>
              )
            })}
          </nav>

          {/* User info and logout */}
          <div className="p-4 border-t">
            <div className="flex items-center mb-4">
              <div className="flex-1">
                <p className="text-sm font-medium text-gray-900">{user?.username}</p>
                <p className="text-xs text-gray-500">{user?.email}</p>
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center w-full px-4 py-2 text-sm font-medium text-gray-600 rounded-lg hover:bg-gray-100 hover:text-gray-900"
            >
              <ArrowRightOnRectangleIcon className="mr-3 h-5 w-5" />
              Cerrar Sesión
            </button>
          </div>
        </div>
      </div>

      {/* Main content */}
      <div className="pl-64">
        <main className="p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
