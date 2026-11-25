import {
  ArcElement, BarElement, CategoryScale, Chart as ChartJS, Filler, Legend, LinearScale,
  LineElement, PointElement, Tooltip,
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, ArcElement, Filler, Tooltip, Legend);

ChartJS.defaults.font.family = 'Montserrat, sans-serif';
ChartJS.defaults.font.size = 11;
ChartJS.defaults.color = '#90A8C8';
ChartJS.defaults.maintainAspectRatio = false;

/** Палитра макета v3 (синяя тема). */
export const CHART = {
  primary: '#2D7DD2',
  primaryLight: '#5B9FE8',
  primaryFill: 'rgba(45,125,210,0.12)',
  green: '#3DDC84',
  red: '#FF453A',
  yellow: '#FFD60A',
  purple: '#BF5AF2',
  grid: 'rgba(144,168,200,0.08)',
  series: ['#2D7DD2', '#5B9FE8', '#3DDC84', '#FFD60A', '#BF5AF2', '#FF453A', '#1A5BA6'],
};

export const axisOptions = {
  x: { grid: { color: CHART.grid } },
  y: { grid: { color: CHART.grid }, beginAtZero: true },
};
