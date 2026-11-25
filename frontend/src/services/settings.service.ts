import { api } from './api';
import type { AppSettings } from '../types';

export const settingsService = {
  get: () => api.get<AppSettings>('/settings').then((r) => r.data),
  update: (data: Partial<AppSettings>) => api.put<AppSettings>('/settings', data).then((r) => r.data),
  exportData: () => api.get('/data/export').then((r) => r.data)
};
