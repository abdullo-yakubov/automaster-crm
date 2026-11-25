import { api } from './api';
import type { Part, WarehouseStats } from '../types';

export const warehouseService = {
  list: (params: { filter?: string; search?: string }) =>
    api.get<Part[]>('/parts', { params }).then((r) => r.data),
  stats: () => api.get<WarehouseStats>('/warehouse/stats').then((r) => r.data),
  create: (data: Partial<Part>) => api.post<Part>('/parts', data).then((r) => r.data),
  update: (id: string, data: Partial<Part>) => api.put<Part>(`/parts/${id}`, data).then((r) => r.data),
  updateQuantity: (id: string, quantity: number) => api.patch(`/parts/${id}/quantity`, { quantity }),
  remove: (id: string) => api.delete(`/parts/${id}`)
};
