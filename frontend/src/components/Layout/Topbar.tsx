import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../store/authStore';
import { useUiStore } from '../../store/uiStore';
import { authService } from '../../services/auth.service';
import { ALL_NAV_ITEMS } from './navigation';

export function Topbar() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const { theme, lang, toggleTheme, setLang, toggleSidebar } = useUiStore();

  const current = ALL_NAV_ITEMS.find((i) => pathname.startsWith(i.to));
  const canCreateOrder = user?.role === 'Admin' || user?.role === 'Manager';

  function handleLang(next: 'ru' | 'tj') {
    setLang(next);
    i18n.changeLanguage(next);
  }

  async function handleLogout() {
    try {
      await authService.logout();
    } finally {
      clearAuth();
      navigate('/login');
    }
  }

  return (
    <header className="topbar">
      <button className="hamburger" onClick={toggleSidebar} aria-label="Меню">☰</button>
      <div className="topbar-title">{current ? t(current.key).toUpperCase() : 'AUTOMASTER'}</div>
      <div className="topbar-actions">
        {canCreateOrder && (
          <button className="topbar-btn" title={t('newOrder')} onClick={() => navigate('/orders?new=1')}>➕</button>
        )}
        <button className="topbar-btn" title="Тема" onClick={toggleTheme}>{theme === 'dark' ? '🌙' : '☀️'}</button>
        <div className="lang-toggle">
          <button className={`lang-btn${lang === 'ru' ? ' active' : ''}`} onClick={() => handleLang('ru')}>RU</button>
          <button className={`lang-btn${lang === 'tj' ? ' active' : ''}`} onClick={() => handleLang('tj')}>TJ</button>
        </div>
        <span className="topbar-user">{user?.fullName}</span>
        <button className="topbar-btn" title={t('logout')} onClick={handleLogout}>⎋</button>
      </div>
    </header>
  );
}
