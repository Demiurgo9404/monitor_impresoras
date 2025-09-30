// src/services/api.js
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para añadir el token JWT
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para manejar respuestas
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authAPI = {
  login: (credentials) => api.post('/api/auth/login', credentials),
  refreshToken: (refreshToken) => api.post('/api/auth/refresh', { refreshToken }),
  logout: () => api.post('/api/auth/logout'),
};

export const printerAPI = {
  getAll: () => api.get('/api/printers'),
  getById: (id) => api.get(`/api/printers/${id}`),
  getStatus: (id) => api.get(`/api/printers/${id}/status`),
  getDashboard: () => api.get('/api/printers/dashboard'),
  create: (printer) => api.post('/api/printers', printer),
  update: (id, printer) => api.put(`/api/printers/${id}`, printer),
  delete: (id) => api.delete(`/api/printers/${id}`),
};

export const telemetryAPI = {
  getLatest: (printerId) => api.get(`/api/telemetry/printer/${printerId}`),
  getHistory: (printerId, from, to) =>
    api.get(`/api/telemetry/printer/${printerId}/history?from=${from}&to=${to}`),
  collect: (printerId) => api.post(`/api/telemetry/collect?printerId=${printerId}`),
};

export default api;
