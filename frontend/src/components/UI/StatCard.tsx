export function StatCard({ label, value, suffix }: { label: string; value: string | number; suffix?: string }) {
  return (
    <div className="card stat-card">
      <div className="label">{label}</div>
      <div className="value">{value}{suffix}</div>
    </div>
  );
}
