import type { ReactNode } from 'react';

export function PageHeader({ title, sub, actions }: { title: string; sub?: string; actions?: ReactNode }) {
  return (
    <div className="flex-between mb-16" style={{ flexWrap: 'wrap', gap: 12 }}>
      <div>
        <div className="section-title">{title}</div>
        {sub && <div className="section-sub">{sub}</div>}
      </div>
      {actions}
    </div>
  );
}

export function Empty({ icon = '📭', text }: { icon?: string; text: string }) {
  return (
    <div className="empty-state">
      <div className="empty-icon">{icon}</div>
      <div className="empty-text">{text}</div>
    </div>
  );
}

export function ErrorBox({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className="card" style={{ borderColor: 'var(--red)' }}>
      <div className="flex-between">
        <span className="text-red">⚠️ {message}</span>
        {onRetry && <button className="btn btn-secondary btn-sm" onClick={onRetry}>Повторить</button>}
      </div>
    </div>
  );
}

export function Loading() {
  return <div className="skeleton" aria-busy="true" />;
}

export function AiPanel({ title, insights }: { title: string; insights: string[] }) {
  return (
    <div className="ai-panel">
      <div className="ai-header"><div className="ai-dot" /><div className="ai-title">{title}</div></div>
      <div className="ai-insights">
        {insights.map((i) => <div className="ai-insight" key={i}>{i}</div>)}
      </div>
    </div>
  );
}

export function Tabs<T extends string>({ tabs, active, onChange }: { tabs: { id: T; label: string }[]; active: T; onChange: (id: T) => void }) {
  return (
    <div className="tabs" role="tablist">
      {tabs.map((t) => (
        <button key={t.id} role="tab" aria-selected={active === t.id} className={`tab-btn${active === t.id ? ' active' : ''}`} onClick={() => onChange(t.id)}>
          {t.label}
        </button>
      ))}
    </div>
  );
}

const RISK = { high: ['badge-red', 'Высокий'], medium: ['badge-yellow', 'Средний'], low: ['badge-green', 'Низкий'] } as const;
export function RiskBadge({ level, suffix = '' }: { level: 'high' | 'medium' | 'low'; suffix?: string }) {
  const [cls, label] = RISK[level];
  return <span className={`badge ${cls}`}>{label}{suffix}</span>;
}
