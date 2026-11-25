import { useCallback, useEffect, useState } from 'react';
import { apiError } from '../services/features.service';

/** Загрузка данных с состоянием loading/error и ручным reload. */
export function useAsync<T>(loader: () => Promise<T>, deps: unknown[] = []) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const run = useCallback(loader, deps);

  const reload = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await run());
    } catch (e) {
      setError(apiError(e, 'Не удалось загрузить данные'));
    } finally {
      setLoading(false);
    }
  }, [run]);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    run()
      .then((d) => { if (!cancelled) setData(d); })
      .catch((e) => { if (!cancelled) setError(apiError(e, 'Не удалось загрузить данные')); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [run]);

  return { data, loading, error, reload, setData };
}

export const money = (v: number) => `${Math.round(v).toLocaleString('ru-RU')} TJS`;
export const dateRu = (iso: string | null | undefined) => (iso ? new Date(iso).toLocaleDateString('ru-RU') : '—');
export const timeRu = (iso: string) => new Date(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
