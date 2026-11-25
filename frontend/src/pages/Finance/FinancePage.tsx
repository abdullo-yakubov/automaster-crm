import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { financeService } from '../../services/finance.service';
import { StatCard } from '../../components/UI/StatCard';
import { Modal } from '../../components/UI/Modal';
import type { FinanceStats, Transaction } from '../../types';

const CATEGORIES = ['Ремонт', 'Запчасти', 'Аренда', 'Зарплата', 'Маркетинг', 'Оборудование', 'Прочее'];

export function FinancePage() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<FinanceStats | null>(null);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [showCreate, setShowCreate] = useState(false);

  async function load() {
    const [s, tx] = await Promise.all([financeService.stats(), financeService.transactions()]);
    setStats(s);
    setTransactions(tx);
  }

  useEffect(() => {
    load();
  }, []);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 16 }}>
        <h2>{t('finance')}</h2>
        <button className="btn" onClick={() => setShowCreate(true)}>+ Транзакция</button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 16 }}>
        <StatCard label="Выручка за месяц" value={(stats?.revenueThisMonth ?? 0).toLocaleString()} suffix=" TJS" />
        <StatCard label="Расходы за месяц" value={(stats?.expensesThisMonth ?? 0).toLocaleString()} suffix=" TJS" />
        <StatCard label="Прибыль" value={(stats?.profit ?? 0).toLocaleString()} suffix=" TJS" />
        <StatCard label="Маржа" value={(stats?.marginPercent ?? 0).toFixed(1)} suffix="%" />
      </div>

      <div className="card">
        <h3 style={{ marginTop: 0 }}>Транзакции</h3>
        <table>
          <thead><tr><th>Дата</th><th>Описание</th><th>Тип</th><th>Категория</th><th>Сумма</th></tr></thead>
          <tbody>
            {transactions.map((tx) => (
              <tr key={tx.id}>
                <td>{tx.date}</td>
                <td>{tx.description}</td>
                <td>{tx.type === 'Income' ? 'Доход' : 'Расход'}</td>
                <td>{tx.category}</td>
                <td style={{ color: tx.type === 'Income' ? '#22c55e' : '#ef4444' }}>
                  {tx.type === 'Income' ? '+' : '-'}{tx.amount.toLocaleString()} TJS
                </td>
              </tr>
            ))}
            {transactions.length === 0 && <tr><td colSpan={5}>Нет транзакций</td></tr>}
          </tbody>
        </table>
      </div>

      {showCreate && (
        <CreateTransactionModal onClose={() => setShowCreate(false)} onCreated={() => { setShowCreate(false); load(); }} />
      )}
    </div>
  );
}

function CreateTransactionModal({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const { t } = useTranslation();
  const [type, setType] = useState('Income');
  const [amount, setAmount] = useState(0);
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState(CATEGORIES[0]);
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    try {
      await financeService.create({ type, amount, description, category, date });
      toast.success('Транзакция добавлена');
      onCreated();
    } catch {
      setError('Не удалось сохранить');
    }
  }

  return (
    <Modal title="Транзакция" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <div className="field"><label>Тип</label>
          <select value={type} onChange={(e) => setType(e.target.value)}>
            <option value="Income">Доход</option>
            <option value="Expense">Расход</option>
          </select>
        </div>
        <div className="field"><label>Сумма</label><input type="number" min={0} required value={amount} onChange={(e) => setAmount(+e.target.value)} /></div>
        <div className="field"><label>Описание</label><input required value={description} onChange={(e) => setDescription(e.target.value)} /></div>
        <div className="field"><label>Категория</label>
          <select value={category} onChange={(e) => setCategory(e.target.value)}>
            {CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>
        <div className="field"><label>Дата</label><input type="date" required value={date} onChange={(e) => setDate(e.target.value)} /></div>
        {error && <div className="error-text">{error}</div>}
        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end', marginTop: 12 }}>
          <button type="button" className="btn btn-secondary" onClick={onClose}>{t('cancel')}</button>
          <button type="submit" className="btn">{t('save')}</button>
        </div>
      </form>
    </Modal>
  );
}
