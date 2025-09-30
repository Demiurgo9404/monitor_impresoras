// src/components/UserList.jsx
import { useState, useEffect } from 'react';
import UserCard from './UserCard';
import UserForm from './UserForm';

const UserList = () => {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterRole, setFilterRole] = useState('all');

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);
      // Por ahora usaremos datos simulados hasta que tengamos el endpoint real
      const mockUsers = [
        {
          id: 1,
          email: 'admin@empresa.com',
          firstName: 'Administrador',
          lastName: 'Sistema',
          isActive: true,
          roles: ['Admin'],
          lastLogin: new Date().toISOString(),
          createdAt: new Date().toISOString()
        },
        {
          id: 2,
          email: 'soporte@empresa.com',
          firstName: 'Técnico',
          lastName: 'Soporte',
          isActive: true,
          roles: ['Manager'],
          lastLogin: new Date(Date.now() - 86400000).toISOString(), // 1 día atrás
          createdAt: new Date().toISOString()
        },
        {
          id: 3,
          email: 'usuario@empresa.com',
          firstName: 'Usuario',
          lastName: 'Final',
          isActive: true,
          roles: ['User'],
          lastLogin: new Date(Date.now() - 172800000).toISOString(), // 2 días atrás
          createdAt: new Date().toISOString()
        }
      ];
      setUsers(mockUsers);
    } catch (error) {
      setError('Error al cargar usuarios');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateUser = async (userData) => {
    try {
      // Aquí iría la llamada real a la API
      const newUser = {
        id: Date.now(),
        ...userData,
        createdAt: new Date().toISOString(),
        lastLogin: null
      };
      setUsers(prev => [...prev, newUser]);
      setShowForm(false);
    } catch (error) {
      setError('Error al crear usuario');
    }
  };

  const handleUpdateUser = async (userData) => {
    try {
      // Aquí iría la llamada real a la API
      setUsers(prev => prev.map(user =>
        user.id === editingUser.id ? { ...user, ...userData } : user
      ));
      setEditingUser(null);
    } catch (error) {
      setError('Error al actualizar usuario');
    }
  };

  const handleDeleteUser = async (id) => {
    if (window.confirm('¿Estás seguro de que deseas eliminar este usuario?')) {
      try {
        // Aquí iría la llamada real a la API
        setUsers(prev => prev.filter(user => user.id !== id));
      } catch (error) {
        setError('Error al eliminar usuario');
      }
    }
  };

  const handleEditUser = (user) => {
    setEditingUser(user);
  };

  const filteredUsers = users.filter(user => {
    const fullName = `${user.firstName} ${user.lastName}`.toLowerCase();
    const matchesSearch = fullName.includes(searchTerm.toLowerCase()) ||
                         user.email.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesFilter = filterRole === 'all' ||
                         user.roles.includes(filterRole);

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
          <h2 className="text-2xl font-bold text-gray-900">Gestión de Usuarios</h2>
          <p className="mt-1 text-sm text-gray-600">
            Administra usuarios y asigna roles del sistema
          </p>
        </div>
        <button
          onClick={() => setShowForm(true)}
          className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
        >
          + Nuevo Usuario
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
              placeholder="Nombre, apellido o email..."
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            />
          </div>
          <div>
            <label htmlFor="role-filter" className="block text-sm font-medium text-gray-700">
              Rol
            </label>
            <select
              id="role-filter"
              value={filterRole}
              onChange={(e) => setFilterRole(e.target.value)}
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
            >
              <option value="all">Todos los roles</option>
              <option value="Admin">Administrador</option>
              <option value="Manager">Manager</option>
              <option value="User">Usuario</option>
            </select>
          </div>
          <div className="flex items-end">
            <button
              onClick={loadUsers}
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

      {/* Lista de usuarios */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredUsers.map((user) => (
          <UserCard
            key={user.id}
            user={user}
            onEdit={handleEditUser}
            onDelete={handleDeleteUser}
          />
        ))}
      </div>

      {filteredUsers.length === 0 && !loading && (
        <div className="text-center py-12">
          <p className="text-gray-500">No se encontraron usuarios</p>
        </div>
      )}

      {/* Modal para crear/editar usuario */}
      {(showForm || editingUser) && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-medium text-gray-900">
                {editingUser ? 'Editar Usuario' : 'Nuevo Usuario'}
              </h3>
              <button
                onClick={() => {
                  setShowForm(false);
                  setEditingUser(null);
                }}
                className="text-gray-400 hover:text-gray-600"
              >
                ×
              </button>
            </div>

            <UserForm
              user={editingUser}
              onSubmit={editingUser ? handleUpdateUser : handleCreateUser}
              onCancel={() => {
                setShowForm(false);
                setEditingUser(null);
              }}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default UserList;
