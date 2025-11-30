import React, { createContext, useContext, useEffect, useReducer, ReactNode } from 'react';
import { User, authService, AuthResponse } from '../services/api';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  requiresMfa: boolean;
  error: string | null;
  pendingCredentials: { email: string; password: string } | null;
  pendingUserId: string | null;
}

type AuthAction =
  | { type: 'LOGIN_START' }
  | { type: 'LOGIN_SUCCESS'; payload: { user: User; accessToken: string; refreshToken: string } }
  | { type: 'LOGIN_MFA_REQUIRED'; payload: { email: string; password: string; userId?: string } }
  | { type: 'LOGIN_FAILURE'; payload: string }
  | { type: 'LOGOUT' }
  | { type: 'SET_USER'; payload: User }
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'CLEAR_ERROR' };

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true,
  requiresMfa: false,
  error: null,
  pendingCredentials: null,
  pendingUserId: null,
};

const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'LOGIN_START':
      return { ...state, isLoading: true, error: null };
    case 'LOGIN_SUCCESS':
      return {
        ...state,
        user: action.payload.user,
        isAuthenticated: true,
        isLoading: false,
        requiresMfa: false,
        error: null,
        pendingCredentials: null,
        pendingUserId: null,
      };
    case 'LOGIN_MFA_REQUIRED':
      return {
        ...state,
        requiresMfa: true,
        isLoading: false,
        error: null,
        pendingCredentials: action.payload,
        pendingUserId: action.payload.userId || null,
      };
    case 'LOGIN_FAILURE':
      return {
        ...state,
        user: null,
        isAuthenticated: false,
        isLoading: false,
        requiresMfa: false,
        error: action.payload,
        pendingCredentials: null,
        pendingUserId: null,
      };
    case 'LOGOUT':
      return {
        ...state,
        user: null,
        isAuthenticated: false,
        isLoading: false,
        requiresMfa: false,
        error: null,
        pendingCredentials: null,
        pendingUserId: null,
      };
    case 'SET_USER':
      return { ...state, user: action.payload, isAuthenticated: true };
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    case 'CLEAR_ERROR':
      return { ...state, error: null };
    default:
      return state;
  }
};

interface AuthContextType extends AuthState {
  login: (email: string, password: string, mfaCode?: string) => Promise<void>;
  verifyMfaCode: (code: string) => Promise<void>;
  register: (firstName: string, lastName: string, email: string, password: string, confirmPassword: string) => Promise<void>;
  logout: () => Promise<void>;
  googleLogin: (accessToken: string) => Promise<void>;
  githubLogin: (accessToken: string) => Promise<void>;
  discordLogin: (accessToken: string) => Promise<void>;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  useEffect(() => {
    // Check if user is logged in on app start
    const checkAuthStatus = () => {
      const accessToken = localStorage.getItem('accessToken');
      const userStr = localStorage.getItem('user');
      
      if (accessToken && userStr) {
        try {
          const user = JSON.parse(userStr);
          dispatch({ type: 'SET_USER', payload: user });
        } catch (error) {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('user');
        }
      }
      
      dispatch({ type: 'SET_LOADING', payload: false });
    };

    checkAuthStatus();
  }, []);

  const handleAuthSuccess = (response: AuthResponse, email: string, password: string, userId?: string) => {
    // Check MFA requirement FIRST, before checking success
    if (response.requiresMfa === true) {
      dispatch({ type: 'LOGIN_MFA_REQUIRED', payload: { email, password, userId: userId || response.user?.id } });
    } else if (response.success && response.accessToken && response.refreshToken && response.user) {
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      dispatch({
        type: 'LOGIN_SUCCESS',
        payload: {
          user: response.user,
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
        },
      });
    } else {
      dispatch({ type: 'LOGIN_FAILURE', payload: response.message || 'Authentication failed' });
    }
  };

  const login = async (email: string, password: string, mfaCode?: string) => {
    dispatch({ type: 'LOGIN_START' });
    
    try {
      const response = await authService.login({ email, password, mfaCode });
      handleAuthSuccess(response.data, email, password);
    } catch (error: any) {
      dispatch({
        type: 'LOGIN_FAILURE',
        payload: error.response?.data?.message || 'Login failed',
      });
    }
  };

  const verifyMfaCode = async (code: string) => {
    dispatch({ type: 'LOGIN_START' });
    
    try {

      if (state.pendingUserId) {
        const response = await authService.verifyMfaLogin({ userId: state.pendingUserId, mfaCode: code });
        handleAuthSuccess(response.data, '', '');
      } 
      else if (state.pendingCredentials) {
        const { email, password } = state.pendingCredentials;
        const response = await authService.login({ email, password, mfaCode: code });
        handleAuthSuccess(response.data, email, password);
      } else {
        dispatch({ type: 'LOGIN_FAILURE', payload: 'Autenticacion pendientes no encontradas' });
      }
    } catch (error: any) {
      dispatch({
        type: 'LOGIN_FAILURE',
        payload: error.response?.data?.message || 'Verificacion MFA fallida',
      });
    }
  };

  const register = async (firstName: string, lastName: string, email: string, password: string, confirmPassword: string) => {
    dispatch({ type: 'LOGIN_START' });
    
    try {
      const response = await authService.register({ firstName, lastName, email, password, confirmPassword });
      handleAuthSuccess(response.data, email, password);
    } catch (error: any) {
      dispatch({
        type: 'LOGIN_FAILURE',
        payload: error.response?.data?.message || 'Registro fallido',
      });
    }
  };

  const logout = async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      await authService.logout(refreshToken || undefined);
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      dispatch({ type: 'LOGOUT' });
    }
  };

  const googleLogin = async (accessToken: string) => {
    dispatch({ type: 'LOGIN_START' });
    
    try {
      const response = await authService.googleLogin(accessToken);
      handleAuthSuccess(response.data, '', '');
    } catch (error: any) {
      dispatch({
        type: 'LOGIN_FAILURE',
        payload: error.response?.data?.message || 'Google login fallido',
      });
    }
  };

  const githubLogin = async (accessToken: string) => {
    dispatch({ type: 'LOGIN_START' });
    
    try {
      const response = await authService.githubLogin(accessToken);
      handleAuthSuccess(response.data, '', '');
    } catch (error: any) {
      dispatch({
        type: 'LOGIN_FAILURE',
        payload: error.response?.data?.message || 'GitHub login fallido',
      });
    }
  };

  const discordLogin = async (accessToken: string) => {
    dispatch({ type: 'LOGIN_START' });
    
    try {
      const response = await authService.discordLogin(accessToken);
      handleAuthSuccess(response.data, '', '');
    } catch (error: any) {
      dispatch({
        type: 'LOGIN_FAILURE',
        payload: error.response?.data?.message || 'Discord login fallido',
      });
    }
  };

  const clearError = () => {
    dispatch({ type: 'CLEAR_ERROR' });
  };

  const value: AuthContextType = {
    ...state,
    login,
    verifyMfaCode,
    register,
    logout,
    googleLogin,
    githubLogin,
    discordLogin,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth debe usarse dentro de un AuthProvider');
  }
  return context;
};