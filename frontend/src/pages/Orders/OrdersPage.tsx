import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { ordersService } from '../../services/orders.service';
import { mechanicsService } from '../../services/mechanics.service';
import { StatusBadge, LicensePlate } from '../../components/UI/Badge';
import { Modal } from '../../components/UI/Modal';
import { ORDER_STATUSES, STATUS_LABELS_RU } from '../../types';
import type { Order, Mechanic } from '../../types';

export function OrdersPage() {
  const { t } = useTranslation();
  const [orders, setOrders] = useState<Order[]>([]);
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [mechanics, setMechanics] = useState<Mechanic[]>([]);

  async function load() {
    const res = await ordersService.list({ status: statusFilter || undefined, search: search || undefined });
    setOrders(res.items);
  }

  useEffect(() => {
    load();
  }, [statusFilter, search]);

  useEffect(() => {
    mechanicsService.list().then(setMechanics).catch(() => {});
  }, []);

  async function handleExport() {
    const blob = await ordersService.exportCsv();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'orders.csv';
    a.click();
    URL.revokeObjectURL(url);
  }

  async function handleDelete(id: string) {
    if (!confirm('Удалить заказ?')) return;
    await ordersService.remove(id);
    toast.success('Заказ удалён');
    load();
  }

  async function handleStatusChange(id: string, status: string) {
    await ordersService.updateStatus(id, status);
    toast.success('Статус обновлён');
    load();
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>{t('orders')}</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          <input
            placeholder={t('search')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{ width: 220 }}
          />
          <button className="btn btn-secondary" onClick={handleExport}>CSV</button>
          <button className="btn" onClick={() => setShowCreate(true)}>{t('newOrder')}</button>
        </div>
      </div>

      <div className="filter-chips">
        <div className={`chip ${!statusFilter ? 'active' : ''}`} onClick={() => setStatusFilter('')}>{t('all')}</div>
        {ORDER_STATUSES.map((s) => (
          <div key={s} className={`chip ${statusFilter === s ? 'active' : ''}`} onClick={() => setStatusFilter(s)}>
            {STATUS_LABELS_RU[s]}
          </div>
        ))}
      </div>

      <div className="card">
        <table>
          <thead>
            <tr>
              <th>№</th>
              <th>Клиент</th>
              <th>Авто</th>
              <th>Мастер</th>
              <th>{t('status')}</th>
              <th>Сумма</th>
              <th>{t('actions')}</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((o) => (
              <tr key={o.id}>
                <td>{o.orderNumber}</td>
                <td>{o.customerName}<br /><small>{o.customerPhone}</small></td>
                <td>{o.vehicleMakeModel} <LicensePlate value={o.licensePlate} /></td>
                <td>{o.mechanicName || '—'}</td>
                <td>
                  <select value={o.status} onChange={(e) => handleStatusChange(o.id, e.target.value)}>
                    {ORDER_STATUSES.map((s) => (
                      <option key={s} value={s}>{STATUS_LABELS_RU[s]}</option>
                    ))}
                  </select>
                  <StatusBadge status={o.status} />
                </td>
                <td>{o.totalCost.toLocaleString()} TJS</td>
                <td>
                  <button className="btn btn-secondary" onClick={() => handleDelete(o.id)}>✕</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showCreate && (
        <CreateOrderModal
          mechanics={mechanics}
          onClose={() => setShowCreate(false)}
          onCreated={() => {
            setShowCreate(false);
            load();
          }}
        />
      )}
    </div>
  );
}

function CreateOrderModal({
  mechanics,
  onClose,
  onCreated,
}: {
  mechanics: Mechanic[];
  onClose: () => void;
  onCreated: () => void;
}) {
  const { t } = useTranslation();
  const [form, setForm] = useState({
    customerName: '',
    customerPhone: '+992',
    vehicleMake: '',
    vehicleModel: '',
    licensePlate: '',
    mileage: 0,
    serviceDescription: '',
    mechanicId: '',
    priority: 'Normal',
    laborCost: 0,
    partsCost: 0,
  });
  const [error, setError] = useState('');

  function set<K extends keyof typeof form>(key: K, value: typeof form[K]) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await ordersService.create({ ...form, mechanicId: form.mechanicId || undefined });
      toast.success('Заказ создан');
      onCreated();
    } catch {
      setError('Не удалось создать заказ. Проверьте данные.');
    }
  }

  return (
    <Modal title={t('newOrder')} onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className="field"><label>Клиент</label><input required value={form.customerName} onChange={(e) => set('customerName', e.target.value)} /></div>
        <div className="field"><label>Телефон</label><input required value={form.customerPhone} onChange={(e) => set('customerPhone', e.target.value)} /></div>
        <div className="field"><label>Марка</label><input required value={form.vehicleMake} onChange={(e) => set('vehicleMake', e.target.value)} /></div>
        <div className="field"><label>Модель</label><input required value={form.vehicleModel} onChange={(e) => set('vehicleModel', e.target.value)} /></div>
        <div className="field"><label>Госномер</label><input required value={form.licensePlate} onChange={(e) => set('licensePlate', e.target.value)} /></div>
        <div className="field"><label>Пробег</label><input type="number" value={form.mileage} onChange={(e) => set('mileage', Number(e.target.value))} /></div>
        <div className="field"><label>Описание услуги</label><textarea required value={form.serviceDescription} onChange={(e) => set('serviceDescription', e.target.value)} /></div>
        <div className="field">
          <label>Мастер</label>
          <select value={form.mechanicId} onChange={(e) => set('mechanicId', e.target.value)}>
            <option value="">—</option>
            {mechanics.map((m) => <option key={m.id} value={m.id}>{m.fullName}</option>)}
          </select>
        </div>
        <div className="field"><label>Стоимость работ</label><input type="number" value={form.laborCost} onChange={(e) => set('laborCost', Number(e.target.value))} /></div>
        <div className="field"><label>Стоимость запчастей</label><input type="number" value={form.partsCost} onChange={(e) => set('partsCost', Number(e.target.value))} /></div>
        {error && <div className="error-text">{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={onClose}>{t('cancel')}</button>
          <button type="submit" className="btn">{t('save')}</button>
        </div>
      </form>
    </Modal>
  );
}
