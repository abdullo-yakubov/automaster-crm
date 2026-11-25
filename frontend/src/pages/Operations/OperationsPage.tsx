import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { operationsService } from '../../services/operations.service';
import type { ChecklistItem, SensorReading, SlaViolation, MysteryAudit, AuditLog } from '../../types';

const TABS = ['checklists', 'iot', 'sla', 'mystery', 'audit'] as const;
type Tab = typeof TABS[number];
const TAB_LABELS: Record<Tab, string> = {
  checklists: 'Чек-листы', iot: 'IoT Сенсоры', sla: 'SLA Нарушения', mystery: 'Аудит качества', audit: 'Аудит-лог'
};

export function OperationsPage() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<Tab>('checklists');

  return (
    <div>
      <h2>{t('operations')}</h2>
      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        {TABS.map((tb) => (
          <button key={tb} className={`chip ${tab === tb ? 'active' : ''}`} onClick={() => setTab(tb)}>{TAB_LABELS[tb]}</button>
        ))}
      </div>

      {tab === 'checklists' && <ChecklistsTab />}
      {tab === 'iot' && <IotTab />}
      {tab === 'sla' && <SlaTab />}
      {tab === 'mystery' && <MysteryTab />}
      {tab === 'audit' && <AuditTab />}
    </div>
  );
}

function ChecklistsTab() {
  const [open, setOpen] = useState<ChecklistItem[]>([]);
  const [close, setClose] = useState<ChecklistItem[]>([]);

  async function load() {
    setOpen(await operationsService.checklist('open'));
    setClose(await operationsService.checklist('close'));
  }

  useEffect(() => { load(); }, []);

  async function toggle(type: string, id: string) {
    await operationsService.toggleItem(type, id);
    load();
  }

  function List({ title, type, items }: { title: string; type: string; items: ChecklistItem[] }) {
    const done = items.filter((i) => i.isDone).length;
    return (
      <div className="card">
        <h3 style={{ marginTop: 0 }}>{title} ({done}/{items.length})</h3>
        {items.map((i) => (
          <label key={i.id} style={{ display: 'block', marginBottom: 6 }}>
            <input type="checkbox" checked={i.isDone} onChange={() => toggle(type, i.id)} /> {i.title}
          </label>
        ))}
      </div>
    );
  }

  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 16 }}>
      <List title="Открытие сервиса" type="open" items={open} />
      <List title="Закрытие сервиса" type="close" items={close} />
    </div>
  );
}

function IotTab() {
  const [sensors, setSensors] = useState<SensorReading[]>([]);

  function load() {
    operationsService.sensors().then(setSensors);
  }

  useEffect(() => { load(); }, []);

  return (
    <div>
      <button className="btn btn-secondary" onClick={load} style={{ marginBottom: 12 }}>Обновить</button>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16 }}>
        {sensors.map((s) => (
          <div className="card" key={s.name}>
            <div className="label">{s.name}</div>
            <div className="value">{s.value} {s.unit}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

function SlaTab() {
  const [violations, setViolations] = useState<SlaViolation[]>([]);

  useEffect(() => { operationsService.slaViolations().then(setViolations); }, []);

  return (
    <div className="card">
      <table>
        <thead><tr><th>Заказ</th><th>Клиент</th><th>Статус</th><th>SLA (ч)</th><th>Прошло (ч)</th><th>Превышение (ч)</th></tr></thead>
        <tbody>
          {violations.map((v) => (
            <tr key={v.orderId}>
              <td>{v.orderNumber}</td>
              <td>{v.customerName}</td>
              <td>{v.status}</td>
              <td>{v.slaHours}</td>
              <td>{v.elapsedHours}</td>
              <td style={{ color: '#ef4444' }}>+{v.overHours}</td>
            </tr>
          ))}
          {violations.length === 0 && <tr><td colSpan={6}>Нарушений нет</td></tr>}
        </tbody>
      </table>
    </div>
  );
}

const CRITERIA: { key: keyof import('../../types').MysteryAuditScore; label: string }[] = [
  { key: 'greeting', label: 'Приветствие клиента' },
  { key: 'speed', label: 'Скорость обслуживания' },
  { key: 'cleanliness', label: 'Чистота боксов' },
  { key: 'politeness', label: 'Вежливость мастера' },
  { key: 'repairQuality', label: 'Качество ремонта' },
  { key: 'billAccuracy', label: 'Правильность счёта' },
  { key: 'handoverTime', label: 'Время выдачи авто' }
];

function MysteryTab() {
  const [scores, setScores] = useState({ greeting: 5, speed: 5, cleanliness: 5, politeness: 5, repairQuality: 5, billAccuracy: 5, handoverTime: 5 });
  const [history, setHistory] = useState<MysteryAudit[]>([]);

  function load() {
    operationsService.mysteryAudits().then(setHistory);
  }

  useEffect(() => { load(); }, []);

  async function save() {
    await operationsService.saveMysteryAudit(scores);
    toast.success('Аудит сохранён');
    load();
  }

  const total = (Object.values(scores).reduce((a, b) => a + b, 0) / 7).toFixed(2);

  return (
    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
      <div className="card">
        <h3 style={{ marginTop: 0 }}>Mystery Shopper</h3>
        {CRITERIA.map((c) => (
          <div className="field" key={c.key}>
            <label>{c.label}</label>
            <select value={scores[c.key]} onChange={(e) => setScores({ ...scores, [c.key]: +e.target.value })}>
              {[1, 2, 3, 4, 5].map((v) => <option key={v} value={v}>{'⭐'.repeat(v)}</option>)}
            </select>
          </div>
        ))}
        <div style={{ margin: '8px 0' }}>Итоговый балл: <b>{total}</b></div>
        <button className="btn" onClick={save}>Сохранить аудит</button>
      </div>
      <div className="card">
        <h3 style={{ marginTop: 0 }}>История аудитов</h3>
        <table>
          <thead><tr><th>Дата</th><th>Балл</th></tr></thead>
          <tbody>
            {history.map((h) => (
              <tr key={h.id}><td>{new Date(h.createdAt).toLocaleString()}</td><td>{h.totalScore}</td></tr>
            ))}
            {history.length === 0 && <tr><td colSpan={2}>Нет аудитов</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function AuditTab() {
  const [logs, setLogs] = useState<AuditLog[]>([]);

  useEffect(() => { operationsService.auditLogs().then(setLogs); }, []);

  return (
    <div className="card">
      <table>
        <thead><tr><th>Время</th><th>Пользователь</th><th>Действие</th><th>Объект</th><th>Детали</th></tr></thead>
        <tbody>
          {logs.map((l) => (
            <tr key={l.id}>
              <td>{new Date(l.createdAt).toLocaleString()}</td>
              <td>{l.userName}</td>
              <td>{l.action}</td>
              <td>{l.objectType} #{l.objectId}</td>
              <td>{l.details}</td>
            </tr>
          ))}
          {logs.length === 0 && <tr><td colSpan={5}>Лог пуст</td></tr>}
        </tbody>
      </table>
    </div>
  );
}
