import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { authService } from '../../services/auth.service';
import { useAuthStore } from '../../store/authStore';

export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const setAuth = useAuthStore((s) => s.setAuth);
  const [email, setEmail] = useState('admin@automaster.tj');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const res = await authService.login(email, password);
      setAuth(res.user, res.accessToken);
      toast.success('Добро пожаловать!');
      navigate('/dashboard');
    } catch (err: any) {
      if (err?.response?.status === 429) {
        setError('Слишком много попыток входа. Подождите минуту и попробуйте снова.');
      } else {
        setError('Неверный email или пароль');
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <form className="card login-card" onSubmit={handleSubmit}>
        <h2 style={{ marginTop: 0, textAlign: 'center' }}>AutoMaster CRM</h2>
        <div className="field">
          <label>{t('email')}</label>
          <input value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
        </div>
        <div className="field">
          <label>{t('password')}</label>
          <input value={password} onChange={(e) => setPassword(e.target.value)} type="password" required />
        </div>
        {error && <div className="error-text">{error}</div>}
        <button className="btn" type="submit" disabled={loading} style={{ width: '100%', marginTop: 8 }}>
          {loading ? '...' : t('login')}
        </button>
      </form>
    </div>
  );
}
