import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { branchesService } from '../../services/branches.service';
import { StatCard } from '../../components/UI/StatCard';
import { Modal } from '../../components/UI/Modal';
import type { Branch, NetworkKpi } from '../../types';

const TYPES = [
  { key: 'own', label: 'Собственный' },
  { key: 'franchise', label: 'Франшиза' },
  { key: 'whitelabel', label: 'White Label' },
  { key: 'partner', label: 'Партнёр' }
];

export function BranchesPage() {
  const { t } = useTranslation();
  const [kpi, setKpi] = useState<NetworkKpi | null>(null);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [showCreate, setShowCreate] = useState(false);

  async function load() {
    const [k, b] = await Promise.all([branchesService.networkKpi(), branchesService.list()]);
    setKpi(k);
    setBranches(b);
  }

  useEffect(() => { load(); }, []);

  async function handleDelete(id: string) {
    if (!confirm('Удалить филиал?')) return;
    await branchesService.remove(id);
    toast.success('Филиал удалён');
    load();
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>{t('branches')}</h2>
        <button className="btn" onClick={() => setShowCreate(true)}>+ Филиал</button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 16 }}>
        <StatCard label="Кол-во филиалов" value={kpi?.branchesCount ?? 0} />
        <StatCard label="Выручка сети" value={(kpi?.totalRevenue ?? 0).toLocaleString()} suffix=" TJS" />
        <StatCard label="Заказов сети" value={kpi?.totalOrders ?? 0} />
        <StatCard label="Средний NPS" value={(kpi?.averageNps ?? 0).toFixed(0)} />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 16 }}>
        {branches.map((b) => (
          <div className="card" key={b.id}>
            <h3 style={{ marginTop: 0 }}>{b.name}</h3>
            <div>{b.address}</div>
            <div>{b.phone}</div>
            <div>Тип: {b.type}</div>
            <div>Заказов: {b.ordersCount} · Выручка: {b.revenue.toLocaleString()} TJS</div>
            <div>NPS: {b.nps}</div>
            <button className="btn btn-secondary" style={{ marginTop: 8 }} onClick={() => handleDelete(b.id)}>Удалить</button>
          </div>
        ))}
      </div>

      {showCreate && (
        <CreateBranchModal onClose={() => setShowCreate(false)} onCreated={() => { setShowCreate(false); load(); }} />
      )}
    </div>
  );
}

function CreateBranchModal({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [phone, setPhone] = useState('+992');
  const [type, setType] = useState('own');
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await branchesService.create({ name, address, phone, type });
      toast.success('Филиал добавлен');
      onCreated();
    } catch {
      setError('Не удалось сохранить');
    }
  }

  return (
    <Modal title="Добавить филиал" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className="field"><label>Название</label><input required value={name} onChange={(e) => setName(e.target.value)} /></div>
        <div className="field"><label>Адрес</label><input required value={address} onChange={(e) => setAddress(e.target.value)} /></div>
        <div className="field"><label>Телефон</label><input value={phone} onChange={(e) => setPhone(e.target.value)} /></div>
        <div className="field"><label>Тип</label>
          <select value={type} onChange={(e) => setType(e.target.value)}>
            {TYPES.map((tp) => <option key={tp.key} value={tp.key}>{tp.label}</option>)}
          </select>
        </div>
        {error && <div className="error-text">{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={onClose}>{t('cancel')}</button>
          <button type="submit" className="btn">{t('save')}</button>
        </div>
      </form>
    </Modal>
  );
}
