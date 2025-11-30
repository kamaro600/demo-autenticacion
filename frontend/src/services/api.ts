import axios, { AxiosResponse } from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const response = await authService.refreshToken(refreshToken);
          if (response.data.accessToken && response.data.refreshToken) {
            localStorage.setItem('accessToken', response.data.accessToken);
            localStorage.setItem('refreshToken', response.data.refreshToken);
            
            // Retry original request
            originalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;
            return api(originalRequest);
          } else {
            throw new Error('Invalid refresh response');
          }
        } catch (refreshError) {
          // Refresh failed, redirect to login
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          window.location.href = '/login';
        }
      } else {
        // No refresh token, redirect to login
        window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);

// Auth types
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  hasMfaEnabled: boolean;
}

export interface AuthResponse {
  success: boolean;
  accessToken?: string;
  refreshToken?: string;
  accessTokenExpiry?: string;
  user?: User;
  message?: string;
  requiresMfa?: boolean;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  mfaCode?: string;
}

export interface MfaLoginRequest {
  userId: string;
  mfaCode: string;
}

export interface MfaSetupResponse {
  success: boolean;
  qrCodeBase64?: string;
  manualEntryKey?: string;
  message?: string;
}

export interface MfaStatusResponse {
  isEnabled: boolean;
  enabledAt?: string;
}

// Auth service
export const authService = {
  register: (data: RegisterRequest): Promise<AxiosResponse<AuthResponse>> =>
    api.post('/auth/register', data),

  login: (data: LoginRequest): Promise<AxiosResponse<AuthResponse>> =>
    api.post('/auth/login', data),

  verifyMfaLogin: (data: MfaLoginRequest): Promise<AxiosResponse<AuthResponse>> =>
    api.post('/auth/mfa/verify-login', data),

  refreshToken: (refreshToken: string): Promise<AxiosResponse<AuthResponse>> =>
    api.post('/auth/refresh-token', { refreshToken }),

  logout: (refreshToken?: string): Promise<AxiosResponse<any>> =>
    api.post('/auth/logout', { refreshToken }),

  googleLogin: (accessToken: string): Promise<AxiosResponse<AuthResponse>> =>
    api.post('/auth/external/google', { provider: 'Google', accessToken }),

  githubLogin: (accessToken: string): Promise<AxiosResponse<AuthResponse>> =>
    api.post('/auth/external/github', { provider: 'GitHub', accessToken }),

  discordLogin: (accessToken: string): Promise<AxiosResponse<AuthResponse>> =>
    api.post('/auth/external/discord', { provider: 'Discord', accessToken }),
};

// MFA service
export const mfaService = {
  setupMfa: (): Promise<AxiosResponse<MfaSetupResponse>> =>
    api.post('/mfa/setup'),

  enableMfa: (code: string): Promise<AxiosResponse<{ success: boolean; message?: string }>> =>
    api.post('/mfa/enable', { code }),

  verifyMfa: (code: string): Promise<AxiosResponse<{ success: boolean; message?: string }>> =>
    api.post('/mfa/verify', { code }),

  disableMfa: (): Promise<AxiosResponse<{ message: string }>> =>
    api.delete('/mfa/disable'),

  getMfaStatus: (): Promise<AxiosResponse<MfaStatusResponse>> =>
    api.get('/mfa/status'),
};

export default api;