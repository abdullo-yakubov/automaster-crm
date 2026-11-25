export type Role = 'Admin' | 'Manager' | 'Mechanic' | 'Accountant' | 'Client';

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: Role;
  phone?: string;
  isActive: boolean;
}

export interface Order {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  customerPhone: string;
  vehicleId: string;
  vehicleMakeModel: string;
  licensePlate: string;
  mechanicId?: string;
  mechanicName?: string;
  serviceDescription: string;
  status: string;
  priority: string;
  laborCost: number;
  partsCost: number;
  totalCost: number;
  note?: string;
  createdAt: string;
  updatedAt: string;
  completedAt?: string;
}

export interface Customer {
  id: string;
  fullName: string;
  phone: string;
  email?: string;
  birthday?: string;
  status: string;
  loyaltyPoints: number;
  totalSpent: number;
  note?: string;
  createdAt: string;
  lastVisit?: string;
  vehicleCount: number;
}

export interface Vehicle {
  id: string;
  customerId: string;
  customerName: string;
  make: string;
  model: string;
  year: number;
  licensePlate: string;
  vin?: string;
  mileage: number;
  visitCount: number;
  createdAt: string;
}

export interface Mechanic {
  id: string;
  fullName: string;
  phone?: string;
  specialization: string;
  level: string;
  salary: number;
  isActive: boolean;
  rating: number;
  completedOrders: number;
  revenue: number;
  workloadPercent: number;
}

export interface DashboardStats {
  ordersToday: number;
  ordersTodayChangePercent: number;
  revenueToday: number;
  activeOrders: number;
  totalCustomers: number;
  averageTicket: number;
}

export interface Paged<T> {
  items: T[];
  total: number;
  page: number;
  limit: number;
}

export const ORDER_STATUSES = [
  'Intake', 'Diagnosis', 'Estimate', 'Approved', 'InProgress', 'Waiting', 'Ready', 'Completed'
] as const;

export const STATUS_LABELS_RU: Record<string, string> = {
  Intake: 'Приём', Diagnosis: 'Диагностика', Estimate: 'Смета', Approved: 'Одобрен',
  InProgress: 'В работе', Waiting: 'Ожидание', Ready: 'Готово', Completed: 'Завершён'
};

export const STATUS_COLORS: Record<string, string> = {
  Intake: '#3b82f6', Diagnosis: '#a855f7', Estimate: '#eab308', Approved: '#f97316',
  InProgress: '#06b6d4', Waiting: '#ef4444', Ready: '#22c55e', Completed: '#6b7280'
};

export interface Booking {
  id: string;
  customerName: string;
  phone: string;
  date: string;
  time: string;
  service: string;
  status: string;
}

export interface Slot {
  time: string;
  isFree: boolean;
}

export interface LoyaltyStats {
  totalMembers: number;
  vipCount: number;
  pointsAwarded: number;
  pointsRedeemed: number;
}

export interface LoyaltyTopCustomer {
  customerId: string;
  fullName: string;
  status: string;
  loyaltyPoints: number;
  totalSpent: number;
}

export interface Part {
  id: string;
  name: string;
  sku: string;
  category: string;
  quantity: number;
  minQuantity: number;
  purchasePrice: number;
  salePrice: number;
  marginPercent: number;
  status: string;
}

export interface WarehouseStats {
  totalItems: number;
  lowStockCount: number;
  totalValue: number;
  monthlyTurnover: number;
}

export interface Transaction {
  id: string;
  type: string;
  amount: number;
  description: string;
  category: string;
  date: string;
}

export interface FinanceStats {
  revenueThisMonth: number;
  expensesThisMonth: number;
  profit: number;
  marginPercent: number;
}

export interface AppSettings {
  id: string;
  serviceName: string;
  address: string;
  phone: string;
  currency: string;
  theme: string;
  language: string;
}

export interface Branch {
  id: string;
  name: string;
  address: string;
  phone: string;
  type: string;
  isActive: boolean;
  ordersCount: number;
  revenue: number;
  nps: number;
}

export interface NetworkKpi {
  branchesCount: number;
  totalRevenue: number;
  totalOrders: number;
  averageNps: number;
  branches: Branch[];
}

export interface ChecklistItem {
  id: string;
  type: string;
  title: string;
  isDone: boolean;
  sortOrder: number;
}

export interface SensorReading {
  name: string;
  type: string;
  value: number;
  unit: string;
  readAt: string;
}

export interface SlaViolation {
  orderId: string;
  orderNumber: string;
  customerName: string;
  status: string;
  slaHours: number;
  elapsedHours: number;
  overHours: number;
}

export interface MysteryAuditScore {
  greeting: number;
  speed: number;
  cleanliness: number;
  politeness: number;
  repairQuality: number;
  billAccuracy: number;
  handoverTime: number;
}

export interface MysteryAudit {
  id: string;
  scores: MysteryAuditScore;
  totalScore: number;
  createdAt: string;
}

export interface AuditLog {
  id: string;
  createdAt: string;
  userName: string;
  action: string;
  objectType: string;
  objectId: string;
  details?: string;
}
