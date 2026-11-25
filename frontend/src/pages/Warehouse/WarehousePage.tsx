import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { warehouseService } from '../../services/warehouse.service';
import { StatCard } from '../../components/UI/StatCard';
import { Modal } from '../../components/UI/Modal';
import type { Part, WarehouseStats } from '../../types';

const CATEGORIES = ['filters', 'oils', 'brakes', 'suspension', 'engine', 'electrical', 'body', 'consumables'];
const FILTERS = [
  { key: 'all', label: 'Все' },
  { key: 'low', label: 'Мало' },
  { key: 'ok', label: 'ОК' }
];

export function WarehousePage() {
  const { t } = useTranslation();
  const [parts, setParts] = useState<Part[]>([]);
  const [stats, setStats] = useState<WarehouseStats | null>(null);
  const [filter, setFilter] = useState('all');
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);

  async function load() {
    const [p, s] = await Promise.all([
      warehouseService.list({ filter: filter === 'all' ? undefined : filter, search: search || undefined }),
      warehouseService.stats()
    ]);
    setParts(p);
    setStats(s);
  }

  useEffect(() => {
    load();
  }, [filter, search]);

  async function handleDelete(id: string) {
    if (!confirm('Удалить запчасть?')) return;
    await warehouseService.remove(id);
    toast.success('Запчасть удалена');
    load();
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>{t('warehouse')}</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          <input placeholder={t('search')} value={search} onChange={(e) => setSearch(e.target.value)} style={{ width: 220 }} />
          <button className="btn" onClick={() => setShowCreate(true)}>+ Запчасть</button>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 16 }}>
        <StatCard label="Всего позиций" value={stats?.totalItems ?? 0} />
        <StatCard label="Низкий остаток" value={stats?.lowStockCount ?? 0} />
        <StatCard label="Стоимость склада" value={(stats?.totalValue ?? 0).toLocaleString()} suffix=" TJS" />
        <StatCard label="Оборот за месяц" value={(stats?.monthlyTurnover ?? 0).toLocaleString()} suffix=" TJS" />
      </div>

      <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
        {FILTERS.map((f) => (
          <button key={f.key} className={`chip ${filter === f.key ? 'active' : ''}`} onClick={() => setFilter(f.key)}>{f.label}</button>
        ))}
      </div>

      <div className="card">
        <table>
          <thead>
            <tr>
              <th>Название</th><th>Артикул</th><th>Категория</th><th>Остаток</th><th>Мин.</th>
              <th>Закупка</th><th>Продажа</th><th>Маржа</th><th>Статус</th><th>{t('actions')}</th>
            </tr>
          </thead>
          <tbody>
            {parts.map((p) => (
              <tr key={p.id}>
                <td>{p.name}</td>
                <td>{p.sku}</td>
                <td>{p.category}</td>
                <td>{p.quantity}</td>
                <td>{p.minQuantity}</td>
                <td>{p.purchasePrice.toLocaleString()} TJS</td>
                <td>{p.salePrice.toLocaleString()} TJS</td>
                <td>{p.marginPercent.toFixed(0)}%</td>
                <td>{p.status}</td>
                <td><button className="btn btn-secondary" onClick={() => handleDelete(p.id)}>✕</button></td>
              </tr>
            ))}
            {parts.length === 0 && <tr><td colSpan={10}>Нет позиций</td></tr>}
          </tbody>
        </table>
      </div>

      {showCreate && (
        <CreatePartModal onClose={() => setShowCreate(false)} onCreated={() => { setShowCreate(false); load(); }} />
      )}
    </div>
  );
}

function CreatePartModal({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const { t } = useTranslation();
  const [name, setName] = useState('');
  const [sku, setSku] = useState('');
  const [category, setCategory] = useState(CATEGORIES[0]);
  const [quantity, setQuantity] = useState(0);
  const [minQuantity, setMinQuantity] = useState(0);
  const [purchasePrice, setPurchasePrice] = useState(0);
  const [salePrice, setSalePrice] = useState(0);
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await warehouseService.create({ name, sku, category, quantity, minQuantity, purchasePrice, salePrice });
      toast.success('Запчасть добавлена');
      onCreated();
    } catch {
      setError('Не удалось сохранить');
    }
  }

  return (
    <Modal title="Добавить запчасть" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className="field"><label>Название</label><input required value={name} onChange={(e) => setName(e.target.value)} /></div>
        <div className="field"><label>Артикул</label><input value={sku} onChange={(e) => setSku(e.target.value)} /></div>
        <div className="field"><label>Категория</label>
          <select value={category} onChange={(e) => setCategory(e.target.value)}>
            {CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>
        <div className="field"><label>Остаток</label><input type="number" min={0} value={quantity} onChange={(e) => setQuantity(+e.target.value)} /></div>
        <div className="field"><label>Минимальный остаток</label><input type="number" min={0} value={minQuantity} onChange={(e) => setMinQuantity(+e.target.value)} /></div>
        <div className="field"><label>Цена закупки</label><input type="number" min={0} value={purchasePrice} onChange={(e) => setPurchasePrice(+e.target.value)} /></div>
        <div className="field"><label>Цена продажи</label><input type="number" min={0} value={salePrice} onChange={(e) => setSalePrice(+e.target.value)} /></div>
        {error && <div className="error-text">{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={onClose}>{t('cancel')}</button>
          <button type="submit" className="btn">{t('save')}</button>
        </div>
      </form>
    </Modal>
  );
}
