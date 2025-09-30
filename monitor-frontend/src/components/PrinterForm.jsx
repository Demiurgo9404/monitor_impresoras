// src/components/PrinterForm.jsx
import { useState, useEffect } from 'react';

const PrinterForm = ({ printer, onSubmit, onCancel }) => {
  const [formData, setFormData] = useState({
    name: '',
    model: '',
    location: '',
    ipAddress: '',
    isActive: true
  });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (printer) {
      setFormData({
        name: printer.name || '',
        model: printer.model || '',
        location: printer.location || '',
        ipAddress: printer.ipAddress || '',
        isActive: printer.isActive !== false
      });
    }
  }, [printer]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));

    // Limpiar error del campo cuando el usuario empiece a escribir
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = 'El nombre es requerido';
    }

    if (!formData.model.trim()) {
      newErrors.model = 'El modelo es requerido';
    }

    if (!formData.location.trim()) {
      newErrors.location = 'La ubicación es requerida';
    }

    if (formData.ipAddress && !isValidIP(formData.ipAddress)) {
      newErrors.ipAddress = 'Formato de IP inválido';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const isValidIP = (ip) => {
    const ipRegex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
    return ipRegex.test(ip);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Error submitting form:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Nombre */}
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700">
          Nombre *
        </label>
        <input
          type="text"
          id="name"
          name="name"
          value={formData.name}
          onChange={handleChange}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.name ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Ej: Impresora Oficina 1"
        />
        {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name}</p>}
      </div>

      {/* Modelo */}
      <div>
        <label htmlFor="model" className="block text-sm font-medium text-gray-700">
          Modelo *
        </label>
        <input
          type="text"
          id="model"
          name="model"
          value={formData.model}
          onChange={handleChange}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.model ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Ej: HP LaserJet Pro M404dn"
        />
        {errors.model && <p className="mt-1 text-sm text-red-600">{errors.model}</p>}
      </div>

      {/* Ubicación */}
      <div>
        <label htmlFor="location" className="block text-sm font-medium text-gray-700">
          Ubicación *
        </label>
        <input
          type="text"
          id="location"
          name="location"
          value={formData.location}
          onChange={handleChange}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.location ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Ej: Oficina Principal - Piso 2"
        />
        {errors.location && <p className="mt-1 text-sm text-red-600">{errors.location}</p>}
      </div>

      {/* Dirección IP */}
      <div>
        <label htmlFor="ipAddress" className="block text-sm font-medium text-gray-700">
          Dirección IP
        </label>
        <input
          type="text"
          id="ipAddress"
          name="ipAddress"
          value={formData.ipAddress}
          onChange={handleChange}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.ipAddress ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Ej: 192.168.1.100"
        />
        {errors.ipAddress && <p className="mt-1 text-sm text-red-600">{errors.ipAddress}</p>}
      </div>

      {/* Estado activo */}
      <div className="flex items-center">
        <input
          type="checkbox"
          id="isActive"
          name="isActive"
          checked={formData.isActive}
          onChange={handleChange}
          className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
        />
        <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
          Impresora activa
        </label>
      </div>

      {/* Botones */}
      <div className="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          onClick={onCancel}
          className="bg-gray-300 text-gray-700 px-4 py-2 rounded-md text-sm font-medium hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-500"
        >
          Cancelar
        </button>
        <button
          type="submit"
          disabled={loading}
          className="bg-indigo-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50"
        >
          {loading ? 'Guardando...' : (printer ? 'Actualizar' : 'Crear')}
        </button>
      </div>
    </form>
  );
};

export default PrinterForm;
