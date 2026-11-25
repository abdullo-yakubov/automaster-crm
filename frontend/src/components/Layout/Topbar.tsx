import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../store/authStore';
import { useUiStore } from '../../store/uiStore';
import { authService } from '../../services/auth.service';

export function Topbar() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const clearAuth = useAuthStore((s) => s.clearAuth);
  const { theme, lang, toggleTheme, setLang } = useUiStore();

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
      <div />
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <button className="btn-secondary btn" onClick={() => handleLang(lang === 'ru' ? 'tj' : 'ru')}>
          {lang === 'ru' ? 'RU' : 'TJ'}
        </button>
        <button className="btn-secondary btn" onClick={toggleTheme}>
          {theme === 'dark' ? '🌙' : '☀️'}
        </button>
        <span>{user?.fullName} ({user?.role})</span>
        <button className="btn" onClick={handleLogout}>{t('logout')}</button>
      </div>
    </header>
  );
}
