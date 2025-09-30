// src/components/PolicyForm.jsx
import { useState, useEffect } from 'react';

const PolicyForm = ({ policy, onSubmit, onCancel }) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    type: 'MonthlyLimit',
    monthlyLimit: 500,
    costPerPage: 0.05,
    appliesToUsers: [],
    appliesToDepartments: [],
    isActive: true
  });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (policy) {
      setFormData({
        name: policy.name || '',
        description: policy.description || '',
        type: policy.type || 'MonthlyLimit',
        monthlyLimit: policy.monthlyLimit || 500,
        costPerPage: policy.costPerPage || 0.05,
        appliesToUsers: policy.appliesToUsers || [],
        appliesToDepartments: policy.appliesToDepartments || [],
        isActive: policy.isActive !== false
      });
    }
  }, [policy]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;

    if (name === 'appliesToUsers' || name === 'appliesToDepartments') {
      setFormData(prev => ({
        ...prev,
        [name]: checked
          ? [...prev[name], value]
          : prev[name].filter(item => item !== value)
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: type === 'checkbox' ? checked : type === 'number' ? parseFloat(value) || 0 : value
      }));
    }

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

    if (!formData.description.trim()) {
      newErrors.description = 'La descripción es requerida';
    }

    if (formData.type === 'MonthlyLimit' && (!formData.monthlyLimit || formData.monthlyLimit <= 0)) {
      newErrors.monthlyLimit = 'El límite mensual debe ser mayor a 0';
    }

    if (formData.costPerPage < 0) {
      newErrors.costPerPage = 'El costo no puede ser negativo';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
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

  const policyTypes = [
    { value: 'MonthlyLimit', label: 'Límite Mensual' },
    { value: 'Unlimited', label: 'Sin Límite' },
    { value: 'CostBased', label: 'Basado en Costo' }
  ];

  // Datos simulados para usuarios y departamentos
  const availableUsers = [
    { id: 'user1', name: 'Juan Pérez' },
    { id: 'user2', name: 'María González' },
    { id: 'executive1', name: 'Carlos López' }
  ];

  const availableDepartments = [
    'IT', 'HR', 'Executive', 'Marketing', 'Sales'
  ];

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
          placeholder="Ej: Política Básica Empleados"
        />
        {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name}</p>}
      </div>

      {/* Descripción */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700">
          Descripción *
        </label>
        <textarea
          id="description"
          name="description"
          value={formData.description}
          onChange={handleChange}
          rows={3}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.description ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Describe los detalles de esta política..."
        />
        {errors.description && <p className="mt-1 text-sm text-red-600">{errors.description}</p>}
      </div>

      {/* Tipo de política */}
      <div>
        <label htmlFor="type" className="block text-sm font-medium text-gray-700">
          Tipo de Política *
        </label>
        <select
          id="type"
          name="type"
          value={formData.type}
          onChange={handleChange}
          className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
        >
          {policyTypes.map((type) => (
            <option key={type.value} value={type.value}>
              {type.label}
            </option>
          ))}
        </select>
      </div>

      {/* Límite mensual (solo para MonthlyLimit) */}
      {formData.type === 'MonthlyLimit' && (
        <div>
          <label htmlFor="monthlyLimit" className="block text-sm font-medium text-gray-700">
            Límite Mensual (páginas) *
          </label>
          <input
            type="number"
            id="monthlyLimit"
            name="monthlyLimit"
            value={formData.monthlyLimit}
            onChange={handleChange}
            min="1"
            className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
              errors.monthlyLimit ? 'border-red-300' : 'border-gray-300'
            }`}
          />
          {errors.monthlyLimit && <p className="mt-1 text-sm text-red-600">{errors.monthlyLimit}</p>}
        </div>
      )}

      {/* Costo por página */}
      <div>
        <label htmlFor="costPerPage" className="block text-sm font-medium text-gray-700">
          Costo por Página ($)
        </label>
        <input
          type="number"
          id="costPerPage"
          name="costPerPage"
          value={formData.costPerPage}
          onChange={handleChange}
          min="0"
          step="0.01"
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.costPerPage ? 'border-red-300' : 'border-gray-300'
          }`}
        />
        {errors.costPerPage && <p className="mt-1 text-sm text-red-600">{errors.costPerPage}</p>}
      </div>

      {/* Usuarios aplicables */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Usuarios Aplicables
        </label>
        <div className="space-y-2 max-h-32 overflow-y-auto">
          {availableUsers.map((user) => (
            <label key={user.id} className="flex items-center">
              <input
                type="checkbox"
                name="appliesToUsers"
                value={user.id}
                checked={formData.appliesToUsers.includes(user.id)}
                onChange={handleChange}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <span className="ml-2 text-sm text-gray-700">{user.name}</span>
            </label>
          ))}
        </div>
      </div>

      {/* Departamentos aplicables */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Departamentos Aplicables
        </label>
        <div className="space-y-2 max-h-32 overflow-y-auto">
          {availableDepartments.map((dept) => (
            <label key={dept} className="flex items-center">
              <input
                type="checkbox"
                name="appliesToDepartments"
                value={dept}
                checked={formData.appliesToDepartments.includes(dept)}
                onChange={handleChange}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <span className="ml-2 text-sm text-gray-700">{dept}</span>
            </label>
          ))}
        </div>
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
          Política activa
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
          {loading ? 'Guardando...' : (policy ? 'Actualizar' : 'Crear')}
        </button>
      </div>
    </form>
  );
};

export default PolicyForm;
