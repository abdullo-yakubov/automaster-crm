import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { vehiclesService } from '../../services/vehicles.service';
import { Modal } from '../../components/UI/Modal';
import { LicensePlate } from '../../components/UI/Badge';
import type { Vehicle } from '../../types';

export function VehiclesPage() {
  const { t } = useTranslation();
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);

  async function load() {
    const res = await vehiclesService.list({ search: search || undefined });
    setVehicles(res.items);
  }

  useEffect(() => {
    load();
  }, [search]);

  async function handleDelete(id: string) {
    if (!confirm('Удалить автомобиль?')) return;
    await vehiclesService.remove(id);
    toast.success('Удалено');
    load();
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>{t('vehicles')}</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          <input placeholder={t('search')} value={search} onChange={(e) => setSearch(e.target.value)} style={{ width: 220 }} />
          <button className="btn" onClick={() => setShowCreate(true)}>+ Авто</button>
        </div>
      </div>

      <div className="card">
        <table>
          <thead>
            <tr>
              <th>Госномер</th><th>Марка/модель</th><th>Год</th><th>Владелец</th><th>Пробег</th><th>{t('actions')}</th>
            </tr>
          </thead>
          <tbody>
            {vehicles.map((v) => (
              <tr key={v.id}>
                <td><LicensePlate value={v.licensePlate} /></td>
                <td>{v.make} {v.model}</td>
                <td>{v.year}</td>
                <td>{v.customerName}</td>
                <td>{v.mileage.toLocaleString()} км</td>
                <td><button className="btn btn-secondary" onClick={() => handleDelete(v.id)}>✕</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showCreate && (
        <CreateVehicleModal onClose={() => setShowCreate(false)} onCreated={() => { setShowCreate(false); load(); }} />
      )}
    </div>
  );
}

function CreateVehicleModal({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const { t } = useTranslation();
  const [form, setForm] = useState({ ownerName: '', make: '', model: '', year: 2020, licensePlate: '', mileage: 0 });
  const [error, setError] = useState('');

  function set<K extends keyof typeof form>(key: K, value: typeof form[K]) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await vehiclesService.create(form);
      toast.success('Автомобиль создан');
      onCreated();
    } catch {
      setError('Не удалось создать автомобиль');
    }
  }

  return (
    <Modal title="Новый автомобиль" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className="field"><label>Владелец</label><input required value={form.ownerName} onChange={(e) => set('ownerName', e.target.value)} /></div>
        <div className="field"><label>Марка</label><input required value={form.make} onChange={(e) => set('make', e.target.value)} /></div>
        <div className="field"><label>Модель</label><input required value={form.model} onChange={(e) => set('model', e.target.value)} /></div>
        <div className="field"><label>Год</label><input type="number" value={form.year} onChange={(e) => set('year', Number(e.target.value))} /></div>
        <div className="field"><label>Госномер</label><input required value={form.licensePlate} onChange={(e) => set('licensePlate', e.target.value)} /></div>
        <div className="field"><label>Пробег</label><input type="number" value={form.mileage} onChange={(e) => set('mileage', Number(e.target.value))} /></div>
        {error && <div className="error-text">{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={onClose}>{t('cancel')}</button>
          <button type="submit" className="btn">{t('save')}</button>
        </div>
      </form>
    </Modal>
  );
}
