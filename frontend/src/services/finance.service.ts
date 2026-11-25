import { api } from './api';
import type { FinanceStats, Transaction } from '../types';

export const financeService = {
  stats: () => api.get<FinanceStats>('/finance/stats').then((r) => r.data),
  transactions: () => api.get<Transaction[]>('/finance/transactions').then((r) => r.data),
  create: (data: { type: string; amount: number; description: string; category: string; date: string }) =>
    api.post<Transaction>('/finance/transactions', data).then((r) => r.data),
  chart: (months = 6) => api.get('/finance/chart', { params: { months } }).then((r) => r.data),
  expensesStructure: () => api.get('/finance/expenses-structure').then((r) => r.data)
};
