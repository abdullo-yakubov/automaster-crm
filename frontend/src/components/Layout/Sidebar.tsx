import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../store/authStore';
import type { Role } from '../../types';

const NAV: { to: string; key: string; roles: Role[] }[] = [
  { to: '/dashboard', key: 'dashboard', roles: ['Admin', 'Manager', 'Accountant'] },
  { to: '/orders', key: 'orders', roles: ['Admin', 'Manager'] },
  { to: '/kanban', key: 'kanban', roles: ['Admin', 'Manager', 'Mechanic'] },
  { to: '/schedule', key: 'schedule', roles: ['Admin', 'Manager'] },
  { to: '/customers', key: 'customers', roles: ['Admin', 'Manager'] },
  { to: '/vehicles', key: 'vehicles', roles: ['Admin', 'Manager'] },
  { to: '/loyalty', key: 'loyalty', roles: ['Admin', 'Manager'] },
  { to: '/mechanics', key: 'mechanics', roles: ['Admin', 'Manager', 'Accountant'] },
  { to: '/warehouse', key: 'warehouse', roles: ['Admin', 'Manager'] },
  { to: '/finance', key: 'finance', roles: ['Admin', 'Accountant'] },
  { to: '/reports', key: 'reports', roles: ['Admin', 'Accountant'] },
  { to: '/operations', key: 'operations', roles: ['Admin', 'Manager'] },
  { to: '/branches', key: 'branches', roles: ['Admin'] },
  { to: '/settings', key: 'settings', roles: ['Admin'] },
];

export function Sidebar() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);

  return (
    <nav className="sidebar">
      <div className="brand">AutoMaster CRM</div>
      {NAV.filter((item) => !user || item.roles.includes(user.role)).map((item) => (
        <NavLink key={item.to} to={item.to} className={({ isActive }) => (isActive ? 'active' : '')}>
          {t(item.key)}
        </NavLink>
      ))}
    </nav>
  );
}
