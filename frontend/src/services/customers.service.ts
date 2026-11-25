import { api } from './api';
import type { Customer, Paged } from '../types';

export const customersService = {
  list: (params: { search?: string; status?: string; page?: number; limit?: number }) =>
    api.get<Paged<Customer>>('/customers', { params }).then((r) => r.data),
  getById: (id: string) => api.get(`/customers/${id}`).then((r) => r.data),
  create: (data: Partial<Customer> & { vehicleMake?: string; vehicleModel?: string; licensePlate?: string }) =>
    api.post<Customer>('/customers', data).then((r) => r.data),
  update: (id: string, data: Partial<Customer>) => api.put<Customer>(`/customers/${id}`, data).then((r) => r.data),
  remove: (id: string) => api.delete(`/customers/${id}`)
};
