// src/components/PolicyCard.jsx
const PolicyCard = ({ policy, onEdit, onDelete }) => {
  const getTypeColor = (type) => {
    switch (type) {
      case 'MonthlyLimit':
        return 'bg-blue-100 text-blue-800';
      case 'Unlimited':
        return 'bg-green-100 text-green-800';
      case 'CostBased':
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const formatMonthlyLimit = (limit) => {
    return limit ? `${limit} páginas/mes` : 'Sin límite';
  };

  const formatCost = (cost) => {
    return cost ? `$${cost.toFixed(2)}/página` : 'Sin costo';
  };

  return (
    <div className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow duration-200">
      <div className="p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 bg-indigo-500 rounded-full flex items-center justify-center">
              <span className="text-white font-semibold text-sm">
                {policy.name.charAt(0)}
              </span>
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900">{policy.name}</h3>
              <p className="text-sm text-gray-600">{policy.description}</p>
            </div>
          </div>
          <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getTypeColor(policy.type)}`}>
            {policy.type === 'MonthlyLimit' ? 'Límite Mensual' :
             policy.type === 'Unlimited' ? 'Sin Límite' :
             policy.type === 'CostBased' ? 'Basado en Costo' : policy.type}
          </span>
        </div>

        {/* Información de la política */}
        <div className="space-y-3 mb-4">
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Límite Mensual:</span>
            <span className="font-medium">{formatMonthlyLimit(policy.monthlyLimit)}</span>
          </div>

          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Costo por Página:</span>
            <span className="font-medium">{formatCost(policy.costPerPage)}</span>
          </div>

          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Usuarios Aplicables:</span>
            <span className="font-medium">
              {policy.appliesToUsers?.length || 0} usuario{(policy.appliesToUsers?.length || 0) !== 1 ? 's' : ''}
            </span>
          </div>

          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Departamentos:</span>
            <span className="font-medium">
              {policy.appliesToDepartments?.length || 0} departamento{(policy.appliesToDepartments?.length || 0) !== 1 ? 's' : ''}
            </span>
          </div>

          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Estado:</span>
            <span className={`font-medium ${policy.isActive ? 'text-green-600' : 'text-red-600'}`}>
              {policy.isActive ? 'Activa' : 'Inactiva'}
            </span>
          </div>

          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Última Modificación:</span>
            <span className="font-medium">{new Date(policy.updatedAt).toLocaleDateString()}</span>
          </div>
        </div>

        {/* Aplicación de la política */}
        {policy.appliesToUsers?.length > 0 && (
          <div className="mb-4">
            <p className="text-sm font-medium text-gray-700 mb-2">Usuarios:</p>
            <div className="flex flex-wrap gap-1">
              {policy.appliesToUsers.slice(0, 3).map((userId, index) => (
                <span key={index} className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                  Usuario {userId}
                </span>
              ))}
              {policy.appliesToUsers.length > 3 && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                  +{policy.appliesToUsers.length - 3} más
                </span>
              )}
            </div>
          </div>
        )}

        {policy.appliesToDepartments?.length > 0 && (
          <div className="mb-4">
            <p className="text-sm font-medium text-gray-700 mb-2">Departamentos:</p>
            <div className="flex flex-wrap gap-1">
              {policy.appliesToDepartments.map((dept, index) => (
                <span key={index} className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                  {dept}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Botones de acción */}
        <div className="flex space-x-2">
          <button
            onClick={() => onEdit(policy)}
            className="flex-1 bg-gray-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-500"
          >
            Editar
          </button>
          <button
            onClick={() => onDelete(policy.id)}
            className="bg-red-600 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
          >
            Eliminar
          </button>
        </div>
      </div>
    </div>
  );
};

export default PolicyCard;
