import { STATUS_COLORS, STATUS_LABELS_RU } from '../../types';

export function StatusBadge({ status }: { status: string }) {
  return (
    <span className="badge" style={{ background: STATUS_COLORS[status] || '#666' }}>
      {STATUS_LABELS_RU[status] || status}
    </span>
  );
}

export function LicensePlate({ value }: { value: string }) {
  return <span className="license-plate">{value}</span>;
}
