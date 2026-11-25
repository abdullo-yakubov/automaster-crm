import { useState } from 'react';
import { Bar, Line } from 'react-chartjs-2';
import { axisOptions, CHART } from '../../lib/charts';
import { money, useAsync } from '../../lib/useAsync';
import { analyticsService } from '../../services/features.service';
import { AiPanel, Empty, ErrorBox, Loading, PageHeader, Tabs } from '../../components/UI/States';

type Tab = 'overview' | 'kpi' | 'heatmap' | 'forecast';

export function AnalyticsPage() {
  const [tab, setTab] = useState<Tab>('overview');

  return (
    <div>
      <PageHeader title="АНАЛИТИКА & BI" sub="Глубокий анализ бизнеса на основе реальных заказов" />
      <Tabs<Tab>
        active={tab}
        onChange={setTab}
        tabs={[
          { id: 'overview', label: 'Обзор' },
          { id: 'kpi', label: 'KPI мастеров' },
          { id: 'heatmap', label: 'Тепловая карта' },
          { id: 'forecast', label: 'Прогноз' },
        ]}
      />
      {tab === 'overview' && <Overview />}
      {tab === 'kpi' && <KpiTable />}
      {tab === 'heatmap' && <HeatmapView />}
      {tab === 'forecast' && <ForecastView />}
    </div>
  );
}

function Overview() {
  const services = useAsync(() => analyticsService.popularServices(8));
  const mechanics = useAsync(() => analyticsService.mechanics());

  return (
    <div className="chart-grid-2">
      <div className="card">
        <div className="card-header"><div className="card-title">ПОПУЛЯРНЫЕ УСЛУГИ</div></div>
        {services.error ? <ErrorBox message={services.error} onRetry={services.reload} /> : services.loading ? <Loading /> : (
          <div className="chart-container">
            <Bar
              data={{
                labels: services.data!.map((s) => s.service),
                datasets: [{ label: 'Заказов', data: services.data!.map((s) => s.count), backgroundColor: CHART.primary, borderRadius: 4 }],
              }}
              options={{ indexAxis: 'y', plugins: { legend: { display: false } }, scales: axisOptions }}
            />
          </div>
        )}
      </div>
      <div className="card">
        <div className="card-header"><div className="card-title">ВЫРУЧКА ПО МАСТЕРАМ</div></div>
        {mechanics.error ? <ErrorBox message={mechanics.error} onRetry={mechanics.reload} /> : mechanics.loading ? <Loading /> : (
          <div className="chart-container">
            <Bar
              data={{
                labels: mechanics.data!.map((m) => m.fullName.split(' ')[0]),
                datasets: [{ label: 'Выручка, TJS', data: mechanics.data!.map((m) => m.revenue), backgroundColor: CHART.series, borderRadius: 4 }],
              }}
              options={{ plugins: { legend: { display: false } }, scales: axisOptions }}
            />
          </div>
        )}
      </div>
    </div>
  );
}

