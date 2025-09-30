// src/components/UserForm.jsx
import { useState, useEffect } from 'react';

const UserForm = ({ user, onSubmit, onCancel }) => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    roles: [],
    isActive: true
  });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        email: user.email || '',
        password: '',
        confirmPassword: '',
        roles: user.roles || [],
        isActive: user.isActive !== false
      });
    }
  }, [user]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;

    if (name === 'roles') {
      setFormData(prev => ({
        ...prev,
        roles: checked
          ? [...prev.roles, value]
          : prev.roles.filter(role => role !== value)
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: type === 'checkbox' ? checked : value
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

    if (!formData.firstName.trim()) {
      newErrors.firstName = 'El nombre es requerido';
    }

    if (!formData.lastName.trim()) {
      newErrors.lastName = 'El apellido es requerido';
    }

    if (!formData.email.trim()) {
      newErrors.email = 'El email es requerido';
    } else if (!isValidEmail(formData.email)) {
      newErrors.email = 'Formato de email inválido';
    }

    if (!user) { // Solo validar contraseña para nuevos usuarios
      if (!formData.password) {
        newErrors.password = 'La contraseña es requerida';
      } else if (formData.password.length < 6) {
        newErrors.password = 'La contraseña debe tener al menos 6 caracteres';
      }

      if (formData.password !== formData.confirmPassword) {
        newErrors.confirmPassword = 'Las contraseñas no coinciden';
      }
    }

    if (formData.roles.length === 0) {
      newErrors.roles = 'Debe seleccionar al menos un rol';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const isValidEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      const submitData = { ...formData };
      if (user) {
        delete submitData.password; // No enviar contraseña en edición
        delete submitData.confirmPassword;
      }

      await onSubmit(submitData);
    } catch (error) {
      console.error('Error submitting form:', error);
    } finally {
      setLoading(false);
    }
  };

  const availableRoles = [
    { value: 'Admin', label: 'Administrador' },
    { value: 'Manager', label: 'Manager' },
    { value: 'User', label: 'Usuario' }
  ];

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* Nombre */}
      <div>
        <label htmlFor="firstName" className="block text-sm font-medium text-gray-700">
          Nombre *
        </label>
        <input
          type="text"
          id="firstName"
          name="firstName"
          value={formData.firstName}
          onChange={handleChange}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.firstName ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Ej: Juan"
        />
        {errors.firstName && <p className="mt-1 text-sm text-red-600">{errors.firstName}</p>}
      </div>

      {/* Apellido */}
      <div>
        <label htmlFor="lastName" className="block text-sm font-medium text-gray-700">
          Apellido *
        </label>
        <input
          type="text"
          id="lastName"
          name="lastName"
          value={formData.lastName}
          onChange={handleChange}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.lastName ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Ej: Pérez"
        />
        {errors.lastName && <p className="mt-1 text-sm text-red-600">{errors.lastName}</p>}
      </div>

      {/* Email */}
      <div>
        <label htmlFor="email" className="block text-sm font-medium text-gray-700">
          Email *
        </label>
        <input
          type="email"
          id="email"
          name="email"
          value={formData.email}
          onChange={handleChange}
          className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
            errors.email ? 'border-red-300' : 'border-gray-300'
          }`}
          placeholder="Ej: juan.perez@empresa.com"
        />
        {errors.email && <p className="mt-1 text-sm text-red-600">{errors.email}</p>}
      </div>

      {/* Contraseña (solo para nuevos usuarios) */}
      {!user && (
        <>
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">
              Contraseña *
            </label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
                errors.password ? 'border-red-300' : 'border-gray-300'
              }`}
              placeholder="Mínimo 6 caracteres"
            />
            {errors.password && <p className="mt-1 text-sm text-red-600">{errors.password}</p>}
          </div>

          <div>
            <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">
              Confirmar Contraseña *
            </label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              className={`mt-1 block w-full border rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm ${
                errors.confirmPassword ? 'border-red-300' : 'border-gray-300'
              }`}
            />
            {errors.confirmPassword && <p className="mt-1 text-sm text-red-600">{errors.confirmPassword}</p>}
          </div>
        </>
      )}

      {/* Roles */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Roles *
        </label>
        <div className="space-y-2">
          {availableRoles.map((role) => (
            <label key={role.value} className="flex items-center">
              <input
                type="checkbox"
                name="roles"
                value={role.value}
                checked={formData.roles.includes(role.value)}
                onChange={handleChange}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <span className="ml-2 text-sm text-gray-700">{role.label}</span>
            </label>
          ))}
        </div>
        {errors.roles && <p className="mt-1 text-sm text-red-600">{errors.roles}</p>}
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
          Usuario activo
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
          {loading ? 'Guardando...' : (user ? 'Actualizar' : 'Crear')}
        </button>
      </div>
    </form>
  );
};

export default UserForm;
