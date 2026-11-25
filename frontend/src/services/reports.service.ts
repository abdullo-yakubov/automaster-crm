import { api } from './api';

async function downloadFile(url: string, filename: string) {
  const res = await api.get(url, { responseType: 'blob' });
  const blobUrl = URL.createObjectURL(res.data);
  const link = document.createElement('a');
  link.href = blobUrl;
  link.download = filename;
  link.click();
  URL.revokeObjectURL(blobUrl);
}

export const reportsService = {
  dailyPdf: () => downloadFile('/reports/daily/pdf', `daily-report-${Date.now()}.pdf`),
  weeklyPdf: () => downloadFile('/reports/weekly/pdf', `weekly-report-${Date.now()}.pdf`),
  ordersCsv: () => downloadFile('/orders/export/csv', `orders-${Date.now()}.csv`)
};
