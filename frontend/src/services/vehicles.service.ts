import { api } from './api';
import type { Vehicle, Paged } from '../types';

export const vehiclesService = {
  list: (params: { search?: string; page?: number; limit?: number }) =>
    api.get<Paged<Vehicle>>('/vehicles', { params }).then((r) => r.data),
  create: (data: Partial<Vehicle> & { ownerName?: string; customerId?: string }) =>
    api.post<Vehicle>('/vehicles', data).then((r) => r.data),
  update: (id: string, data: Partial<Vehicle>) => api.put<Vehicle>(`/vehicles/${id}`, data).then((r) => r.data),
  remove: (id: string) => api.delete(`/vehicles/${id}`)
};
