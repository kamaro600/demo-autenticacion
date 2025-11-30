import React, { useEffect } from 'react';

const GitHubCallback: React.FC = () => {
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const error = urlParams.get('error');

    if (code) {
      // Send the code to the parent window
      if (window.opener) {
        window.opener.postMessage(
          { type: 'github-oauth-success', code },
          window.location.origin
        );
      }
    } else if (error) {
      // Send the error to the parent window
      if (window.opener) {
        window.opener.postMessage(
          { type: 'github-oauth-error', error },
          window.location.origin
        );
      }
    }
  }, []);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mx-auto"></div>
        <p className="mt-4 text-gray-600">Procesando autenticación de GitHub...</p>
      </div>
    </div>
  );
};

export default GitHubCallback;
