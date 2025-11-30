// Discord OAuth Configuration
export const DISCORD_CLIENT_ID = process.env.REACT_APP_DISCORD_CLIENT_ID || '1444180338124263559';
export const DISCORD_REDIRECT_URI = process.env.REACT_APP_DISCORD_REDIRECT_URI || 'http://localhost:3000/auth/discord/callback';
export const DISCORD_AUTH_URL = 'https://discord.com/api/oauth2/authorize';
export const DISCORD_SCOPES = 'identify email';

export const USE_DEMO_MODE = false;
