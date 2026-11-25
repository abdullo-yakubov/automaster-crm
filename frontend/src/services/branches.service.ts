import { api } from './api';
import type { Branch, NetworkKpi } from '../types';

export const branchesService = {
  list: () => api.get<Branch[]>('/branches').then((r) => r.data),
  networkKpi: () => api.get<NetworkKpi>('/branches/network-kpi').then((r) => r.data),
  create: (data: { name: string; address: string; phone: string; type: string }) =>
    api.post<Branch>('/branches', data).then((r) => r.data),
  remove: (id: string) => api.delete(`/branches/${id}`)
};
