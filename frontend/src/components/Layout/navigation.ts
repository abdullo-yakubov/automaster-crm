import type { Role } from '../../types';

export interface NavItem {
  to: string;
  key: string;
  icon: string;
  roles: Role[];
}

export interface NavSection {
  label: string;
  items: NavItem[];
}

const STAFF: Role[] = ['Admin', 'Manager'];
const ANALYSTS: Role[] = ['Admin', 'Manager', 'Accountant'];

/** Единый источник навигации: используется сайдбаром, заголовком и роутером. */
export const NAV_SECTIONS: NavSection[] = [
  { label: 'nav_core', items: [{ to: '/dashboard', key: 'dashboard', icon: '📊', roles: ANALYSTS }] },
  {
    label: 'nav_operations',
    items: [
      { to: '/orders', key: 'orders', icon: '🔩', roles: STAFF },
      { to: '/kanban', key: 'kanban', icon: '📋', roles: ['Admin', 'Manager', 'Mechanic'] },
      { to: '/schedule', key: 'schedule', icon: '📅', roles: STAFF },
    ],
  },
  {
    label: 'nav_crm',
    items: [
      { to: '/customers', key: 'customers', icon: '👥', roles: STAFF },
      { to: '/vehicles', key: 'vehicles', icon: '🚗', roles: STAFF },
      { to: '/loyalty', key: 'loyalty', icon: '⭐', roles: STAFF },
    ],
  },
  { label: 'nav_staff', items: [{ to: '/mechanics', key: 'mechanics', icon: '👨‍🔧', roles: ANALYSTS }] },
  { label: 'nav_inventory', items: [{ to: '/warehouse', key: 'warehouse', icon: '📦', roles: STAFF }] },
  {
    label: 'nav_finance',
    items: [
      { to: '/finance', key: 'finance', icon: '💰', roles: ['Admin', 'Accountant'] },
      { to: '/analytics', key: 'analytics', icon: '📈', roles: ANALYSTS },
    ],
  },
  {
    label: 'nav_ai',
    items: [
      { to: '/ai', key: 'ai', icon: '🤖', roles: ['Admin', 'Manager', 'Mechanic'] },
      { to: '/digital-twin', key: 'digital_twin', icon: '🏭', roles: ['Admin', 'Manager', 'Mechanic'] },
      { to: '/voice', key: 'voice', icon: '🎤', roles: ['Admin', 'Manager', 'Mechanic', 'Accountant'] },
    ],
  },
  {
    label: 'nav_sales',
    items: [
      { to: '/marketing', key: 'marketing', icon: '📣', roles: STAFF },
      { to: '/qr', key: 'qr', icon: '📱', roles: STAFF },
      { to: '/client-portal', key: 'client_portal', icon: '🌐', roles: ['Admin', 'Manager', 'Client'] },
    ],
  },
  {
    label: 'nav_management',
    items: [
      { to: '/operations', key: 'operations', icon: '✅', roles: STAFF },
      { to: '/branches', key: 'branches', icon: '🌍', roles: ['Admin'] },
      { to: '/bi', key: 'bi', icon: '📊', roles: ANALYSTS },
      { to: '/reports', key: 'reports', icon: '📑', roles: ['Admin', 'Accountant'] },
      { to: '/settings', key: 'settings', icon: '⚙️', roles: ['Admin'] },
    ],
  },
];

export const ALL_NAV_ITEMS = NAV_SECTIONS.flatMap((s) => s.items);

/** Первая доступная роли страница — куда отправлять после входа. */
export function homeFor(role: Role | undefined): string {
  if (!role) return '/login';
  return ALL_NAV_ITEMS.find((i) => i.roles.includes(role))?.to ?? '/login';
}
