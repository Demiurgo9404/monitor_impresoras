// src/components/PrinterDetail.jsx
import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { printerAPI, telemetryAPI } from '../services/api';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  BarChart,
  Bar
} from 'recharts';

const PrinterDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [printer, setPrinter] = useState(null);
  const [telemetry, setTelemetry] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState('overview');
  const [timeRange, setTimeRange] = useState('24h');

  useEffect(() => {
    loadPrinterData();
  }, [id]);

  const loadPrinterData = async () => {
    try {
      setLoading(true);

      // Cargar datos de la impresora
      const printerResponse = await printerAPI.getById(id);
      setPrinter(printerResponse.data);

      // Cargar telemetría histórica
      const endDate = new Date();
      const startDate = new Date();

      switch (timeRange) {
        case '1h':
          startDate.setHours(endDate.getHours() - 1);
          break;
        case '24h':
          startDate.setHours(endDate.getHours() - 24);
          break;
        case '7d':
          startDate.setDate(endDate.getDate() - 7);
          break;
        case '30d':
          startDate.setDate(endDate.getDate() - 30);
          break;
        default:
          startDate.setHours(endDate.getHours() - 24);
      }

      const telemetryResponse = await telemetryAPI.getHistory(id, startDate.toISOString(), endDate.toISOString());
      setTelemetry(telemetryResponse.data);
    } catch (error) {
      setError(error.response?.data?.message || 'Error al cargar datos de la impresora');
    } finally {
      setLoading(false);
    }
  };

  const handleStatusCheck = async () => {
    try {
      const response = await printerAPI.getStatus(id);
      setPrinter(prev => ({ ...prev, ...response.data }));
    } catch (error) {
      setError(error.response?.data?.message || 'Error al verificar estado');
    }
  };

  const handleCollectTelemetry = async () => {
    try {
      await telemetryAPI.collect(id);
      loadPrinterData(); // Recargar datos
    } catch (error) {
      setError(error.response?.data?.message || 'Error al recolectar telemetría');
    }
  };

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'online':
        return 'bg-green-100 text-green-800';
      case 'offline':
        return 'bg-red-100 text-red-800';
      case 'error':
        return 'bg-yellow-100 text-yellow-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (error || !printer) {
    return (
      <div className="text-center py-12">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error || 'Impresora no encontrada'}
        </div>
        <button
          onClick={() => navigate('/printers')}
          className="mt-4 bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700"
        >
          Volver al listado
        </button>
      </div>
    );
  }

  // Preparar datos para gráficos
  const chartData = telemetry.map(t => ({
    time: new Date(t.timestampUtc).toLocaleTimeString(),
    tonerLevel: t.tonerLevel,
    paperLevel: t.paperLevel,
    temperature: t.temperature,
    pagesPrinted: t.pagesPrinted,
    errorsCount: t.errorsCount
  }));

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <button
            onClick={() => navigate('/printers')}
            className="text-gray-600 hover:text-gray-900"
          >
            ← Volver al listado
          </button>
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{printer.name}</h1>
            <p className="text-gray-600">{printer.model} - {printer.location}</p>
          </div>
        </div>
        <div className="flex space-x-2">
          <button
            onClick={handleStatusCheck}
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
          >
            Verificar Estado
          </button>
          <button
            onClick={handleCollectTelemetry}
            className="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700"
          >
            Recolectar Datos
          </button>
        </div>
      </div>

      {/* Información general */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center">
            <div className={`w-4 h-4 rounded-full mr-3 ${printer.isOnline ? 'bg-green-500' : 'bg-red-500'}`}></div>
            <div>
              <p className="text-sm font-medium text-gray-600">Estado</p>
              <p className={`text-lg font-semibold ${printer.isOnline ? 'text-green-600' : 'text-red-600'}`}>
                {printer.isOnline ? 'En línea' : 'Fuera de línea'}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center">
            <div className="w-4 h-4 bg-blue-500 rounded-full mr-3"></div>
            <div>
              <p className="text-sm font-medium text-gray-600">Estado del Sistema</p>
              <p className="text-lg font-semibold">{printer.status || 'Desconocido'}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center">
            <div className="w-4 h-4 bg-purple-500 rounded-full mr-3"></div>
            <div>
              <p className="text-sm font-medium text-gray-600">Nivel de Tóner</p>
              <p className={`text-lg font-semibold ${printer.tonerLevel < 20 ? 'text-red-600' : 'text-green-600'}`}>
                {printer.tonerLevel}%
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center">
            <div className="w-4 h-4 bg-orange-500 rounded-full mr-3"></div>
            <div>
              <p className="text-sm font-medium text-gray-600">Nivel de Papel</p>
              <p className={`text-lg font-semibold ${printer.paperLevel < 10 ? 'text-red-600' : 'text-green-600'}`}>
                {printer.paperLevel}%
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <nav className="-mb-px flex space-x-8 px-6">
            {[
              { id: 'overview', label: 'Resumen' },
              { id: 'metrics', label: 'Métricas' },
              { id: 'history', label: 'Historial' }
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`py-4 px-1 border-b-2 font-medium text-sm ${
                  activeTab === tab.id
                    ? 'border-indigo-500 text-indigo-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        <div className="p-6">
          {activeTab === 'overview' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <h3 className="text-lg font-medium text-gray-900 mb-4">Información General</h3>
                  <dl className="space-y-3">
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Nombre</dt>
                      <dd className="text-sm text-gray-900">{printer.name}</dd>
                    </div>
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Modelo</dt>
                      <dd className="text-sm text-gray-900">{printer.model}</dd>
                    </div>
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Ubicación</dt>
                      <dd className="text-sm text-gray-900">{printer.location}</dd>
                    </div>
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Dirección IP</dt>
                      <dd className="text-sm text-gray-900">{printer.ipAddress || 'No configurada'}</dd>
                    </div>
                    <div>
                      <dt className="text-sm font-medium text-gray-500">Estado Activo</dt>
                      <dd className="text-sm text-gray-900">
                        <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                          printer.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }`}>
                          {printer.isActive ? 'Activa' : 'Inactiva'}
                        </span>
                      </dd>
                    </div>
                  </dl>
                </div>

                <div>
                  <h3 className="text-lg font-medium text-gray-900 mb-4">Últimas Métricas</h3>
                  {printer.lastTelemetry ? (
                    <dl className="space-y-3">
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Páginas Impresas</dt>
                        <dd className="text-sm text-gray-900">{printer.lastTelemetry.pagesPrinted}</dd>
                      </div>
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Errores</dt>
                        <dd className="text-sm text-gray-900">{printer.lastTelemetry.errorsCount}</dd>
                      </div>
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Temperatura</dt>
                        <dd className="text-sm text-gray-900">{printer.lastTelemetry.temperature}°C</dd>
                      </div>
                      <div>
                        <dt className="text-sm font-medium text-gray-500">Última Actualización</dt>
                        <dd className="text-sm text-gray-900">
                          {new Date(printer.lastTelemetry.timestampUtc).toLocaleString()}
                        </dd>
                      </div>
                    </dl>
                  ) : (
                    <p className="text-sm text-gray-500">No hay métricas disponibles</p>
                  )}
                </div>
              </div>
            </div>
          )}

          {activeTab === 'metrics' && (
            <div className="space-y-6">
              <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium text-gray-900">Métricas en Tiempo Real</h3>
                <select
                  value={timeRange}
                  onChange={(e) => {
                    setTimeRange(e.target.value);
                    loadPrinterData();
                  }}
                  className="border border-gray-300 rounded-md px-3 py-1 text-sm"
                >
                  <option value="1h">Última hora</option>
                  <option value="24h">Últimas 24 horas</option>
                  <option value="7d">Últimos 7 días</option>
                  <option value="30d">Últimos 30 días</option>
                </select>
              </div>

              {chartData.length > 0 ? (
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-md font-medium text-gray-900 mb-4">Niveles de Consumibles</h4>
                    <ResponsiveContainer width="100%" height={300}>
                      <LineChart data={chartData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="time" />
                        <YAxis />
                        <Tooltip />
                        <Legend />
                        <Line type="monotone" dataKey="tonerLevel" stroke="#8884d8" name="Tóner (%)" />
                        <Line type="monotone" dataKey="paperLevel" stroke="#82ca9d" name="Papel (%)" />
                      </LineChart>
                    </ResponsiveContainer>
                  </div>

                  <div>
                    <h4 className="text-md font-medium text-gray-900 mb-4">Actividad y Errores</h4>
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={chartData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="time" />
                        <YAxis />
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="pagesPrinted" fill="#ffc658" name="Páginas Impresas" />
                        <Bar dataKey="errorsCount" fill="#ff7300" name="Errores" />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                </div>
              ) : (
                <p className="text-center text-gray-500 py-8">No hay datos de métricas disponibles</p>
              )}
            </div>
          )}

          {activeTab === 'history' && (
            <div>
              <h3 className="text-lg font-medium text-gray-900 mb-4">Historial de Métricas</h3>
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Fecha y Hora
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Tóner
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Papel
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Páginas
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Errores
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Estado
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {telemetry.slice(-10).map((record, index) => (
                      <tr key={index}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {new Date(record.timestampUtc).toLocaleString()}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {record.tonerLevel}%
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {record.paperLevel}%
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {record.pagesPrinted}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {record.errorsCount}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(record.status)}`}>
                            {record.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PrinterDetail;