function KpiTable() {
  const { data, loading, error, reload } = useAsync(() => analyticsService.mechanics());
  if (error) return <ErrorBox message={error} onRetry={reload} />;
  if (loading) return <Loading />;
  if (!data!.length) return <Empty icon="👨‍🔧" text="Нет активных мастеров" />;

  return (
    <div className="card" style={{ padding: 0 }}>
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr><th>Мастер</th><th>Уровень</th><th>Заказов</th><th>Завершено</th><th>Выручка</th><th>Рейтинг</th><th>Оклад</th><th>Бонус (3%)</th></tr>
          </thead>
          <tbody>
            {data!.map((m) => (
              <tr key={m.mechanicId}>
                <td><div className="flex-gap"><div className="avatar avatar-sm">{m.fullName[0]}</div>{m.fullName}</div></td>
                <td><span className="badge badge-orange">{m.level}</span></td>
                <td className="fw-bold text-orange">{m.totalOrders}</td>
                <td>{m.completedOrders}</td>
                <td className="fw-bold font-mono">{money(m.revenue)}</td>
                <td>{'⭐'.repeat(Math.round(m.rating))} {m.rating}</td>
                <td className="font-mono">{money(m.salary)}</td>
                <td className="fw-bold text-green font-mono">+{money(m.bonus)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function HeatmapView() {
  const [days, setDays] = useState(90);
  const { data, loading, error, reload } = useAsync(() => analyticsService.heatmap(days), [days]);

  return (
    <div className="card">
      <div className="card-header">
        <div className="card-title">ЗАГРУЗКА ПО ДНЯМ / ЧАСАМ</div>
        <select value={days} onChange={(e) => setDays(Number(e.target.value))} style={{ width: 'auto' }}>
          <option value={30}>30 дней</option>
          <option value={90}>90 дней</option>
          <option value={180}>180 дней</option>
        </select>
      </div>
      {error ? <ErrorBox message={error} onRetry={reload} /> : loading ? <Loading /> : (
        <>
          <div style={{ overflowX: 'auto', padding: '12px 0' }}>
            <div style={{ display: 'grid', gridTemplateColumns: `40px repeat(${data!.hours.length}, minmax(36px, 1fr))`, gap: 3, fontSize: 10 }}>
              <div />
              {data!.hours.map((h) => <div key={h} className="center text-muted">{h}:00</div>)}
              {data!.days.map((day, di) => (
                <HeatRow key={day} day={day} values={data!.values[di]} hours={data!.hours} max={data!.max} />
              ))}
            </div>
          </div>
          <div className="text-muted center text-sm">Количество принятых заказов (время Душанбе, UTC+5)</div>
        </>
      )}
    </div>
  );
}

function HeatRow({ day, values, hours, max }: { day: string; values: number[]; hours: number[]; max: number }) {
  return (
    <>
      <div className="fw-bold text-muted" style={{ display: 'flex', alignItems: 'center', textTransform: 'capitalize' }}>{day}</div>
      {values.map((v, hi) => {
        const ratio = max > 0 ? v / max : 0;
        return (
          <div
            key={hours[hi]}
            className="heat-cell"
            title={`${day} ${hours[hi]}:00 — ${v} заказов`}
            style={{ background: `rgba(45,125,210,${(ratio * 0.85 + 0.06).toFixed(2)})`, color: ratio > 0.55 ? '#fff' : 'var(--text3)' }}
          >
            {v}
          </div>
        );
      })}
    </>
  );
}

function ForecastView() {
  const { data, loading, error, reload } = useAsync(() => analyticsService.forecast());
  if (error) return <ErrorBox message={error} onRetry={reload} />;
  if (loading) return <Loading />;
  const f = data!;
  const labels = [...f.history.map((h) => h.month), ...f.forecast.map((p) => p.month)];
  const pad = (n: number) => Array<number | null>(n).fill(null);
  const lastActual = f.history[f.history.length - 1]?.revenue ?? null;

  return (
    <div className="stack">
      <AiPanel title="ПРОГНОЗ ВЫРУЧКИ" insights={f.insights} />
      <div className="stats-grid">
        <div className="stat-card"><div className="stat-icon">📅</div><div className="stat-value">{money(f.nextMonth)}</div><div className="stat-label">Следующий месяц</div></div>
        <div className="stat-card"><div className="stat-icon">📈</div><div className={`stat-value ${f.monthlyGrowthPercent >= 0 ? 'text-green' : 'text-red'}`}>{f.monthlyGrowthPercent > 0 ? '+' : ''}{f.monthlyGrowthPercent}%</div><div className="stat-label">Рост в месяц</div></div>
        <div className="stat-card"><div className="stat-icon">💰</div><div className="stat-value">{money(f.nextSixMonthsTotal)}</div><div className="stat-label">Сумма на 6 месяцев</div></div>
      </div>
      <div className="card">
        <div className="card-header"><div className="card-title">ИСТОРИЯ И ПРОГНОЗ НА 6 МЕСЯЦЕВ</div></div>
        <div className="chart-container" style={{ height: 300 }}>
          <Line
            data={{
              labels,
              datasets: [
                { label: 'Факт', data: [...f.history.map((h) => h.revenue), ...pad(6)], borderColor: CHART.primary, backgroundColor: CHART.primaryFill, fill: true, tension: 0.35 },
                { label: 'Прогноз', data: [...pad(f.history.length - 1), lastActual, ...f.forecast.map((p) => p.expected)], borderColor: CHART.primaryLight, borderDash: [5, 3], tension: 0.35 },
                { label: 'Оптимистично (+15%)', data: [...pad(f.history.length), ...f.forecast.map((p) => p.optimistic)], borderColor: CHART.green, borderDash: [2, 4], pointRadius: 0, borderWidth: 1 },
                { label: 'Пессимистично (−15%)', data: [...pad(f.history.length), ...f.forecast.map((p) => p.pessimistic)], borderColor: CHART.red, borderDash: [2, 4], pointRadius: 0, borderWidth: 1 },
              ],
            }}
            options={{ scales: axisOptions, spanGaps: false }}
          />
        </div>
      </div>
    </div>
  );
}
