import { create } from 'zustand';
import type { User } from '../types';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  setAuth: (user: User, accessToken: string) => void;
  clearAuth: () => void;
}

const USER_KEY = 'am_user';

export const useAuthStore = create<AuthState>((set) => ({
  user: JSON.parse(localStorage.getItem(USER_KEY) || 'null'),
  accessToken: null,
  setAuth: (user, accessToken) => {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    set({ user, accessToken });
  },
  clearAuth: () => {
    localStorage.removeItem(USER_KEY);
    set({ user: null, accessToken: null });
  }
}));
