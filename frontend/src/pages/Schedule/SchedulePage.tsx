import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { scheduleService } from '../../services/schedule.service';
import { Modal } from '../../components/UI/Modal';
import type { Booking, Slot } from '../../types';

function today() {
  return new Date().toISOString().slice(0, 10);
}

export function SchedulePage() {
  const { t } = useTranslation();
  const [date, setDate] = useState(today());
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [slots, setSlots] = useState<Slot[]>([]);
  const [showCreate, setShowCreate] = useState(false);

  async function load() {
    const [b, s] = await Promise.all([scheduleService.bookings(date), scheduleService.slots(date)]);
    setBookings(b);
    setSlots(s);
  }

  useEffect(() => {
    load();
  }, [date]);

  async function handleDelete(id: string) {
    if (!confirm('Отменить запись?')) return;
    await scheduleService.remove(id);
    toast.success('Запись отменена');
    load();
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>{t('schedule')}</h2>
        <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1.4fr 1fr', gap: 16 }}>
        <div className="card">
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <h3 style={{ marginTop: 0 }}>Запись на {date}</h3>
            <button className="btn" onClick={() => setShowCreate(true)}>+ Записать</button>
          </div>
          <table>
            <thead><tr><th>Клиент</th><th>Телефон</th><th>Время</th><th>Услуга</th><th>Статус</th><th></th></tr></thead>
            <tbody>
              {bookings.map((b) => (
                <tr key={b.id}>
                  <td>{b.customerName}</td>
                  <td>{b.phone}</td>
                  <td>{b.time}</td>
                  <td>{b.service}</td>
                  <td>{b.status}</td>
                  <td><button className="btn btn-secondary" onClick={() => handleDelete(b.id)}>✕</button></td>
                </tr>
              ))}
              {bookings.length === 0 && <tr><td colSpan={6}>Записей нет</td></tr>}
            </tbody>
          </table>
        </div>

        <div className="card">
          <h3 style={{ marginTop: 0 }}>Слоты</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 8 }}>
            {slots.map((s) => (
              <div key={s.time} style={{
                padding: 8, textAlign: 'center', borderRadius: 6,
                background: s.isFree ? 'rgba(34,197,94,0.15)' : 'rgba(239,68,68,0.15)',
                color: s.isFree ? '#22c55e' : '#ef4444', fontWeight: 600
              }}>
                {s.time}
              </div>
            ))}
          </div>
        </div>
      </div>

      {showCreate && (
        <CreateBookingModal date={date} onClose={() => setShowCreate(false)} onCreated={() => { setShowCreate(false); load(); }} />
      )}
    </div>
  );
}

function CreateBookingModal({ date, onClose, onCreated }: { date: string; onClose: () => void; onCreated: () => void }) {
  const { t } = useTranslation();
  const [customerName, setCustomerName] = useState('');
  const [phone, setPhone] = useState('+992');
  const [time, setTime] = useState('08:00');
  const [service, setService] = useState('');
  const [error, setError] = useState('');

  const times = Array.from({ length: 11 }, (_, i) => `${(8 + i).toString().padStart(2, '0')}:00`);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await scheduleService.create({ customerName, phone, date, time, service });
      toast.success('Запись создана');
      onCreated();
    } catch {
      setError('Не удалось создать запись');
    }
  }

  return (
    <Modal title="Онлайн запись" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className="field"><label>Клиент</label><input required value={customerName} onChange={(e) => setCustomerName(e.target.value)} /></div>
        <div className="field"><label>Телефон</label><input required value={phone} onChange={(e) => setPhone(e.target.value)} /></div>
        <div className="field"><label>Время</label>
          <select value={time} onChange={(e) => setTime(e.target.value)}>
            {times.map((tm) => <option key={tm} value={tm}>{tm}</option>)}
          </select>
        </div>
        <div className="field"><label>Услуга</label><input required value={service} onChange={(e) => setService(e.target.value)} /></div>
        {error && <div className="error-text">{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={onClose}>{t('cancel')}</button>
          <button type="submit" className="btn">{t('save')}</button>
        </div>
      </form>
    </Modal>
  );
}
