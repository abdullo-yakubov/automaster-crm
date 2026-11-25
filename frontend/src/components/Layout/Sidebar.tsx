import { NavLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../store/authStore';
import { useUiStore } from '../../store/uiStore';
import { NAV_SECTIONS } from './navigation';

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Администратор',
  Manager: 'Менеджер',
  Mechanic: 'Мастер',
  Accountant: 'Бухгалтер',
  Client: 'Клиент',
};

function initials(name?: string) {
  if (!name) return '?';
  return name.split(' ').filter(Boolean).slice(0, 2).map((p) => p[0]).join('').toUpperCase();
}

export function Sidebar() {
  const { t } = useTranslation();
  const user = useAuthStore((s) => s.user);
  const { sidebarOpen, closeSidebar } = useUiStore();

  const sections = NAV_SECTIONS.map((s) => ({
    ...s,
    items: s.items.filter((i) => user && i.roles.includes(user.role)),
  })).filter((s) => s.items.length > 0);

  return (
    <>
      <div className={`sidebar-overlay${sidebarOpen ? ' open' : ''}`} onClick={closeSidebar} />
      <nav className={`sidebar${sidebarOpen ? ' open' : ''}`} aria-label="Главное меню">
        <div className="logo-area">
          <div className="logo-icon">🔧</div>
          <div className="logo-text">
            <div className="logo-title">AutoMaster</div>
            <div className="logo-sub">CRM v3</div>
          </div>
        </div>

        {sections.map((section) => (
          <div className="nav-section" key={section.label}>
            <div className="nav-label">{t(section.label)}</div>
            {section.items.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                onClick={closeSidebar}
                className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
              >
                <span className="nav-icon">{item.icon}</span>
                <span>{t(item.key)}</span>
              </NavLink>
            ))}
          </div>
        ))}

        <div className="sidebar-bottom">
          <div className="user-card">
            <div className="user-avatar">{initials(user?.fullName)}</div>
            <div className="user-info">
              <div className="user-name">{user?.fullName}</div>
              <div className="user-role">{user ? ROLE_LABELS[user.role] ?? user.role : ''}</div>
            </div>
          </div>
        </div>
      </nav>
    </>
  );
}
