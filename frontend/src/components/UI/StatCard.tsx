interface StatCardProps {
  label: string;
  value: string | number;
  suffix?: string;
  icon?: string;
  change?: { text: string; up: boolean };
}

export function StatCard({ label, value, suffix, icon, change }: StatCardProps) {
  return (
    <div className="stat-card">
      {icon && <div className="stat-icon">{icon}</div>}
      <div className="stat-value">
        {value}
        {suffix && <span style={{ fontSize: '0.55em', marginLeft: 4 }}>{suffix}</span>}
      </div>
      <div className="stat-label">{label}</div>
      {change && <span className={`stat-change ${change.up ? 'up' : 'down'}`}>{change.text}</span>}
    </div>
  );
}
