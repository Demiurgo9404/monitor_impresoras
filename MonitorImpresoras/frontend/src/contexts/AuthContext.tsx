import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { login as apiLogin, getCurrentUser, logout as apiLogout, refreshToken as apiRefreshToken } from '../services/auth'

export interface User {
  id: string
  username: string
  email: string
  roles: string[]
}

interface AuthContextType {
  user: User | null
  loading: boolean
  login: (username: string, password: string) => Promise<void>
  logout: () => Promise<void>
  refreshToken: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    checkAuthStatus()
  }, [])

  const checkAuthStatus = async () => {
    try {
      const token = localStorage.getItem('token')
      if (token) {
        const userData = await getCurrentUser()
        setUser(userData)
      }
    } catch (error) {
      console.error('Error checking auth status:', error)
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
    } finally {
      setLoading(false)
    }
  }

  const login = async (username: string, password: string) => {
    try {
      const response = await apiLogin(username, password)
      localStorage.setItem('token', response.token)
      localStorage.setItem('refreshToken', response.refreshToken)
      setUser(response.user)
    } catch (error) {
      throw error
    }
  }

  const logout = async () => {
    try {
      await apiLogout()
    } catch (error) {
      console.error('Error during logout:', error)
    } finally {
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      setUser(null)
    }
  }

  const refreshToken = async () => {
    try {
      const refreshTokenValue = localStorage.getItem('refreshToken')
      if (refreshTokenValue) {
        const response = await apiRefreshToken(refreshTokenValue)
        localStorage.setItem('token', response.token)
        localStorage.setItem('refreshToken', response.refreshToken)
        setUser(response.user)
      }
    } catch (error) {
      console.error('Error refreshing token:', error)
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      setUser(null)
      throw error
    }
  }

  const value: AuthContextType = {
    user,
    loading,
    login,
    logout,
    refreshToken
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}
