import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { customersService } from '../../services/customers.service';
import { Modal } from '../../components/UI/Modal';
import type { Customer } from '../../types';

export function CustomersPage() {
  const { t } = useTranslation();
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);

  async function load() {
    const res = await customersService.list({ search: search || undefined });
    setCustomers(res.items);
  }

  useEffect(() => {
    load();
  }, [search]);

  async function handleDelete(id: string) {
    if (!confirm('Удалить клиента?')) return;
    await customersService.remove(id);
    toast.success('Клиент удалён');
    load();
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>{t('customers')}</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          <input placeholder={t('search')} value={search} onChange={(e) => setSearch(e.target.value)} style={{ width: 220 }} />
          <button className="btn" onClick={() => setShowCreate(true)}>+ Клиент</button>
        </div>
      </div>

      <div className="card">
        <table>
          <thead>
            <tr>
              <th>Имя</th><th>Телефон</th><th>Статус</th><th>Баллы</th><th>Потрачено</th><th>Авто</th><th>{t('actions')}</th>
            </tr>
          </thead>
          <tbody>
            {customers.map((c) => (
              <tr key={c.id}>
                <td>{c.fullName}</td>
                <td>{c.phone}</td>
                <td>{c.status}</td>
                <td>{c.loyaltyPoints}</td>
                <td>{c.totalSpent.toLocaleString()} TJS</td>
                <td>{c.vehicleCount}</td>
                <td><button className="btn btn-secondary" onClick={() => handleDelete(c.id)}>✕</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showCreate && (
        <CreateCustomerModal onClose={() => setShowCreate(false)} onCreated={() => { setShowCreate(false); load(); }} />
      )}
    </div>
  );
}

function CreateCustomerModal({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const { t } = useTranslation();
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('+992');
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await customersService.create({ fullName, phone, email: email || undefined });
      toast.success('Клиент создан');
      onCreated();
    } catch {
      setError('Не удалось создать клиента');
    }
  }

  return (
    <Modal title="Новый клиент" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className="field"><label>Имя</label><input required value={fullName} onChange={(e) => setFullName(e.target.value)} /></div>
        <div className="field"><label>Телефон</label><input required value={phone} onChange={(e) => setPhone(e.target.value)} /></div>
        <div className="field"><label>Email</label><input value={email} onChange={(e) => setEmail(e.target.value)} /></div>
        {error && <div className="error-text">{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={onClose}>{t('cancel')}</button>
          <button type="submit" className="btn">{t('save')}</button>
        </div>
      </form>
    </Modal>
  );
}
