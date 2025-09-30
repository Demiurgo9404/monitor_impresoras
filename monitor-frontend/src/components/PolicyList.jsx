// src/components/PolicyList.jsx
import { useState, useEffect } from 'react';
import PolicyCard from './PolicyCard';
import PolicyForm from './PolicyForm';

const PolicyList = () => {
  const [policies, setPolicies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingPolicy, setEditingPolicy] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState('all');

  useEffect(() => {
    loadPolicies();
  }, []);

  const loadPolicies = async () => {
    try {
      setLoading(true);
      // Por ahora usaremos datos simulados hasta que tengamos el endpoint real
      const mockPolicies = [
        {
          id: 1,
          name: 'Política Básica Empleados',
          description: 'Límite mensual de 500 páginas para empleados',
          type: 'MonthlyLimit',
          monthlyLimit: 500,
          costPerPage: 0.05,
          isActive: true,
          appliesToUsers: ['user1', 'user2'],
          appliesToDepartments: ['IT', 'HR'],
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString()
        },
        {
          id: 2,
          name: 'Política Ejecutiva',
          description: 'Sin límites para ejecutivos',
          type: 'Unlimited',
          monthlyLimit: null,
          costPerPage: 0.0,
          isActive: true,
          appliesToUsers: ['executive1'],
          appliesToDepartments: ['Executive'],
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString()
        },
        {
          id: 3,
          name: 'Política Departamento IT',
          description: 'Límite alto para departamento IT',
          type: 'MonthlyLimit',
          monthlyLimit: 2000,
          costPerPage: 0.03,
          isActive: true,
          appliesToUsers: [],
          appliesToDepartments: ['IT'],
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString()
        }
      ];
      setPolicies(mockPolicies);
    } catch (error) {
      setError('Error al cargar políticas');
    } finally {
      setLoading(false);
    }
  };

  const handleCreatePolicy = async (policyData) => {
    try {
      // Aquí iría la llamada real a la API
      const newPolicy = {
        id: Date.now(),
        ...policyData,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };
      setPolicies(prev => [...prev, newPolicy]);
      setShowForm(false);
    } catch (error) {
      setError('Error al crear política');
    }
  };

  const handleUpdatePolicy = async (policyData) => {
    try {
      // Aquí iría la llamada real a la API
      setPolicies(prev => prev.map(policy =>
        policy.id === editingPolicy.id
          ? { ...policy, ...policyData, updatedAt: new Date().toISOString() }
          : policy
      ));
      setEditingPolicy(null);
    } catch (error) {
      setError('Error al actualizar política');
    }
  };

  const handleDeletePolicy = async (id) => {
    if (window.confirm('¿Estás seguro de que deseas eliminar esta política?')) {
      try {
        // Aquí iría la llamada real a la API
        setPolicies(prev => prev.filter(policy => policy.id !== id));
      } catch (error) {
        setError('Error al eliminar política');
      }
    }
  };

  const handleEditPolicy = (policy) => {
    setEditingPolicy(policy);
  };

  const filteredPolicies = policies.filter(policy => {
    const matchesSearch = policy.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         policy.description.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesFilter = filterType === 'all' || policy.type === filterType;

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
          <h2 className="text-2xl font-bold text-gray-900">Políticas de Impresión</h2>
          <p className="mt-1 text-sm text-gray-600">
            Gestiona límites, costos y reglas de impresión
          </p>
        </div>
        <button
          onClick={() => setShowForm(true)}
          className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
        >
          + Nueva Política
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
              placeholder="Nombre o descripción..."
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            />
          </div>
          <div>
            <label htmlFor="type-filter" className="block text-sm font-medium text-gray-700">
              Tipo
            </label>
            <select
              id="type-filter"
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            >
              <option value="all">Todos los tipos</option>
              <option value="MonthlyLimit">Límite Mensual</option>
              <option value="Unlimited">Sin Límite</option>
              <option value="CostBased">Basado en Costo</option>
            </select>
          </div>
          <div className="flex items-end">
            <button
              onClick={loadPolicies}
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

      {/* Lista de políticas */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredPolicies.map((policy) => (
          <PolicyCard
            key={policy.id}
            policy={policy}
            onEdit={handleEditPolicy}
            onDelete={handleDeletePolicy}
          />
        ))}
      </div>

      {filteredPolicies.length === 0 && !loading && (
        <div className="text-center py-12">
          <p className="text-gray-500">No se encontraron políticas</p>
        </div>
      )}

      {/* Modal para crear/editar política */}
      {(showForm || editingPolicy) && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-[600px] shadow-lg rounded-md bg-white">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-medium text-gray-900">
                {editingPolicy ? 'Editar Política' : 'Nueva Política'}
              </h3>
              <button
                onClick={() => {
                  setShowForm(false);
                  setEditingPolicy(null);
                }}
                className="text-gray-400 hover:text-gray-600"
              >
                ×
              </button>
            </div>

            <PolicyForm
              policy={editingPolicy}
              onSubmit={editingPolicy ? handleUpdatePolicy : handleCreatePolicy}
              onCancel={() => {
                setShowForm(false);
                setEditingPolicy(null);
              }}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default PolicyList;
