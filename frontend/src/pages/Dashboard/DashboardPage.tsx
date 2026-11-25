import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Line, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  Tooltip,
  Legend,
} from 'chart.js';
import { dashboardService } from '../../services/dashboard.service';
import { StatCard } from '../../components/UI/StatCard';
import type { DashboardStats } from '../../types';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, ArcElement, Tooltip, Legend);

export function DashboardPage() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [revenue, setRevenue] = useState<{ date: string; total: number }[]>([]);
  const [services, setServices] = useState<{ name: string; count: number }[]>([]);

  useEffect(() => {
    dashboardService.stats().then(setStats).catch(() => {});
    dashboardService.revenue(7).then(setRevenue).catch(() => {});
    dashboardService.servicesDistribution().then(setServices).catch(() => {});
  }, []);

  return (
    <div>
      <h2>{t('dashboard')}</h2>
      {stats && (
        <div className="stats-grid">
          <StatCard label={t('ordersToday')} value={stats.ordersToday} />
          <StatCard label={t('revenueToday')} value={stats.revenueToday.toLocaleString()} suffix=" TJS" />
          <StatCard label={t('activeOrders')} value={stats.activeOrders} />
          <StatCard label={t('totalCustomers')} value={stats.totalCustomers} />
          <StatCard label={t('averageTicket')} value={stats.averageTicket.toLocaleString()} suffix=" TJS" />
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 16 }}>
        <div className="card">
          <h3>Выручка за 7 дней</h3>
          <Line
            data={{
              labels: revenue.map((r) => r.date),
              datasets: [{ label: 'TJS', data: revenue.map((r) => r.total), borderColor: '#4ea4f5', tension: 0.3 }],
            }}
          />
        </div>
        <div className="card">
          <h3>Услуги</h3>
          <Doughnut
            data={{
              labels: services.map((s) => s.name),
              datasets: [
                {
                  data: services.map((s) => s.count),
                  backgroundColor: ['#4ea4f5', '#a855f7', '#22c55e', '#eab308', '#ef4444', '#06b6d4'],
                },
              ],
            }}
          />
        </div>
      </div>
    </div>
  );
}
