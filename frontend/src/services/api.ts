import axios from 'axios';
import { useAuthStore } from '../store/authStore';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5050/api',
  withCredentials: true
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let refreshing: Promise<string> | null = null;

api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const original = error.config;
    if (error.response?.status === 401 && !original._retry && !original.url?.includes('/auth/refresh')) {
      original._retry = true;
      const { setAuth, clearAuth } = useAuthStore.getState();
      try {
        if (!refreshing) {
          refreshing = axios
            .post(`${api.defaults.baseURL}/auth/refresh`, {}, { withCredentials: true })
            .then((res) => {
              setAuth(res.data.user, res.data.accessToken);
              return res.data.accessToken as string;
            })
            .finally(() => {
              refreshing = null;
            });
        }
        const newToken = await refreshing;
        original.headers.Authorization = `Bearer ${newToken}`;
        return api(original);
      } catch {
        clearAuth();
        return Promise.reject(error);
      }
    }
    return Promise.reject(error);
  }
);
