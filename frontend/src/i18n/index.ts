import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

const resources = {
  ru: {
    translation: {
      dashboard: 'Дашборд', orders: 'Заказы', kanban: 'Kanban', customers: 'Клиенты',
      vehicles: 'Автомобили', mechanics: 'Мастера', logout: 'Выйти', login: 'Вход',
      email: 'Email', password: 'Пароль', newOrder: 'Новый заказ', search: 'Поиск',
      ordersToday: 'Заказы сегодня', revenueToday: 'Выручка сегодня', activeOrders: 'Активных заказов',
      totalCustomers: 'Клиентов в базе', averageTicket: 'Средний чек', all: 'Все',
      save: 'Сохранить', cancel: 'Отмена', actions: 'Действия', status: 'Статус',
      schedule: 'Расписание', loyalty: 'Лояльность', warehouse: 'Склад', finance: 'Финансы',
      reports: 'Отчёты', settings: 'Настройки', operations: 'Операционный контроль', branches: 'Филиалы'
    }
  },
  tj: {
    translation: {
      dashboard: 'Дашборд', orders: 'Фармоишҳо', kanban: 'Kanban', customers: 'Мизоҳон',
      vehicles: 'Мошинҳо', mechanics: 'Усторо', logout: 'Баромадан', login: 'Воридшавӣ',
      email: 'Email', password: 'Парол', newOrder: 'Фармоиши нав', search: 'Ҷустуҷӯ',
      ordersToday: 'Фармоишҳои имрӯз', revenueToday: 'Даромади имрӯз', activeOrders: 'Фармоишҳои фаъол',
      totalCustomers: 'Мизоҳон дар база', averageTicket: 'Чеки миёна', all: 'Ҳама',
      save: 'Нигоҳ доштан', cancel: 'Бекор кардан', actions: 'Амалҳо', status: 'Вазъият',
      schedule: 'Ҷадвал', loyalty: 'Содиқӣ', warehouse: 'Анбор', finance: 'Молия',
      reports: 'Ҳисоботҳо', settings: 'Танзимот', operations: 'Назорати амалиётӣ', branches: 'Филиалҳо'
    }
  }
};

i18n.use(initReactI18next).init({
  resources,
  lng: localStorage.getItem('am_lang') || 'ru',
  fallbackLng: 'ru',
  interpolation: { escapeValue: false }
});

export default i18n;
