import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { aiService, apiError } from '../../services/features.service';
import { dateRu, useAsync } from '../../lib/useAsync';
import type { DiagnosisResult } from '../../types/features';
import { AiPanel, Empty, ErrorBox, Loading, PageHeader, RiskBadge } from '../../components/UI/States';

const URGENCY_BORDER = { high: 'var(--red)', medium: 'var(--yellow)', low: 'var(--green)' } as const;

export function AiPage() {
  return (
    <div>
      <PageHeader title="ПОМОЩНИК ДИАГНОСТИКИ" sub="Подсказки по правилам симптомов, прогноз ТО и статистика поломок. Не заменяет осмотр механика." />
      <div className="grid-2">
        <Diagnosis />
      </div>
      <Predictive />
      <IssuesByMake />
    </div>
  );
}

function Diagnosis() {
  const navigate = useNavigate();
  const [plate, setPlate] = useState('');
  const [problem, setProblem] = useState('');
  const [mileage, setMileage] = useState('');
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState<DiagnosisResult | null>(null);

  async function submit(e: FormEvent) {
    e.preventDefault();
    if (!problem.trim()) { toast.error('Опишите проблему'); return; }
    setBusy(true);
    try {
      setResult(await aiService.diagnose({
        plateOrVin: plate.trim() || undefined,
        problem: problem.trim(),
        mileage: mileage ? Number(mileage) : undefined,
      }));
    } catch (err) {
      toast.error(apiError(err));
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <form className="card" onSubmit={submit}>
        <div className="card-header"><div className="card-title">ДИАГНОСТИКА АВТО</div></div>
        <div className="form-group">
          <label className="form-label">Гос. номер или VIN</label>
          <input value={plate} onChange={(e) => setPlate(e.target.value)} placeholder="1000 AB 01" maxLength={32} />
        </div>
        <div className="form-group">
          <label className="form-label">Опишите проблему *</label>
          <textarea rows={3} value={problem} onChange={(e) => setProblem(e.target.value)} placeholder="Скрип при торможении, вибрация руля..." maxLength={1000} />
        </div>
        <div className="form-group">
          <label className="form-label">Пробег (км)</label>
          <input type="number" min={0} value={mileage} onChange={(e) => setMileage(e.target.value)} placeholder="Подставится из карточки авто" />
        </div>
        <button className="btn btn-primary w-full" disabled={busy}>{busy ? 'Анализ…' : 'Показать возможные причины'}</button>
      </form>

      <div className="card">
        <div className="card-header"><div className="card-title">РЕЗУЛЬТАТ</div></div>
        {!result ? <Empty icon="🤖" text="Введите данные и запустите диагностику" /> : (
          <>
            <AiPanel
              title="ДИАГНОЗ"
              insights={[
                `Авто: ${result.vehicleTitle ?? 'не найдено в базе'} · Пробег: ${result.mileage?.toLocaleString('ru-RU') ?? '?'} км`,
                `Проблема: ${result.problem}`,
              ]}
            />
            {result.hypotheses.map((h) => (
              <div key={h.issue} className="card mb-8" style={{ padding: 12, borderLeft: `3px solid ${URGENCY_BORDER[h.urgency]}` }}>
                <div className="flex-between">
                  <div className="fw-bold" style={{ fontSize: 12 }}>{h.issue}</div>
                  <RiskBadge level={h.urgency} suffix={` · ${h.probability}%`} />
                </div>
                <div className="text-muted mt-8" style={{ fontSize: 11 }}>Рекомендация: {h.recommendation}</div>
                <div className="text-orange fw-bold" style={{ fontSize: 12, marginTop: 4 }}>~{h.costFrom}–{h.costTo} TJS</div>
              </div>
            ))}
            {result.recentServices.length > 0 && (
              <div className="text-muted text-sm mb-8">Последние работы: {result.recentServices.join('; ')}</div>
            )}
            <div className="text-muted text-sm mb-8">ℹ️ {result.disclaimer}</div>
            <button
              className="btn btn-primary w-full"
              onClick={() => navigate(`/orders?new=1&service=${encodeURIComponent(result.hypotheses[0]?.recommendation ?? '')}`)}
            >
              🔩 Создать заказ на ремонт
            </button>
          </>
        )}
      </div>
    </>
  );
}

function Predictive() {
  const { data, loading, error, reload } = useAsync(() => aiService.predictive());
  return (
    <div className="card">
      <div className="card-header">
        <div className="card-title">PREDICTIVE MAINTENANCE · ПРОГНОЗ ТО</div>
        {data && <span className="badge badge-orange">{data.length} авто</span>}
      </div>
      {error ? <ErrorBox message={error} onRetry={reload} /> : loading ? <Loading /> : data!.length === 0
        ? <Empty icon="✅" text="Все автомобили обслужены вовремя" />
        : data!.map((v) => (
          <div className="vehicle-card" key={v.vehicleId}>
            <span className="vehicle-icon">🚗</span>
            <div className="vehicle-info">
              <div className="vehicle-name">{v.title} — <span className="plate">{v.licensePlate}</span></div>
              <div className="vehicle-meta">
                {v.ownerName} · {v.ownerPhone} · {v.mileage.toLocaleString('ru-RU')} км · {v.year} · последний визит: {dateRu(v.lastServiceAt)}
              </div>
              <div style={{ marginTop: 6, display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                <RiskBadge level={v.riskLevel} suffix=" риск" />
                {v.recommendations.map((r) => <span key={r} className="badge badge-gray">{r}</span>)}
              </div>
            </div>
            <a className="btn btn-secondary btn-sm" href={`tel:${v.ownerPhone}`}>📞 Позвонить</a>
          </div>
        ))}
    </div>
  );
}

function IssuesByMake() {
  const { data, loading, error, reload } = useAsync(() => aiService.issuesByMake());
  return (
    <div className="card">
      <div className="card-header"><div className="card-title">ЧАСТЫЕ ПРОБЛЕМЫ ПО МАРКАМ</div></div>
      {error ? <ErrorBox message={error} onRetry={reload} /> : loading ? <Loading /> : data!.length === 0
        ? <Empty text="Пока нет заказов" />
        : (
          <div className="grid-3">
            {data!.map((m) => (
              <div key={m.make} className="card" style={{ padding: 12, marginBottom: 0 }}>
                <div className="flex-between mb-8">
                  <span className="fw-bold">{m.make}</span>
                  <span className="badge badge-blue">{m.orders} заказов</span>
                </div>
                {m.topServices.map((s, i) => <div key={s} className="text-muted text-sm">{i + 1}. {s}</div>)}
              </div>
            ))}
          </div>
        )}
    </div>
  );
}
