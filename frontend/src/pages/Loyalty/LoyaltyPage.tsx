import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { loyaltyService } from '../../services/loyalty.service';
import { StatCard } from '../../components/UI/StatCard';
import type { LoyaltyStats, LoyaltyTopCustomer } from '../../types';

const RANK_MEDALS = ['🥇', '🥈', '🥉'];

export function LoyaltyPage() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<LoyaltyStats | null>(null);
  const [top, setTop] = useState<LoyaltyTopCustomer[]>([]);

  useEffect(() => {
    loyaltyService.stats().then(setStats);
    loyaltyService.top(10).then(setTop);
  }, []);

  return (
    <div>
      <h2>{t('loyalty')}</h2>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 16 }}>
        <StatCard label="Всего участников" value={stats?.totalMembers ?? 0} />
        <StatCard label="VIP клиентов" value={stats?.vipCount ?? 0} />
        <StatCard label="Выдано баллов" value={stats?.pointsAwarded ?? 0} />
        <StatCard label="Использовано баллов" value={stats?.pointsRedeemed ?? 0} />
      </div>

      <div className="card">
        <h3 style={{ marginTop: 0 }}>Топ клиенты</h3>
        <table>
          <thead><tr><th>Ранг</th><th>Клиент</th><th>Статус</th><th>Баллы</th><th>Потрачено</th></tr></thead>
          <tbody>
            {top.map((c, i) => (
              <tr key={c.customerId}>
                <td>{RANK_MEDALS[i] ?? i + 1}</td>
                <td>{c.fullName}</td>
                <td>{c.status}</td>
                <td>{c.loyaltyPoints}</td>
                <td>{c.totalSpent.toLocaleString()} TJS</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
