import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { settingsService } from '../../services/settings.service';
import { useUiStore } from '../../store/uiStore';

export function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { theme, toggleTheme, lang, setLang } = useUiStore();
  const [serviceName, setServiceName] = useState('');
  const [address, setAddress] = useState('');
  const [phone, setPhone] = useState('');
  const [currency, setCurrency] = useState('TJS');

  useEffect(() => {
    settingsService.get().then((s) => {
      setServiceName(s.serviceName);
      setAddress(s.address);
      setPhone(s.phone);
      setCurrency(s.currency);
    });
  }, []);

  async function handleSave() {
    await settingsService.update({ serviceName, address, phone, currency, theme, language: lang });
    toast.success('Настройки сохранены');
  }

  function handleSetLang(l: 'ru' | 'tj') {
    setLang(l);
    i18n.changeLanguage(l);
  }

  async function handleExport() {
    const data = await settingsService.exportData();
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `automaster-export-${Date.now()}.json`;
    link.click();
    URL.revokeObjectURL(url);
  }

  return (
    <div>
      <h2>{t('settings')}</h2>

      <div className="card" style={{ marginBottom: 16 }}>
        <h3 style={{ marginTop: 0 }}>Сервис</h3>
        <div className="field"><label>Название сервиса</label><input value={serviceName} onChange={(e) => setServiceName(e.target.value)} /></div>
        <div className="field"><label>Адрес</label><input value={address} onChange={(e) => setAddress(e.target.value)} /></div>
        <div className="field"><label>Телефон</label><input value={phone} onChange={(e) => setPhone(e.target.value)} /></div>
        <div className="field"><label>Валюта</label>
          <select value={currency} onChange={(e) => setCurrency(e.target.value)}>
            <option value="TJS">TJS</option>
            <option value="USD">USD</option>
            <option value="RUB">RUB</option>
          </select>
        </div>
        <button className="btn" onClick={handleSave}>{t('save')}</button>
      </div>

      <div className="card" style={{ marginBottom: 16 }}>
        <h3 style={{ marginTop: 0 }}>Данные</h3>
        <div style={{ display: 'flex', gap: 8 }}>
          <button className="btn btn-secondary" onClick={handleExport}>Экспорт всех данных (JSON)</button>
        </div>
      </div>

      <div className="card">
        <h3 style={{ marginTop: 0 }}>Тема &amp; Язык</h3>
        <div style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
          <button className="btn btn-secondary" onClick={toggleTheme}>Переключить тему (текущая: {theme === 'dark' ? 'тёмная' : 'светлая'})</button>
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <button className={`btn ${lang === 'ru' ? '' : 'btn-secondary'}`} onClick={() => handleSetLang('ru')}>Русский</button>
          <button className={`btn ${lang === 'tj' ? '' : 'btn-secondary'}`} onClick={() => handleSetLang('tj')}>Тоҷикӣ</button>
        </div>
      </div>
    </div>
  );
}
