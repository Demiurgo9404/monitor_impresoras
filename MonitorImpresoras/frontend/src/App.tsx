import React from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Printers from './pages/Printers'
import Alerts from './pages/Alerts'
import Reports from './pages/Reports'
import Settings from './pages/Settings'
import Layout from './components/Layout'

function AppContent() {
  const { user, loading } = useAuth()

  if (loading) {
    return <div className="flex items-center justify-center min-h-screen">Cargando...</div>
  }

  return (
    <Router>
      <Routes>
        <Route path="/login" element={user ? <Navigate to="/dashboard" /> : <Login />} />
        <Route path="/" element={user ? <Layout /> : <Navigate to="/login" />}>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/printers" element={<Printers />} />
          <Route path="/alerts" element={<Alerts />} />
          <Route path="/reports" element={<Reports />} />
          <Route path="/settings" element={<Settings />} />
        </Route>
        <Route path="*" element={<Navigate to={user ? "/dashboard" : "/login"} />} />
      </Routes>
    </Router>
  )
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  )
}

export default App
