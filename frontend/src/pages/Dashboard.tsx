import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { mfaService, MfaStatusResponse } from '../services/api';

const Dashboard: React.FC = () => {
  const { user, logout } = useAuth();
  const [mfaStatus, setMfaStatus] = useState<MfaStatusResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchMfaStatus = async () => {
      try {
        const response = await mfaService.getMfaStatus();
        setMfaStatus(response.data);
      } catch (error) {
        console.error('Error al obtener el estado de MFA:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMfaStatus();
  }, []);

  const handleLogout = async () => {
    await logout();
  };

  const handleDisableMfa = async () => {
    if (window.confirm('Estas seguro de deshabilitar el MFA?')) {
      try {
        await mfaService.disableMfa();
        setMfaStatus({ isEnabled: false });
      } catch (error) {
        console.error('Error al deshabilitar MFA:', error);
      }
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg">Cargando...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-semibold">Demo Auth App</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-gray-700">Bienvenido, {user?.firstName}!</span>
              <button
                onClick={handleLogout}
                className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-md text-sm font-medium"
              >
                Cerrar sesión
              </button>
            </div>
          </div>
        </div>
      </nav>

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="border-4 border-dashed border-gray-200 rounded-lg p-8">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h2>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="bg-white p-6 rounded-lg shadow">
                <h3 className="text-lg font-medium text-gray-900 mb-4">Información del usuario</h3>
                <dl className="space-y-2">
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Nombre</dt>
                    <dd className="text-sm text-gray-900">{user?.firstName} {user?.lastName}</dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-gray-500">Correo electrónico</dt>
                    <dd className="text-sm text-gray-900">{user?.email}</dd>
                  </div>
                  <div>
                    <dt className="text-sm font-medium text-gray-500">ID de usuario</dt>
                    <dd className="text-sm text-gray-900">{user?.id}</dd>
                  </div>
                </dl>
              </div>

              <div className="bg-white p-6 rounded-lg shadow">
                <h3 className="text-lg font-medium text-gray-900 mb-4">Autenticación multifactor</h3>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-900">
                      Estado: {mfaStatus?.isEnabled ? 'Habilitado' : 'Deshabilitado'}
                    </span>
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                      mfaStatus?.isEnabled ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                    }`}>
                      {mfaStatus?.isEnabled ? 'Activo' : 'Inactivo'}
                    </span>
                  </div>
                  
                  {mfaStatus?.enabledAt && (
                    <div className="text-sm text-gray-500">
                      Habilitado el: {new Date(mfaStatus.enabledAt).toLocaleDateString()}
                    </div>
                  )}
                  
                  <div className="flex space-x-3">
                    {!mfaStatus?.isEnabled ? (
                      <Link
                        to="/mfa-setup"
                        className="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-md text-sm font-medium"
                      >
                        Configurar MFA
                      </Link>
                    ) : (
                      <button
                        onClick={handleDisableMfa}
                        className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-md text-sm font-medium"
                      >
                        Deshabilitar MFA
                      </button>
                    )}
                  </div>
                </div>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;