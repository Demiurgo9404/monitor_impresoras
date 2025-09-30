// src/components/UserCard.jsx
const UserCard = ({ user, onEdit, onDelete }) => {
  const getRoleColor = (role) => {
    switch (role) {
      case 'Admin':
        return 'bg-red-100 text-red-800';
      case 'Manager':
        return 'bg-blue-100 text-blue-800';
      case 'User':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const formatLastLogin = (lastLogin) => {
    if (!lastLogin) return 'Nunca';

    const date = new Date(lastLogin);
    const now = new Date();
    const diffMs = now - date;
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffHours / 24);

    if (diffHours < 1) {
      return 'Hace menos de 1 hora';
    } else if (diffHours < 24) {
      return `Hace ${diffHours} hora${diffHours > 1 ? 's' : ''}`;
    } else if (diffDays < 7) {
      return `Hace ${diffDays} día${diffDays > 1 ? 's' : ''}`;
    } else {
      return date.toLocaleDateString();
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow duration-200">
      <div className="p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 bg-indigo-500 rounded-full flex items-center justify-center">
              <span className="text-white font-semibold text-sm">
                {user.firstName.charAt(0)}{user.lastName.charAt(0)}
              </span>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900">
                {user.firstName} {user.lastName}
              </h3>
              <p className="text-sm text-gray-600">{user.email}</p>
            </div>
          </div>
          <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
            user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
          }`}>
            {user.isActive ? 'Activo' : 'Inactivo'}
          </span>
        </div>

        {/* Información adicional */}
        <div className="space-y-2 mb-4">
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Roles:</span>
            <div className="flex flex-wrap gap-1">
              {user.roles.map((role, index) => (
                <span key={index} className={`px-2 py-1 text-xs font-semibold rounded-full ${getRoleColor(role)}`}>
                  {role}
                </span>
              ))}
            </div>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Último acceso:</span>
            <span className="font-medium">{formatLastLogin(user.lastLogin)}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Creado:</span>
            <span className="font-medium">{new Date(user.createdAt).toLocaleDateString()}</span>
          </div>
        </div>

        {/* Botones de acción */}
        <div className="flex space-x-2">
          <button
            onClick={() => onEdit(user)}
            className="flex-1 bg-gray-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-500"
          >
            Editar
          </button>
          <button
            onClick={() => onDelete(user.id)}
            className="bg-red-600 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
          >
            Eliminar
          </button>
        </div>
      </div>
    </div>
  );
};

export default UserCard;
