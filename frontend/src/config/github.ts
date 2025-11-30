// GitHub OAuth Configuration
export const GITHUB_CLIENT_ID = process.env.REACT_APP_GITHUB_CLIENT_ID || 'Ov23li9VjxaqiAoQjmtK';
export const GITHUB_REDIRECT_URI = process.env.REACT_APP_GITHUB_REDIRECT_URI || 'http://localhost:3000/auth/github/callback';
export const USE_DEMO_MODE = false; 

// GitHub OAuth URLs
export const GITHUB_AUTH_URL = 'https://github.com/login/oauth/authorize';
export const GITHUB_SCOPES = 'read:user user:email';
