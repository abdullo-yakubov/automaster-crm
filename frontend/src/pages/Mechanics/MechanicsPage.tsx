import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { mechanicsService } from '../../services/mechanics.service';
import type { Mechanic } from '../../types';

export function MechanicsPage() {
  const { t } = useTranslation();
  const [mechanics, setMechanics] = useState<Mechanic[]>([]);

  useEffect(() => {
    mechanicsService.list().then(setMechanics);
  }, []);

  return (
    <div>
      <h2>{t('mechanics')}</h2>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: 16 }}>
        {mechanics.map((m) => (
          <div className="card" key={m.id}>
            <h3 style={{ marginTop: 0 }}>{m.fullName}</h3>
            <div>{m.specialization} · {m.level}</div>
            <div>Рейтинг: ⭐ {m.rating.toFixed(1)}</div>
            <div>Завершено заказов: {m.completedOrders}</div>
            <div>Выручка: {m.revenue.toLocaleString()} TJS</div>
            <div style={{ marginTop: 8 }}>
              <div style={{ background: 'var(--border)', borderRadius: 6, height: 8, overflow: 'hidden' }}>
                <div style={{ width: `${m.workloadPercent}%`, background: 'var(--accent)', height: '100%' }} />
              </div>
              <small>Загрузка: {m.workloadPercent}%</small>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
