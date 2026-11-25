import { api } from './api';
import type { Order, Paged } from '../types';

export interface CreateOrderRequest {
  customerName: string;
  customerPhone: string;
  vehicleMake: string;
  vehicleModel: string;
  licensePlate: string;
  vin?: string;
  mileage: number;
  serviceDescription: string;
  mechanicId?: string;
  priority: string;
  laborCost: number;
  partsCost: number;
  note?: string;
}

export const ordersService = {
  list: (params: { status?: string; search?: string; page?: number; limit?: number }) =>
    api.get<Paged<Order>>('/orders', { params }).then((r) => r.data),
  kanban: () => api.get<{ status: string; orders: Order[] }[]>('/orders/kanban').then((r) => r.data),
  getById: (id: string) => api.get(`/orders/${id}`).then((r) => r.data),
  create: (data: CreateOrderRequest) => api.post<Order>('/orders', data).then((r) => r.data),
  updateStatus: (id: string, status: string, note?: string) =>
    api.patch<Order>(`/orders/${id}/status`, { status, note }).then((r) => r.data),
  remove: (id: string) => api.delete(`/orders/${id}`),
  exportCsv: () => api.get('/orders/export/csv', { responseType: 'blob' }).then((r) => r.data)
};
