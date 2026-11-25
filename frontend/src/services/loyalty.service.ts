import { api } from './api';
import type { LoyaltyStats, LoyaltyTopCustomer } from '../types';

export const loyaltyService = {
  stats: () => api.get<LoyaltyStats>('/loyalty/stats').then((r) => r.data),
  top: (limit = 10) => api.get<LoyaltyTopCustomer[]>('/loyalty/top', { params: { limit } }).then((r) => r.data),
  award: (customerId: string, points: number, note?: string) =>
    api.post('/loyalty/award', { customerId, points, note }),
  redeem: (customerId: string, points: number, note?: string) =>
    api.post('/loyalty/redeem', { customerId, points, note })
};
