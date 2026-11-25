import { useState, type FormEvent } from 'react';
import toast from 'react-hot-toast';
import { apiError, marketingService } from '../../services/features.service';
import { dateRu, useAsync } from '../../lib/useAsync';
import { useAuthStore } from '../../store/authStore';
import { Empty, ErrorBox, Loading, PageHeader } from '../../components/UI/States';
import { StatCard } from '../../components/UI/StatCard';
import { Modal } from '../../components/UI/Modal';

const CHANNELS = [
  { id: 'Telegram', label: 'Telegram' },
  { id: 'WhatsApp', label: 'WhatsApp' },
  { id: 'Sms', label: 'SMS' },
  { id: 'Email', label: 'Email' },
];
const SEGMENTS = [
  { id: 'All', label: 'Все клиенты' },
  { id: 'Vip', label: 'VIP клиенты' },
  { id: 'Dormant', label: 'Спящие (3+ мес)' },
  { id: 'AfterService', label: 'После ТО (30 дней)' },
];
const STATUS = { Queued: ['badge-yellow', 'В очереди'], Sent: ['badge-green', 'Отправлена'], Failed: ['badge-red', 'Ошибка'] } as Record<string, [string, string]>;

export function MarketingPage() {
  const stats = useAsync(() => marketingService.stats());
  const promos = useAsync(() => marketingService.promos());
  const campaigns = useAsync(() => marketingService.campaigns());

  const refreshAll = () => { stats.reload(); promos.reload(); campaigns.reload(); };

  return (
    <div>
      <PageHeader title="МАРКЕТИНГ" sub="Рассылки по сегментам клиентов и промокоды" />
      {stats.data && (
        <div className="stats-grid">
          <StatCard icon="📤" label="Кампаний" value={stats.data.campaigns} />
          <StatCard icon="👥" label="Получателей всего" value={stats.data.totalRecipients.toLocaleString('ru-RU')} />
          <StatCard icon="🎟️" label="Активных промокодов" value={stats.data.activePromos} />
        </div>
      )}
      <div className="grid-2">
        <CampaignForm segmentSizes={stats.data?.segmentSizes} onSent={refreshAll} />
        <PromoList state={promos} onChange={refreshAll} />
      </div>
      <div className="card">
        <div className="card-header"><div className="card-title">ИСТОРИЯ КАМПАНИЙ</div></div>
        {campaigns.error ? <ErrorBox message={campaigns.error} onRetry={campaigns.reload} /> : campaigns.loading ? <Loading /> :
          campaigns.data!.length === 0 ? <Empty icon="📣" text="Кампаний пока нет" /> :
            campaigns.data!.map((c) => {
              const [cls, label] = STATUS[c.status] ?? ['badge-gray', c.status];
              return (
                <div className="metric-row" key={c.id}>
                  <div style={{ minWidth: 0 }}>
                    <div className="fw-bold" style={{ fontSize: 12 }}>{c.name}</div>
                    <div className="text-muted text-sm" style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 520 }}>{c.message}</div>
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <span className={`badge ${cls}`}>{label}</span>
                    <div className="text-muted text-sm">👥 {c.recipientsCount} · {dateRu(c.createdAt)}</div>
                  </div>
                </div>
              );
            })}
      </div>
    </div>
  );
}

function CampaignForm({ segmentSizes, onSent }: { segmentSizes?: Record<string, number>; onSent: () => void }) {
  const [channel, setChannel] = useState('Telegram');
  const [segment, setSegment] = useState('All');
  const [message, setMessage] = useState('');
  const [busy, setBusy] = useState(false);

  async function useTemplate() {
    try {
      const t = await marketingService.template(segment);
      setMessage(t.text);
    } catch (e) {
      toast.error(apiError(e));
    }
  }

  async function submit(e: FormEvent) {
    e.preventDefault();
    if (!message.trim()) { toast.error('Введите текст сообщения'); return; }
    setBusy(true);
    try {
      const c = await marketingService.createCampaign({ channel, segment, message: message.trim() });
      toast.success(`Кампания поставлена в очередь: ${c.recipientsCount} получателей`);
      setMessage('');
      onSent();
    } catch (err) {
      toast.error(apiError(err));
    } finally {
      setBusy(false);
    }
  }

  return (
    <form className="card" onSubmit={submit}>
      <div className="card-header"><div className="card-title">СОЗДАТЬ РАССЫЛКУ</div></div>
      <div className="form-grid">
        <div className="form-group">
          <label className="form-label">Канал</label>
          <select value={channel} onChange={(e) => setChannel(e.target.value)}>
            {CHANNELS.map((c) => <option key={c.id} value={c.id}>{c.label}</option>)}
          </select>
        </div>
        <div className="form-group">
          <label className="form-label">Сегмент</label>
          <select value={segment} onChange={(e) => setSegment(e.target.value)}>
            {SEGMENTS.map((s) => (
              <option key={s.id} value={s.id}>{s.label}{segmentSizes ? ` (${segmentSizes[s.id] ?? 0})` : ''}</option>
            ))}
          </select>
        </div>
      </div>
      <div className="form-group">
        <label className="form-label">Текст сообщения ({message.length}/1000)</label>
        <textarea rows={4} value={message} maxLength={1000} onChange={(e) => setMessage(e.target.value)} placeholder="Уважаемый клиент..." />
      </div>
      <div className="flex-gap">
        <button type="button" className="btn btn-secondary" onClick={useTemplate}>📝 Шаблон для сегмента</button>
        <button className="btn btn-primary" disabled={busy}>📤 Поставить в очередь</button>
      </div>
      <div className="text-muted text-sm mt-8">
        Отправка выполняется через подключённый шлюз (Telegram/SMS). Пока шлюз не настроен, кампании сохраняются со статусом «В очереди».
      </div>
    </form>
  );
}

