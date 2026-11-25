import { api } from './api';
import type {
  BiSummary, BiWidgetId, Campaign, ClientPortal, DiagnosisResult, DigitalTwin, Forecast, Heatmap,
  MakeIssueStat, MarketingStats, MechanicPerformance, PopularService, PredictiveItem, PromoCode,
  QrVerification, ServiceBox, SignedQr, VoiceHistoryItem, VoiceResult,
} from '../types/features';

const get = <T,>(url: string) => api.get<T>(url).then((r) => r.data);
const post = <T,>(url: string, body?: unknown) => api.post<T>(url, body).then((r) => r.data);

export const analyticsService = {
  popularServices: (top = 10) => get<PopularService[]>(`/analytics/popular-services?top=${top}`),
  mechanics: () => get<MechanicPerformance[]>('/analytics/mechanics'),
  heatmap: (days = 90) => get<Heatmap>(`/analytics/heatmap?days=${days}`),
  forecast: () => get<Forecast>('/analytics/forecast'),
};

export const aiService = {
  diagnose: (body: { plateOrVin?: string; problem: string; mileage?: number }) => post<DiagnosisResult>('/ai/diagnose', body),
  predictive: () => get<PredictiveItem[]>('/ai/predictive'),
  issuesByMake: () => get<MakeIssueStat[]>('/ai/issues-by-make'),
};

export const marketingService = {
  stats: () => get<MarketingStats>('/marketing/stats'),
  promos: () => get<PromoCode[]>('/marketing/promos'),
  createPromo: (body: { code?: string; discountPercent: number; expiresAt: string; maxUses?: number }) =>
    post<PromoCode>('/marketing/promos', body),
  setPromoActive: (id: string, isActive: boolean) => api.patch<PromoCode>(`/marketing/promos/${id}`, { isActive }).then((r) => r.data),
  deletePromo: (id: string) => api.delete(`/marketing/promos/${id}`),
  campaigns: () => get<Campaign[]>('/marketing/campaigns'),
  createCampaign: (body: { channel: string; segment: string; message: string }) => post<Campaign>('/marketing/campaigns', body),
  template: (segment: string) => get<{ segment: string; text: string }>(`/marketing/templates/${encodeURIComponent(segment)}`),
};

export const digitalTwinService = {
  state: () => get<DigitalTwin>('/digital-twin'),
  createBox: (body: { name: string; specialization: string }) => post<ServiceBox>('/digital-twin/boxes', body),
  assign: (boxId: string, orderId: string) => post<void>(`/digital-twin/boxes/${boxId}/assign`, { orderId }),
  release: (boxId: string) => post<void>(`/digital-twin/boxes/${boxId}/release`),
};

export const voiceService = {
  command: (transcript: string) => post<VoiceResult>('/voice/command', { transcript }),
  history: () => get<VoiceHistoryItem[]>('/voice/history'),
};

export const portalService = {
  me: () => get<ClientPortal>('/portal/me'),
  preview: (customerId: string) => get<ClientPortal>(`/portal/customers/${customerId}`),
};

export const qrService = {
  warranty: (orderId: string) => post<SignedQr>('/qr/warranty', { orderId }),
  payment: (orderId: string) => post<SignedQr>('/qr/payment', { orderId }),
  promo: (code: string) => post<SignedQr>('/qr/promo', { code }),
  verify: (payload: string) => post<QrVerification>('/qr/verify', { payload }),
};

export const biService = {
  widgets: () => get<BiWidgetId[]>('/bi/widgets'),
  saveWidgets: (widgets: BiWidgetId[]) => api.put<BiWidgetId[]>('/bi/widgets', { widgets }).then((r) => r.data),
  summary: () => get<BiSummary>('/bi/summary'),
};

/** Достаёт понятное сообщение об ошибке из ответа API (бизнес-ошибка или ошибки валидации). */
export function apiError(err: unknown, fallback = 'Ошибка запроса'): string {
  const data = (err as { response?: { data?: { error?: string; errors?: Record<string, string[]> } } })?.response?.data;
  if (data?.error) return data.error;
  if (data?.errors) return Object.values(data.errors).flat()[0] ?? fallback;
  return fallback;
}
