import React from 'react';
import { GITHUB_CLIENT_ID, GITHUB_AUTH_URL, GITHUB_SCOPES, GITHUB_REDIRECT_URI } from '../config/github';

interface GitHubLoginButtonProps {
  onSuccess?: (code: string) => void;
  onFailure?: (error: string) => void;
  className?: string;
  children?: React.ReactNode;
}

const GitHubLoginButton: React.FC<GitHubLoginButtonProps> = ({
  onSuccess,
  onFailure,
  className = '',
  children = (
    <>
      <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
        <path fillRule="evenodd" d="M10 0C4.477 0 0 4.484 0 10.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0110 4.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.203 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.942.359.31.678.921.678 1.856 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0020 10.017C20 4.484 15.522 0 10 0z" clipRule="evenodd" />
      </svg>
      Continue with GitHub
    </>
  ),
}) => {
  const handleClick = () => {
    const params = new URLSearchParams({
      client_id: GITHUB_CLIENT_ID,
      redirect_uri: GITHUB_REDIRECT_URI,
      scope: GITHUB_SCOPES,
      state: Math.random().toString(36).substring(7), // Random state for CSRF protection
    });

    const authUrl = `${GITHUB_AUTH_URL}?${params.toString()}`;
    
    // Open GitHub OAuth in a popup
    const width = 600;
    const height = 700;
    const left = window.screen.width / 2 - width / 2;
    const top = window.screen.height / 2 - height / 2;
    
    const popup = window.open(
      authUrl,
      'GitHub Login',
      `width=${width},height=${height},left=${left},top=${top}`
    );

    // Listen for the OAuth callback
    const handleMessage = (event: MessageEvent) => {
      if (event.origin !== window.location.origin) {
        return;
      }

      if (event.data.type === 'github-oauth-success' && event.data.code) {
        onSuccess?.(event.data.code);
        popup?.close();
        window.removeEventListener('message', handleMessage);
      } else if (event.data.type === 'github-oauth-error') {
        onFailure?.(event.data.error || 'GitHub authentication failed');
        popup?.close();
        window.removeEventListener('message', handleMessage);
      }
    };

    window.addEventListener('message', handleMessage);

    // Check if popup was closed
    const checkClosed = setInterval(() => {
      if (popup?.closed) {
        clearInterval(checkClosed);
        window.removeEventListener('message', handleMessage);
      }
    }, 1000);
  };

  return (
    <button
      type="button"
      onClick={handleClick}
      className={className || 'w-full flex items-center justify-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500'}
    >
      {children}
    </button>
  );
};

export default GitHubLoginButton;
