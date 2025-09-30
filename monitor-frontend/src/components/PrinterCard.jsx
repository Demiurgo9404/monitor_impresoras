// src/components/PrinterCard.jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const PrinterCard = ({ printer, onEdit, onDelete }) => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

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

  const handleViewDetails = () => {
    navigate(`/printers/${printer.id}`);
  };

  return (
    <div className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow duration-200">
      <div className="p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center space-x-3">
            <div className={`w-3 h-3 rounded-full ${printer.isOnline ? 'bg-green-500' : 'bg-red-500'}`}></div>
            <h3 className="text-lg font-semibold text-gray-900">{printer.name}</h3>
          </div>
          <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(printer.status)}`}>
            {printer.status || 'Desconocido'}
          </span>
        </div>

        {/* Información básica */}
        <div className="space-y-2 mb-4">
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Modelo:</span>
            <span className="font-medium">{printer.model}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Ubicación:</span>
            <span className="font-medium">{printer.location}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Estado:</span>
            <span className={`font-medium ${printer.isOnline ? 'text-green-600' : 'text-red-600'}`}>
              {printer.isOnline ? 'En línea' : 'Fuera de línea'}
            </span>
          </div>
        </div>

        {/* Métricas rápidas */}
        {printer.tonerLevel !== undefined && (
          <div className="mb-4">
            <div className="flex justify-between text-sm mb-1">
              <span className="text-gray-600">Tóner:</span>
              <span className={`font-medium ${printer.tonerLevel < 20 ? 'text-red-600' : 'text-green-600'}`}>
                {printer.tonerLevel}%
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className={`h-2 rounded-full ${printer.tonerLevel < 20 ? 'bg-red-500' : 'bg-green-500'}`}
                style={{ width: `${printer.tonerLevel}%` }}
              ></div>
            </div>
          </div>
        )}

        {printer.paperLevel !== undefined && (
          <div className="mb-4">
            <div className="flex justify-between text-sm mb-1">
              <span className="text-gray-600">Papel:</span>
              <span className={`font-medium ${printer.paperLevel < 10 ? 'text-red-600' : 'text-green-600'}`}>
                {printer.paperLevel}%
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className={`h-2 rounded-full ${printer.paperLevel < 10 ? 'bg-red-500' : 'bg-green-500'}`}
                style={{ width: `${printer.paperLevel}%` }}
              ></div>
            </div>
          </div>
        )}

        {/* Última actualización */}
        {printer.lastUpdate && (
          <div className="text-xs text-gray-500 mb-4">
            Última actualización: {new Date(printer.lastUpdate).toLocaleString()}
          </div>
        )}

        {/* Botones de acción */}
        <div className="flex space-x-2">
          <button
            onClick={handleViewDetails}
            className="flex-1 bg-indigo-600 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
          >
            Ver Detalles
          </button>
          <button
            onClick={() => onEdit(printer)}
            className="bg-gray-500 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-gray-500"
          >
            Editar
          </button>
          <button
            onClick={() => onDelete(printer.id)}
            className="bg-red-600 text-white px-3 py-2 rounded-md text-sm font-medium hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
          >
            Eliminar
          </button>
        </div>
      </div>
    </div>
  );
};

export default PrinterCard;
