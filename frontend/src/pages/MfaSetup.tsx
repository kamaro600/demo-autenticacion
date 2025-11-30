import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { mfaService, MfaSetupResponse } from '../services/api';

const MfaSetup: React.FC = () => {
  const [setupResponse, setSetupResponse] = useState<MfaSetupResponse | null>(null);
  const [verificationCode, setVerificationCode] = useState('');
  const [loading, setLoading] = useState(true);
  const [enabling, setEnabling] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const setupMfa = async () => {
      try {
        const response = await mfaService.setupMfa();
        setSetupResponse(response.data);
      } catch (error: any) {
        setError(error.response?.data?.message || 'Fallo al configurar MFA');
      } finally {
        setLoading(false);
      }
    };

    setupMfa();
  }, []);

  const handleEnableMfa = async (e: React.FormEvent) => {
    e.preventDefault();
    setEnabling(true);
    setError(null);

    try {
      const response = await mfaService.enableMfa(verificationCode);
      if (response.data.success) {
        navigate('/dashboard');
      } else {
        setError(response.data.message || 'Fallo al habilitar MFA');
      }
    } catch (error: any) {
      setError(error.response?.data?.message || 'Fallo al habilitar MFA');
    } finally {
      setEnabling(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg">Configurando MFA...</div>
      </div>
    );
  }

  if (!setupResponse?.success) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="text-red-600 text-lg mb-4">Fallo al configurar MFA</div>
          <p className="text-gray-600">{setupResponse?.message || error}</p>
          <button
            onClick={() => navigate('/dashboard')}
            className="mt-4 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-md"
          >
            Volver al Panel
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-2xl mx-auto">
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Configurar Autenticación Multifactor</h2>
          
          <div className="space-y-6">
            <div>
              <h3 className="text-lg font-medium text-gray-900 mb-3">Paso 1: Escanear Código QR</h3>
              <p className="text-sm text-gray-600 mb-4">
                Escanee este código QR con su aplicación autenticadora (Google Authenticator, Authy, etc.)
              </p>
              
              {setupResponse.qrCodeBase64 && (
                <div className="flex justify-center mb-4">
                  <img 
                    src={`data:image/png;base64,${setupResponse.qrCodeBase64}`}
                    alt="MFA QR Code"
                    className="border rounded-lg"
                  />
                </div>
              )}
              
              <div className="bg-gray-100 p-4 rounded-lg">
                <p className="text-sm font-medium text-gray-700 mb-2">Clave de Entrada Manual:</p>
                <code className="text-sm bg-white px-2 py-1 rounded border">
                  {setupResponse.manualEntryKey}
                </code>
                <p className="text-xs text-gray-500 mt-2">
                  Use this key if you can't scan the QR code
                </p>
              </div>
            </div>

            <div>
              <h3 className="text-lg font-medium text-gray-900 mb-3">Paso 2: Ingresar Código de Verificación</h3>
              <p className="text-sm text-gray-600 mb-4">
                Ingrese el código de 6 dígitos de su aplicación autenticadora para completar la configuración
              </p>
              
              <form onSubmit={handleEnableMfa} className="space-y-4">
                {error && (
                  <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
                    {error}
                  </div>
                )}
                
                <div>
                  <input
                    type="text"
                    placeholder="Ingrese el código de 6 dígitos"
                    value={verificationCode}
                    onChange={(e) => setVerificationCode(e.target.value)}
                    maxLength={6}
                    className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                    required
                  />
                </div>
                
                <div className="flex space-x-3">
                  <button
                    type="submit"
                    disabled={enabling || verificationCode.length !== 6}
                    className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-md text-sm font-medium disabled:opacity-50"
                  >
                    {enabling ? 'Habilitando...' : 'Habilitar MFA'}
                  </button>
                  
                  <button
                    type="button"
                    onClick={() => navigate('/dashboard')}
                    className="flex-1 bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-md text-sm font-medium"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MfaSetup;