import { api } from './api';
import type { DashboardStats } from '../types';

export const dashboardService = {
  stats: () => api.get<DashboardStats>('/dashboard/stats').then((r) => r.data),
  revenue: (days = 7) => api.get('/dashboard/revenue', { params: { days } }).then((r) => r.data),
  servicesDistribution: () => api.get('/dashboard/services-distribution').then((r) => r.data)
};
