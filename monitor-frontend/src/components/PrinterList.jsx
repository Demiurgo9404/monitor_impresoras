// src/components/PrinterList.jsx
import { useState, useEffect } from 'react';
import { printerAPI } from '../services/api';
import PrinterCard from './PrinterCard';
import PrinterForm from './PrinterForm';

const PrinterList = () => {
  const [printers, setPrinters] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingPrinter, setEditingPrinter] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');

  useEffect(() => {
    loadPrinters();
  }, []);

  const loadPrinters = async () => {
    try {
      setLoading(true);
      const response = await printerAPI.getAll();
      setPrinters(response.data);
    } catch (error) {
      setError(error.response?.data?.message || 'Error al cargar impresoras');
    } finally {
      setLoading(false);
    }
  };

  const handleCreatePrinter = async (printerData) => {
    try {
      await printerAPI.create(printerData);
      setShowForm(false);
      loadPrinters();
    } catch (error) {
      setError(error.response?.data?.message || 'Error al crear impresora');
    }
  };

  const handleUpdatePrinter = async (printerData) => {
    try {
      await printerAPI.update(editingPrinter.id, printerData);
      setEditingPrinter(null);
      loadPrinters();
    } catch (error) {
      setError(error.response?.data?.message || 'Error al actualizar impresora');
    }
  };

  const handleDeletePrinter = async (id) => {
    if (window.confirm('¿Estás seguro de que deseas eliminar esta impresora?')) {
      try {
        await printerAPI.delete(id);
        loadPrinters();
      } catch (error) {
        setError(error.response?.data?.message || 'Error al eliminar impresora');
      }
    }
  };

  const handleEditPrinter = (printer) => {
    setEditingPrinter(printer);
  };

  const filteredPrinters = printers.filter(printer => {
    const matchesSearch = printer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         printer.model.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         printer.location.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesFilter = filterStatus === 'all' ||
                         (filterStatus === 'online' && printer.isOnline) ||
                         (filterStatus === 'offline' && !printer.isOnline) ||
                         (filterStatus === 'error' && printer.status === 'Error');

    return matchesSearch && matchesFilter;
  });

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Gestión de Impresoras</h2>
          <p className="mt-1 text-sm text-gray-600">
            Administra y monitorea todas las impresoras del sistema
          </p>
        </div>
        <button
          onClick={() => setShowForm(true)}
          className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
        >
          + Nueva Impresora
        </button>
      </div>

      {/* Filtros y búsqueda */}
      <div className="bg-white p-4 rounded-lg shadow">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label htmlFor="search" className="block text-sm font-medium text-gray-700">
              Buscar
            </label>
            <input
              type="text"
              id="search"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Nombre, modelo o ubicación..."
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            />
          </div>
          <div>
            <label htmlFor="status-filter" className="block text-sm font-medium text-gray-700">
              Estado
            </label>
            <select
              id="status-filter"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            >
              <option value="all">Todas</option>
              <option value="online">En línea</option>
              <option value="offline">Fuera de línea</option>
              <option value="error">Con errores</option>
            </select>
          </div>
          <div className="flex items-end">
            <button
              onClick={loadPrinters}
              className="bg-gray-500 text-white px-4 py-2 rounded-md hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-500"
            >
              Actualizar
            </button>
          </div>
        </div>
      </div>

      {/* Error message */}
      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
          <button
            onClick={() => setError('')}
            className="float-right font-bold"
          >
            ×
          </button>
        </div>
      )}

      {/* Lista de impresoras */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredPrinters.map((printer) => (
          <PrinterCard
            key={printer.id}
            printer={printer}
            onEdit={handleEditPrinter}
            onDelete={handleDeletePrinter}
          />
        ))}
      </div>

      {filteredPrinters.length === 0 && !loading && (
        <div className="text-center py-12">
          <p className="text-gray-500">No se encontraron impresoras</p>
        </div>
      )}

      {/* Modal para crear/editar impresora */}
      {(showForm || editingPrinter) && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-medium text-gray-900">
                {editingPrinter ? 'Editar Impresora' : 'Nueva Impresora'}
              </h3>
              <button
                onClick={() => {
                  setShowForm(false);
                  setEditingPrinter(null);
                }}
                className="text-gray-400 hover:text-gray-600"
              >
                ×
              </button>
            </div>

            <PrinterForm
              printer={editingPrinter}
              onSubmit={editingPrinter ? handleUpdatePrinter : handleCreatePrinter}
              onCancel={() => {
                setShowForm(false);
                setEditingPrinter(null);
              }}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default PrinterList;