function PromoList({ state, onChange }: { state: ReturnType<typeof useAsync<Awaited<ReturnType<typeof marketingService.promos>>>>; onChange: () => void }) {
  const isAdmin = useAuthStore((s) => s.user?.role === 'Admin');
  const [open, setOpen] = useState(false);

  async function toggle(id: string, active: boolean) {
    try { await marketingService.setPromoActive(id, active); onChange(); } catch (e) { toast.error(apiError(e)); }
  }
  async function remove(id: string, code: string) {
    if (!confirm(`Удалить промокод ${code}?`)) return;
    try { await marketingService.deletePromo(id); onChange(); toast.success('Удалено'); } catch (e) { toast.error(apiError(e)); }
  }

  return (
    <div className="card">
      <div className="card-header">
        <div className="card-title">ПРОМОКОДЫ</div>
        <button className="btn btn-primary btn-sm" onClick={() => setOpen(true)}>+ Создать</button>
      </div>
      {state.error ? <ErrorBox message={state.error} onRetry={state.reload} /> : state.loading ? <Loading /> :
        state.data!.length === 0 ? <Empty icon="🎟️" text="Промокодов нет" /> :
          state.data!.map((p) => (
            <div className="flex-between" key={p.id} style={{ padding: '10px 0', borderBottom: '1px solid var(--border)', gap: 8 }}>
              <div>
                <div className="fw-bold font-mono" style={{ fontSize: 13, color: p.isActive && !p.isExpired ? 'var(--orange)' : 'var(--text3)' }}>{p.code}</div>
                <div className="text-muted text-sm">
                  Скидка {p.discountPercent}% · использован {p.usedCount}{p.maxUses ? `/${p.maxUses}` : ''} раз · до {dateRu(p.expiresAt)}
                </div>
              </div>
              <div className="flex-gap">
                {p.isExpired ? <span className="badge badge-gray">Истёк</span> : (
                  <button className={`btn btn-sm ${p.isActive ? 'btn-secondary' : 'btn-success'}`} onClick={() => toggle(p.id, !p.isActive)}>
                    {p.isActive ? 'Выключить' : 'Включить'}
                  </button>
                )}
                {isAdmin && <button className="btn btn-sm btn-danger" onClick={() => remove(p.id, p.code)} aria-label="Удалить">🗑</button>}
              </div>
            </div>
          ))}
      {open && <PromoModal onClose={() => setOpen(false)} onCreated={() => { setOpen(false); onChange(); }} />}
    </div>
  );
}

function PromoModal({ onClose, onCreated }: { onClose: () => void; onCreated: () => void }) {
  const defaultDate = new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10);
  const [code, setCode] = useState('');
  const [discount, setDiscount] = useState(15);
  const [expires, setExpires] = useState(defaultDate);
  const [maxUses, setMaxUses] = useState('');
  const [busy, setBusy] = useState(false);

  async function save() {
    setBusy(true);
    try {
      const p = await marketingService.createPromo({
        code: code.trim() || undefined,
        discountPercent: discount,
        expiresAt: expires,
        maxUses: maxUses ? Number(maxUses) : undefined,
      });
      toast.success(`Промокод создан: ${p.code}`);
      onCreated();
    } catch (e) {
      toast.error(apiError(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal
      title="НОВЫЙ ПРОМОКОД"
      onClose={onClose}
      footer={<>
        <button className="btn btn-secondary" onClick={onClose}>Отмена</button>
        <button className="btn btn-primary" onClick={save} disabled={busy}>Создать</button>
      </>}
    >
      <div className="form-group">
        <label className="form-label">Код (пусто — сгенерировать автоматически)</label>
        <input value={code} onChange={(e) => setCode(e.target.value.toUpperCase())} placeholder="NAVRUZ25" maxLength={32} />
      </div>
      <div className="form-grid">
        <div className="form-group">
          <label className="form-label">Скидка, %</label>
          <input type="number" min={1} max={90} value={discount} onChange={(e) => setDiscount(Number(e.target.value))} />
        </div>
        <div className="form-group">
          <label className="form-label">Действует до</label>
          <input type="date" value={expires} min={new Date().toISOString().slice(0, 10)} onChange={(e) => setExpires(e.target.value)} />
        </div>
      </div>
      <div className="form-group">
        <label className="form-label">Лимит использований (необязательно)</label>
        <input type="number" min={1} value={maxUses} onChange={(e) => setMaxUses(e.target.value)} />
      </div>
    </Modal>
  );
}
