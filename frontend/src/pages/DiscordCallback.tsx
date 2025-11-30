import React, { useEffect } from 'react';

const DiscordCallback: React.FC = () => {
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const error = urlParams.get('error');

    if (code && window.opener) {
      window.opener.postMessage({ type: 'discord-oauth-success', code }, window.location.origin);
    } else if (error && window.opener) {
      window.opener.postMessage({ type: 'discord-oauth-error', error }, window.location.origin);
    }
  }, []);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
        <p className="mt-4 text-gray-600">Procesando autenticación con Discord...</p>
      </div>
    </div>
  );
};

export default DiscordCallback;
