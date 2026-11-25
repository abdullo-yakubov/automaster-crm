import { create } from 'zustand';

type Theme = 'dark' | 'light';
type Lang = 'ru' | 'tj';

interface UiState {
  theme: Theme;
  lang: Lang;
  sidebarOpen: boolean;
  toggleTheme: () => void;
  setLang: (lang: Lang) => void;
  toggleSidebar: () => void;
  closeSidebar: () => void;
}

export const useUiStore = create<UiState>((set, get) => ({
  theme: (localStorage.getItem('am_theme') as Theme) || 'dark',
  lang: (localStorage.getItem('am_lang') as Lang) || 'ru',
  sidebarOpen: false,
  toggleTheme: () => {
    const next = get().theme === 'dark' ? 'light' : 'dark';
    localStorage.setItem('am_theme', next);
    set({ theme: next });
  },
  setLang: (lang) => {
    localStorage.setItem('am_lang', lang);
    set({ lang });
  },
  toggleSidebar: () => set({ sidebarOpen: !get().sidebarOpen }),
  closeSidebar: () => set({ sidebarOpen: false }),
}));
