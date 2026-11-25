import { api } from './api';
import type { ChecklistItem, SensorReading, SlaViolation, MysteryAudit, MysteryAuditScore, AuditLog } from '../types';

export const operationsService = {
  checklist: (type: string) => api.get<ChecklistItem[]>(`/checklists/${type}`).then((r) => r.data),
  toggleItem: (type: string, itemId: string) => api.patch<ChecklistItem>(`/checklists/${type}/${itemId}`).then((r) => r.data),
  addItem: (type: string, title: string) => api.post<ChecklistItem>('/checklists/items', { type, title }).then((r) => r.data),
  sensors: () => api.get<SensorReading[]>('/iot/sensors').then((r) => r.data),
  slaViolations: () => api.get<SlaViolation[]>('/orders/sla-violations').then((r) => r.data),
  saveMysteryAudit: (scores: MysteryAuditScore) => api.post<MysteryAudit>('/audit/mystery-shopper', { scores }).then((r) => r.data),
  mysteryAudits: () => api.get<MysteryAudit[]>('/audit/mystery-shopper').then((r) => r.data),
  auditLogs: () => api.get<AuditLog[]>('/audit/logs').then((r) => r.data)
};
