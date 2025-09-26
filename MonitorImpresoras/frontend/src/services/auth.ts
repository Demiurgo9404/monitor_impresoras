import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Add a request interceptor to include the auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Add a response interceptor to handle token refresh
api.interceptors.response.use(
  (response) => {
    return response
  },
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      const refreshToken = localStorage.getItem('refreshToken')
      if (refreshToken) {
        try {
          const response = await axios.post(`${API_BASE_URL}/api/auth/refresh-token`, {
            refreshToken
          })

          const { token, refreshToken: newRefreshToken } = response.data
          localStorage.setItem('token', token)
          localStorage.setItem('refreshToken', newRefreshToken)

          originalRequest.headers.Authorization = `Bearer ${token}`
          return api(originalRequest)
        } catch (refreshError) {
          localStorage.removeItem('token')
          localStorage.removeItem('refreshToken')
          window.location.href = '/login'
          return Promise.reject(refreshError)
        }
      }
    }

    return Promise.reject(error)
  }
)

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  user: {
    id: string
    username: string
    email: string
    roles: string[]
  }
  token: string
  refreshToken: string
}

export interface User {
  id: string
  username: string
  email: string
  roles: string[]
}

export const login = async (credentials: LoginRequest): Promise<LoginResponse> => {
  const response = await api.post('/api/auth/login', credentials)
  return response.data
}

export const getCurrentUser = async (): Promise<User> => {
  const response = await api.get('/api/auth/me')
  return response.data
}

export const logout = async (): Promise<void> => {
  await api.post('/api/auth/logout')
}

export const refreshToken = async (refreshTokenValue: string): Promise<LoginResponse> => {
  const response = await api.post('/api/auth/refresh-token', { refreshToken: refreshTokenValue })
  return response.data
}

export default api
