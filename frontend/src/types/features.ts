// Типы ответов API для модулей: аналитика, AI, маркетинг, Digital Twin, голос, кабинет клиента, QR, BI.

export interface PopularService { service: string; count: number; revenue: number }

export interface MechanicPerformance {
  mechanicId: string; fullName: string; level: string; totalOrders: number; completedOrders: number;
  revenue: number; rating: number; salary: number; bonus: number;
}

export interface Heatmap { days: string[]; hours: number[]; values: number[][]; max: number }

export interface Forecast {
  history: { month: string; revenue: number }[];
  forecast: { month: string; expected: number; optimistic: number; pessimistic: number }[];
  monthlyGrowthPercent: number; nextMonth: number; nextSixMonthsTotal: number; insights: string[];
}

export interface DiagnosisHypothesis {
  issue: string; recommendation: string; probability: number; costFrom: number; costTo: number;
  urgency: 'high' | 'medium' | 'low';
}

export interface DiagnosisResult {
  vehicleTitle: string | null; mileage: number | null; problem: string;
  hypotheses: DiagnosisHypothesis[]; recentServices: string[]; disclaimer: string;
}

export interface PredictiveItem {
  vehicleId: string; title: string; licensePlate: string; ownerName: string; ownerPhone: string;
  year: number; mileage: number; lastServiceAt: string | null; riskLevel: 'high' | 'medium' | 'low'; recommendations: string[];
}

export interface MakeIssueStat { make: string; orders: number; topServices: string[] }

export interface MarketingStats { campaigns: number; totalRecipients: number; activePromos: number; segmentSizes: Record<string, number> }

export interface PromoCode {
  id: string; code: string; discountPercent: number; expiresAt: string; usedCount: number;
  maxUses: number | null; isActive: boolean; isExpired: boolean;
}

export interface Campaign {
  id: string; name: string; channel: string; segment: string; message: string;
  recipientsCount: number; status: string; createdAt: string;
}

export interface ServiceBox {
  id: string; name: string; specialization: string; isActive: boolean; orderId: string | null;
  orderNumber: string | null; customerName: string | null; vehicle: string | null; service: string | null;
  mechanicName: string | null; occupiedSince: string | null; progressPercent: number;
}

export interface QueuedOrder { id: string; orderNumber: string; customerName: string; vehicle: string; service: string; status: string; priority: string }

export interface DigitalTwin {
  boxes: ServiceBox[]; queue: QueuedOrder[]; totalBoxes: number; busyBoxes: number; loadPercent: number;
  averageRepairHours: number; throughputPerDay: number; hourlyLoad: { hour: number; actual: number; forecast: number }[];
  insights: string[];
}

export interface VoiceResult { intent: string; recognized: boolean; message: string; route: string | null; prefillService: string | null }
export interface VoiceHistoryItem { id: string; transcript: string; intent: string; result: string; createdAt: string }

export interface PortalOrder {
  id: string; orderNumber: string; vehicle: string; service: string; status: string;
  mechanicName: string | null; total: number; createdAt: string; completedAt: string | null;
}

export interface ClientPortal {
  customerId: string; fullName: string; phone: string; status: string; loyaltyPoints: number; visits: number;
  totalSpent: number; activeOrder: PortalOrder | null;
  vehicles: { id: string; title: string; licensePlate: string; year: number; mileage: number; vin: string | null }[];
  history: PortalOrder[]; notifications: { text: string; kind: 'success' | 'warning' | 'info' }[];
}

export interface SignedQr { type: string; payload: string; label: string; expiresAt: string }
export interface QrVerification { isValid: boolean; type: string | null; data: Record<string, string> | null; reason: string | null }

export type BiWidgetId =
  | 'revenue_card' | 'orders_chart' | 'mechanics_kpi_widget' | 'warehouse_alert'
  | 'top_services' | 'profit_margin' | 'customer_map' | 'sla_monitor';

export interface BiSummary {
  revenueThisMonth: number; revenueLastMonth: number; revenueChangePercent: number;
  weekdayLabels: string[]; ordersByWeekday: number[]; topMechanic: MechanicPerformance | null;
  lowStock: { id: string; name: string; quantity: number; minQuantity: number }[];
  topServices: PopularService[]; income: number; expense: number; marginPercent: number;
  segments: { segment: string; count: number }[]; activeOrders: number; slaViolations: number; slaHours: number;
}
