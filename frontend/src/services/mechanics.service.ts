import { api } from './api';
import type { Mechanic } from '../types';

export const mechanicsService = {
  list: () => api.get<Mechanic[]>('/mechanics').then((r) => r.data),
  top: (limit = 5) => api.get('/mechanics/top', { params: { limit } }).then((r) => r.data),
  create: (data: Partial<Mechanic>) => api.post<Mechanic>('/mechanics', data).then((r) => r.data),
  update: (id: string, data: Partial<Mechanic>) => api.put<Mechanic>(`/mechanics/${id}`, data).then((r) => r.data),
  remove: (id: string) => api.delete(`/mechanics/${id}`)
};
