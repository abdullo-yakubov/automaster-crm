import { api } from './api';
import type { Booking, Slot } from '../types';

export const scheduleService = {
  bookings: (date: string) => api.get<Booking[]>('/bookings', { params: { date } }).then((r) => r.data),
  slots: (date: string) => api.get<Slot[]>('/bookings/slots', { params: { date } }).then((r) => r.data),
  create: (data: { customerName: string; phone: string; date: string; time: string; service: string }) =>
    api.post<Booking>('/bookings', data).then((r) => r.data),
  remove: (id: string) => api.delete(`/bookings/${id}`)
};
