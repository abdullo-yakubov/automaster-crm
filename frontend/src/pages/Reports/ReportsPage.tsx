import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { reportsService } from '../../services/reports.service';
import { ordersService } from '../../services/orders.service';
import { STATUS_LABELS_RU } from '../../types';
import type { Order } from '../../types';

export function ReportsPage() {
  const { t } = useTranslation();
  const [orders, setOrders] = useState<Order[]>([]);

  useEffect(() => {
    ordersService.list({ limit: 100 }).then((res) => setOrders(res.items));
  }, []);

  async function run(action: () => Promise<void>, label: string) {
    try {
      await action();
      toast.success(label);
    } catch {
      toast.error('Не удалось сформировать отчёт');
    }
  }

  return (
    <div>
      <h2>{t('reports')}</h2>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16, marginBottom: 16 }}>
        <button className="card" style={{ cursor: 'pointer', textAlign: 'left' }} onClick={() => run(reportsService.dailyPdf, 'Дневной отчёт сформирован')}>
          <h3 style={{ marginTop: 0 }}>📄 Дневной отчёт</h3>
          <div>PDF за сегодня</div>
        </button>
        <button className="card" style={{ cursor: 'pointer', textAlign: 'left' }} onClick={() => run(reportsService.weeklyPdf, 'Недельный отчёт сформирован')}>
          <h3 style={{ marginTop: 0 }}>📅 Недельный отчёт</h3>
          <div>PDF за 7 дней</div>
        </button>
        <button className="card" style={{ cursor: 'pointer', textAlign: 'left' }} onClick={() => run(reportsService.ordersCsv, 'CSV экспортирован')}>
          <h3 style={{ marginTop: 0 }}>📊 Экспорт CSV</h3>
          <div>Заказы в Excel</div>
        </button>
      </div>

      <div className="card">
        <h3 style={{ marginTop: 0 }}>Полная история заказов</h3>
        <table>
          <thead>
            <tr><th>№</th><th>Дата</th><th>Клиент</th><th>Авто</th><th>Гос. номер</th><th>Услуги</th><th>Мастер</th><th>Оплата</th><th>Статус</th></tr>
          </thead>
          <tbody>
            {orders.map((o) => (
              <tr key={o.id}>
                <td>{o.orderNumber}</td>
                <td>{new Date(o.createdAt).toLocaleDateString()}</td>
                <td>{o.customerName}</td>
                <td>{o.vehicleMakeModel}</td>
                <td>{o.licensePlate}</td>
                <td>{o.serviceDescription}</td>
                <td>{o.mechanicName ?? '—'}</td>
                <td>{o.totalCost.toLocaleString()} TJS</td>
                <td>{STATUS_LABELS_RU[o.status] ?? o.status}</td>
              </tr>
            ))}
            {orders.length === 0 && <tr><td colSpan={9}>Нет заказов</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  );
}
