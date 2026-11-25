# ТЕХНИЧЕСКОЕ ЗАДАНИЕ
## AutoMaster CRM v1 — Система управления автосервисом
### Город: Душанбе, Таджикистан

---

## 1. ОБЩЕЕ ОПИСАНИЕ ПРОЕКТА

**Название системы:** AutoMaster CRM  
**Назначение:** Полноценная CRM-система для управления автосервисом: от приёма авто до выдачи и аналитики.  
**Технологический стек:**
- Frontend: React 18 + TypeScript + React Router v6
- Backend: ASP.NET Core 8 (Web API) + Entity Framework Core
- Admin Panel: Blazor WebAssembly
- База данных: PostgreSQL
- Авторизация: JWT (Access + Refresh токены) + Role-based
- Документирование API: Swagger / OpenAPI
- Логирование: Serilog
- Язык интерфейса: Русский / Таджикский (i18n)
- Тема: Тёмная / Светлая (переключатель)

---

## 2. РОЛИ ПОЛЬЗОВАТЕЛЕЙ

| Роль | Описание | Уровень доступа |
|------|----------|-----------------|
| `Admin` | Владелец / Директор | Полный доступ ко всем модулям |
| `Manager` | Менеджер / Администратор стойки | Заказы, клиенты, склад, расписание |
| `Mechanic` | Мастер | Только свои заказы, чек-листы |
| `Accountant` | Бухгалтер | Финансы, отчёты |
| `Client` | Клиент | Личный кабинет: статус своего авто |

---

## 3. МОДУЛИ СИСТЕМЫ (18 страниц)

### 3.1 Dashboard — Дашборд
**URL:** `/dashboard`  
**Роли:** Admin, Manager, Accountant

**Компоненты:**
- **AI-панель** — блок "AI ДИРЕКТОР · АНАЛИЗ ДНЯ" с динамическими инсайтами
- **Быстрые действия (Quick Actions):**
  - Новый заказ (открывает модальное окно)
  - Перейти в Клиенты
  - Перейти в Kanban
  - Перейти на Склад
  - Генерация PDF отчёта
  - AI Диагностика
- **Статистика (Stats Cards) — 5 карточек:**
  - Заказы сегодня (с индикатором роста/падения)
  - Выручка сегодня (TJS)
  - Активных заказов (счётчик)
  - Клиентов в базе
  - Средний чек (TJS)
- **Графики:**
  - Выручка за 7 дней (линейный/bar-chart) — Chart.js
  - Распределение услуг (пончик/pie-chart)
- **Последние заказы** — список 5-10 последних
- **Топ мастера** — рейтинговый список

**API:**
- `GET /api/dashboard/stats` — статистика дня
- `GET /api/dashboard/revenue?days=7` — выручка по дням
- `GET /api/dashboard/services-distribution` — распределение услуг
- `GET /api/orders?limit=10&sort=created_desc` — последние заказы
- `GET /api/mechanics/top` — рейтинг мастеров

---

### 3.2 Orders — Заказы
**URL:** `/orders`  
**Роли:** Admin, Manager

**Компоненты:**
- **Фильтр-чипы:** Все / В работе / Готово / Ожидание
- **Поиск** по номеру, клиенту, авто, мастеру
- **Кнопка** "Новый заказ" (открывает modal)
- **Счётчик** найденных заказов
- **Кнопка** "Экспорт CSV"
- **Таблица заказов** — колонки:
  - # (ID заказа)
  - Клиент (имя + телефон)
  - Номер авто (стилизованный badge с гос. номером)
  - Авто (марка/модель)
  - Услуга (текст)
  - Мастер (аватар + имя)
  - Статус (цветной badge)
  - Сумма (TJS)
  - Дата
  - Действия (просмотр, изменение статуса, удаление)

**Статусы заказа:**
| Статус | Цвет | Описание |
|--------|------|----------|
| `intake` | синий | Приём |
| `diagnosis` | фиолетовый | Диагностика |
| `estimate` | жёлтый | Смета |
| `approved` | оранжевый | Одобрен |
| `inprogress` | голубой | В работе |
| `waiting` | красный | Ожидание запчасти |
| `ready` | зелёный | Готово |
| `completed` | серый | Завершён |

**Модальное окно "Новый заказ"** — поля:
- Имя клиента* (text)
- Телефон* (text, маска +992)
- Марка/Модель авто* (text)
- Гос. номер* (text, uppercase)
- VIN (text)
- Пробег (number, км)
- Услуга/Описание работ* (textarea)
- Мастер (select, список мастеров)
- Приоритет (Обычный / Высокий / Срочный)
- Стоимость работ (number, TJS)
- Стоимость запчастей (number, TJS)
- Примечание (text)
- Итого (авто-расчёт)

**Модальное окно "Детали заказа":**
- Все поля заказа (read-only)
- Статус с возможностью смены
- Временная шкала (Timeline) изменений
- Кнопка "Гарантийный PDF"
- Список запчастей использованных
- Фото до/после (photo-grid)

**API:**
- `GET /api/orders` — список (параметры: status, search, page, limit)
- `POST /api/orders` — создание заказа
- `GET /api/orders/{id}` — детали заказа
- `PUT /api/orders/{id}` — обновление
- `PATCH /api/orders/{id}/status` — смена статуса
- `DELETE /api/orders/{id}` — удаление
- `GET /api/orders/export/csv` — экспорт CSV

---

### 3.3 Kanban — Kanban-доска
**URL:** `/kanban`  
**Роли:** Admin, Manager, Mechanic

**Компоненты:**
- **Kanban-доска** с горизонтальным скроллом
- **8 колонок** — по каждому статусу заказа
- В каждой колонке: заголовок (статус + счётчик) + карточки заказов

**Kanban-карточка содержит:**
- Название услуги
- Имя клиента
- Гос. номер авто (badge в стиле номерного знака)
- Имя мастера
- SLA-таймер (время в работе, красный если превышен)

**Функциональность:**
- Drag & Drop карточек между колонками (смена статуса)
- При перетаскивании — автоматическое обновление статуса через API

**API:**
- `GET /api/orders/kanban` — все активные заказы, сгруппированные по статусу
- `PATCH /api/orders/{id}/status` — обновление статуса (через drag)

---

### 3.4 Schedule — Расписание
**URL:** `/schedule`  
**Роли:** Admin, Manager

**Компоненты:**
- **Карточка "Запись на сегодня":**
  - Список записей (клиент, время, услуга, мастер)
  - Кнопка "Записать" (открывает modal)
- **Карточка "Слоты":**
  - Сетка временных слотов (08:00 — 18:00)
  - Цвет: зелёный = свободен, красный = занят

**Модальное окно "Онлайн запись"** — поля:
- Клиент (text)
- Телефон (text)
- Дата (date)
- Время (select: 08:00 — 18:00)
- Услуга (text)

**API:**
- `GET /api/bookings?date=YYYY-MM-DD` — записи на дату
- `POST /api/bookings` — новая запись
- `GET /api/bookings/slots?date=YYYY-MM-DD` — доступные слоты
- `DELETE /api/bookings/{id}` — отмена записи

---

### 3.5 Customers — Клиенты
**URL:** `/customers`  
**Роли:** Admin, Manager

**Компоненты:**
- Поиск по имени, телефону, авто
- Кнопка "Добавить клиента"
- **Таблица клиентов** — колонки:
  - Клиент (аватар + имя)
  - Телефон
  - Авто / Гос. номер
  - Последний визит
  - Всего оплачено (TJS)
  - Статус (Обычный / VIP / Корпоративный)
  - Баллы лояльности
  - Действия (история, редактировать, удалить)

**Модальное окно "Новый клиент"** — поля:
- Полное имя* (text)
- Телефон* (text, маска)
- Email (email)
- День рождения (date)
- Марка/Модель авто (text)
- Гос. номер (text)
- Статус (Обычный / VIP / Корпоративный)
- Примечание (textarea)

**Модальное окно "История клиента":**
- Данные клиента
- Список всех визитов (таблица заказов)
- Сумма потраченного
- Баллы лояльности
- Статус

**API:**
- `GET /api/customers` — список (search, status, page, limit)
- `POST /api/customers` — создание
- `GET /api/customers/{id}` — детали + история заказов
- `PUT /api/customers/{id}` — обновление
- `DELETE /api/customers/{id}` — удаление

---

### 3.6 Vehicles — Автомобили
**URL:** `/vehicles`  
**Роли:** Admin, Manager

**Компоненты:**
- Поиск по марке, модели, VIN, гос. номеру
- Кнопка "Добавить авто"
- **Таблица автомобилей** — колонки:
  - Авто (марка + модель)
  - Гос. номер (badge)
  - VIN
  - Клиент
  - Пробег (км)
  - Год выпуска
  - Количество визитов
  - Действия

**Модальное окно "Добавить автомобиль"** — поля:
- Марка* (text)
- Модель* (text)
- Год (number)
- Гос. номер* (text)
- VIN (text)
- Пробег (number)
- Владелец (text или выбор из клиентов)

**API:**
- `GET /api/vehicles` — список
- `POST /api/vehicles` — создание
- `GET /api/vehicles/{id}` — детали + история ремонтов
- `PUT /api/vehicles/{id}` — обновление
- `DELETE /api/vehicles/{id}` — удаление

---

### 3.7 Loyalty — Программа лояльности
**URL:** `/loyalty`  
**Роли:** Admin, Manager

**Компоненты:**
- **Stats Cards:**
  - Всего участников
  - VIP клиентов
  - Выдано баллов
  - Использовано баллов
- **Топ клиенты** — лидерборд:
  - Ранг (золото/серебро/бронза)
  - Аватар + имя
  - Статус (VIP / Обычный)
  - Кол-во баллов
  - Потрачено TJS

**Логика лояльности:**
- За каждые 100 TJS = 1 балл
- 100 баллов = скидка 5%
- VIP статус: от 5 000 TJS суммарных трат
- Корпоративный статус: от 20 000 TJS

**API:**
- `GET /api/loyalty/stats` — общая статистика
- `GET /api/loyalty/top?limit=10` — топ клиентов по баллам
- `POST /api/loyalty/award` — начислить баллы
- `POST /api/loyalty/redeem` — списать баллы

---

### 3.8 Mechanics — Мастера & KPI
**URL:** `/mechanics`  
**Роли:** Admin

**Компоненты:**
- Кнопка "Добавить мастера"
- **Сетка карточек мастеров** (2 колонки):

**Карточка мастера содержит:**
- Аватар (инициалы)
- Имя и специализация
- Уровень (Junior / Middle / Senior / Expert)
- KPI метрики:
  - Выполнено заказов
  - Выручка (TJS)
  - Рейтинг (звёзды)
  - Загруженность (прогресс-бар)
- Оклад (TJS/мес)
- Статус (активен/неактивен)

**Модальное окно "Новый мастер"** — поля:
- Имя* (text)
- Телефон (text)
- Специализация (select: Двигатель / Подвеска / Электрика / Кузов / Универсал)
- Уровень (select: Junior / Middle / Senior / Expert)
- Оклад (number, TJS/мес)

**API:**
- `GET /api/mechanics` — список мастеров
- `POST /api/mechanics` — создание
- `GET /api/mechanics/{id}/kpi` — KPI конкретного мастера
- `PUT /api/mechanics/{id}` — обновление
- `DELETE /api/mechanics/{id}` — удаление
- `GET /api/mechanics/top` — рейтинг для дашборда

---

### 3.9 Warehouse — Склад запчастей
**URL:** `/warehouse`  
**Роли:** Admin, Manager

**Компоненты:**
- **Stats Cards:**
  - Всего позиций
  - Низкий остаток (алерт)
  - Общая стоимость склада (TJS)
  - Оборот за месяц (TJS)
- **Фильтр-чипы:** Все / Мало / ОК
- Поиск по названию или артикулу
- Кнопка "Добавить запчасть"
- **Таблица запчастей** — колонки:
  - Название
  - Артикул (SKU)
  - Категория
  - Остаток (шт.) + мини stock-bar
  - Минимальный остаток
  - Цена закупки (TJS)
  - Цена продажи (TJS)
  - Маржа (%)
  - Статус (ОК / Мало / Нет)
  - Действия

**Категории запчастей:**
Фильтры / Масла / Тормоза / Подвеска / Двигатель / Электрика / Кузов / Расходники

**Модальное окно "Добавить запчасть"** — поля:
- Название* (text)
- Артикул (text)
- Категория (select)
- Остаток (number)
- Минимальный остаток (number)
- Цена закупки (number, TJS)
- Цена продажи (number, TJS)

**API:**
- `GET /api/parts` — список (filter, search, page)
- `POST /api/parts` — создание
- `PUT /api/parts/{id}` — обновление
- `DELETE /api/parts/{id}` — удаление
- `GET /api/parts/low-stock` — позиции с низким остатком
- `PATCH /api/parts/{id}/quantity` — обновление остатка

---

### 3.10 Finance — Финансы
**URL:** `/finance`  
**Роли:** Admin, Accountant

**Компоненты:**
- **Stats Cards:**
  - Выручка этого месяца (TJS)
  - Расходы этого месяца (TJS)
  - Прибыль (TJS)
  - Маржа (%)
- **Графики:**
  - Доходы/Расходы за 6 месяцев (grouped bar chart)
  - Структура расходов (doughnut chart)
- **Таблица транзакций** — колонки:
  - Дата
  - Описание
  - Тип (Доход/Расход)
  - Категория
  - Сумма (TJS)
- Кнопка "Добавить транзакцию"

**Категории транзакций:**
Ремонт / Запчасти / Аренда / Зарплата / Маркетинг / Оборудование / Прочее

**Модальное окно "Транзакция"** — поля:
- Тип (Доход / Расход)
- Сумма (number, TJS)
- Описание (text)
- Категория (select)
- Дата (date)

**API:**
- `GET /api/finance/stats` — P&L сводка
- `GET /api/transactions` — список транзакций
- `POST /api/transactions` — создание
- `GET /api/finance/chart?months=6` — данные для графика
- `GET /api/finance/expenses-structure` — структура расходов

---

### 3.11 Analytics — Аналитика & BI
**URL:** `/analytics`  
**Роли:** Admin

**Компоненты (вкладки):**

**Вкладка "Обзор":**
- График "Популярные услуги" (bar chart)
- График "Маржа по мастерам" (bar chart)

**Вкладка "KPI мастеров":**
- Таблица с KPI всех мастеров:
  - Имя
  - Заказов выполнено
  - Выручка
  - Средний чек
  - Рейтинг
  - Загруженность %

**Вкладка "Тепловая карта":**
- Grid загрузки по дням недели × часам
- Цвет: от светлого (мало заказов) до тёмного (много)

**Вкладка "Прогноз AI":**
- AI-панель с прогнозом выручки
- График прогноза на 6 месяцев (line chart)

**API:**
- `GET /api/analytics/popular-services` — топ услуг
- `GET /api/analytics/mechanics-margin` — маржа по мастерам
- `GET /api/analytics/heatmap` — тепловая карта нагрузки
- `GET /api/analytics/forecast?months=6` — AI прогноз
- `GET /api/analytics/mechanics-kpi` — KPI таблица

---

### 3.12 AI Механик
**URL:** `/ai`  
**Роли:** Admin, Manager, Mechanic

**Компоненты:**
- **Форма диагностики:**
  - VIN или номер авто (text)
  - Описание проблемы (textarea)
  - Пробег (number)
  - Кнопка "Запустить AI диагностику"
- **Блок результата AI:**
  - Вероятные неисправности (список)
  - Рекомендуемые работы
  - Необходимые запчасти
  - Ориентировочная стоимость
- **Predictive Maintenance (прогноз ТО):**
  - Список автомобилей с прогнозом следующего ТО
  - Дни до ТО / Пробег до ТО
- **Risk Map Душанбе:**
  - Текстовая карта рисков (типичные проблемы авто в условиях города)

**API:**
- `POST /api/ai/diagnose` — запуск диагностики (VIN, проблема, пробег)
- `GET /api/ai/predictive-maintenance` — прогноз ТО для авто
- `GET /api/ai/risk-map` — карта рисков по городу

---

### 3.13 Marketing — Маркетинг
**URL:** `/marketing`  
**Роли:** Admin

**Компоненты:**
- **Stats Cards:**
  - Отправлено сообщений
  - Охват клиентов
  - Конверсия (%)
  - Активных промо
- **Форма создания рассылки:**
  - Канал (select: Telegram / WhatsApp / SMS / Email)
  - Сегмент (select: Все / VIP / Спящие 3+ мес / После ТО)
  - Текст сообщения (textarea)
  - Кнопка "AI генерация" (авто-генерация текста)
  - Кнопка "Отправить"
- **Промо-коды:**
  - Список активных промо-кодов (код, скидка, использований, статус)
  - Кнопка "Создать промо"
- **История кампаний** — список прошлых рассылок

**API:**
- `GET /api/marketing/stats` — статистика кампаний
- `POST /api/marketing/campaigns` — создание кампании
- `GET /api/marketing/campaigns` — история
- `POST /api/marketing/promo-codes` — создание промо-кода
- `GET /api/marketing/promo-codes` — список промо-кодов
- `POST /api/ai/generate-marketing-text` — AI генерация текста

---

### 3.14 QR Система
**URL:** `/qr-system`  
**Роли:** Admin, Manager

**Вкладки:**

**1. QR Запись на сервис:**
- Поле URL/текст для QR
- Подпись
- Кнопка генерации
- Превью QR кода (белый фон)
- Кнопка "Скачать PNG"

**2. QR для оплаты:**
- Номер заказа
- Сумма (TJS)
- Получатель
- Генерация QR

**3. QR Гарантийный талон:**
- Выбор заказа из списка (select)
- Генерация QR с данными гарантии

**4. QR Промо код:**
- Промо код (text)
- Скидка (text)
- Генерация QR

**API:**
- `POST /api/qr/generate` — генерация QR (тип + данные)
- `GET /api/qr/warranty/{orderId}` — данные гарантии для QR

---

### 3.15 Кабинет клиента
**URL:** `/client-portal`  
**Роли:** Admin (симуляция), Client (личный кабинет)

**Компоненты (в режиме симуляции — выбор клиента):**
- Select "Выберите клиента"

**После выбора — личный кабинет:**
- **Header клиента:**
  - Аватар (инициалы)
  - Имя, телефон
  - Статус (VIP badge)
  - Баллы / Визитов / Всего TJS
- **Статус текущего ремонта:**
  - Прогресс-бар с этапами
  - Текущий статус
  - Ориентировочное время
- **Мой автомобиль:**
  - Марка/Модель
  - Гос. номер
  - VIN
  - Пробег
- **История ремонтов:**
  - Таблица всех прошлых заказов
- **Уведомления:**
  - Список системных уведомлений

**Отдельный публичный URL для клиентов:**
`/client/{clientId}/portal` — защищён токеном клиента

**API:**
- `GET /api/client/me` — данные авторизованного клиента
- `GET /api/client/me/orders` — история заказов
- `GET /api/client/me/active-order` — текущий активный заказ

---

### 3.16 Операционный контроль
**URL:** `/operations`  
**Роли:** Admin, Manager

**Вкладки:**

**1. Чек-листы:**
- **Открытие сервиса** — список чекбоксов (с прогрессом X/Y):
  - Проверить оборудование
  - Включить освещение
  - Подготовить рабочие места
  - Проверить запасы
  - Открыть кассу
  - Провести планёрку
- **Закрытие сервиса** — аналогичный список
- **ТО Оборудования** — чеклист с возможностью добавить пункт

**2. IoT Сенсоры:**
- Карточки сенсоров (температура, давление, влажность, уровень масла)
- График истории показаний
- Кнопка "Обновить"

**3. SLA Нарушения:**
- Таблица: Заказ / Клиент / Статус / SLA (часов) / Прошло / Превышение

**4. Аудит качества (Mystery Shopper):**
- 7 критериев оценки от 1 до 5 звёзд:
  - Приветствие клиента
  - Скорость обслуживания
  - Чистота боксов
  - Вежливость мастера
  - Качество ремонта
  - Правильность счёта
  - Время выдачи авто
- Итоговый балл (авто-расчёт)
- Кнопка "Сохранить аудит"
- История аудитов

**5. Аудит-лог:**
- Таблица всех действий в системе:
  - Время / Пользователь / Действие / Объект / Детали

**API:**
- `GET /api/checklists/{type}` — чеклист (open/close)
- `PATCH /api/checklists/{type}/{itemId}` — отметить пункт
- `GET /api/iot/sensors` — данные сенсоров
- `GET /api/orders/sla-violations` — нарушения SLA
- `POST /api/audit/mystery-shopper` — сохранить аудит
- `GET /api/audit/mystery-shopper` — история аудитов
- `GET /api/audit/logs` — аудит-лог системы

---

### 3.17 Филиалы (Multi-service)
**URL:** `/branches`  
**Роли:** Admin

**Компоненты:**
- **Stats Cards сети:**
  - Кол-во филиалов
  - Суммарная выручка сети
  - Суммарных заказов
  - Средний NPS сети
- **Форма добавления филиала:**
  - Название (text)
  - Адрес (text)
  - Телефон (text)
  - Тип (select: Собственный / Франшиза / White Label / Партнёр)
- **Сводный KPI сети** — метрики по каждому филиалу
- **Список филиалов** — карточки с данными и статистикой

**API:**
- `GET /api/branches` — список филиалов
- `POST /api/branches` — добавить филиал
- `GET /api/branches/network-kpi` — сводный KPI
- `PUT /api/branches/{id}` — обновить
- `DELETE /api/branches/{id}` — удалить

---

### 3.18 BI Конструктор
**URL:** `/bi`  
**Роли:** Admin

**Компоненты:**
- Переключатель вид "Сетка / Список"
- Кнопка "Добавить виджет"
- **Модальное окно выбора виджета** (8 вариантов):
  - Карточка выручки
  - График заказов
  - KPI мастеров
  - Алерты склада
  - Топ услуги
  - Маржа прибыли
  - Сегменты клиентов
  - SLA мониторинг
- **Кастомный дашборд** — сетка из добавленных виджетов

**API:**
- `GET /api/bi/widgets` — конфигурация виджетов пользователя
- `POST /api/bi/widgets` — сохранить виджет
- `DELETE /api/bi/widgets/{id}` — удалить виджет
- `GET /api/bi/data/{widgetType}` — данные для виджета

---

### 3.19 Digital Twin — Цифровой двойник
**URL:** `/digital-twin`  
**Роли:** Admin

**Компоненты:**
- AI-панель с анализом загрузки
- **Боксы сервиса в реальном времени:**
  - 6 боксов (Двигатель / Подвеска / Электрика / Кузов / Шиномонтаж / Мойка)
  - Каждый бокс: название, специализация, статус (свободен/занят)
  - Badge: "Свободно: N" (зелёный) / "Занято: N" (красный)
- **График прогноза загрузки** (line chart)
- **Метрики пропускной способности:**
  - Авто/день
  - Среднее время ремонта
  - Эффективность боксов

**API:**
- `GET /api/twin/boxes` — статус боксов в реальном времени
- `GET /api/twin/forecast` — прогноз загрузки

---

### 3.20 Voice Assistant — Голосовой ассистент
**URL:** `/voice`  
**Роли:** Admin, Manager, Mechanic

**Компоненты:**
- Большая кнопка "Начать запись" (микрофон)
- Статус записи (иконка + текст)
- Транскрипт (текст распознанной речи)
- Результат выполненной команды
- **Список поддерживаемых команд:**
  - "добавить замену масла" → создаёт услугу
  - "новый заказ" → открывает форму
  - "статистика" → переход в дашборд
  - "список клиентов" → переход в CRM
  - "склад" → переход на склад
  - "финансы" → переход в финансы
- История выполненных команд

**API:**
- `POST /api/voice/command` — обработка голосовой команды (текст → действие)

---

### 3.21 Reports — Отчёты
**URL:** `/reports`  
**Роли:** Admin, Accountant

**Компоненты:**
- **3 карточки-кнопки:**
  - Дневной отчёт (PDF за сегодня)
  - Недельный отчёт (PDF за 7 дней)
  - Экспорт CSV (заказы в Excel)
- **Полная история заказов** — таблица:
  - ID / Дата / Клиент / Авто / Гос. номер / Услуги / Мастер / Оплата / Статус

**API:**
- `GET /api/reports/daily/pdf` — генерация PDF дневного отчёта
- `GET /api/reports/weekly/pdf` — недельный PDF
- `GET /api/orders/export/csv` — CSV экспорт

---

### 3.22 Settings — Настройки
**URL:** `/settings`  
**Роли:** Admin

**Компоненты:**
- **Блок "Сервис":**
  - Название сервиса (text)
  - Адрес (text)
  - Телефон (text)
  - Валюта (select: TJS / USD / RUB)
  - Кнопка "Сохранить"
- **Блок "Данные":**
  - Экспорт всех данных (JSON)
  - Импорт данных (JSON)
  - Очистить все данные (с подтверждением)
- **Блок "Тема & Язык":**
  - Переключатель тёмная/светлая тема
  - Русский / Таджикский

**API:**
- `GET /api/settings` — текущие настройки
- `PUT /api/settings` — обновить настройки
- `GET /api/data/export` — полный экспорт
- `POST /api/data/import` — импорт

---

## 4. СХЕМА БАЗЫ ДАННЫХ

### Таблица `Users`
```sql
Id (UUID, PK)
Email (varchar, unique)
PasswordHash (varchar)
FullName (varchar)
Role (enum: Admin, Manager, Mechanic, Accountant, Client)
Phone (varchar)
IsActive (bool)
CreatedAt (timestamp)
UpdatedAt (timestamp)
```

### Таблица `Customers`
```sql
Id (UUID, PK)
FullName (varchar)
Phone (varchar, unique)
Email (varchar, nullable)
Birthday (date, nullable)
Status (enum: regular, vip, corporate)
LoyaltyPoints (int, default 0)
TotalSpent (decimal)
Note (text, nullable)
CreatedAt (timestamp)
UpdatedAt (timestamp)
```

### Таблица `Vehicles`
```sql
Id (UUID, PK)
CustomerId (UUID, FK → Customers)
Make (varchar)
Model (varchar)
Year (int)
LicensePlate (varchar, unique)
VIN (varchar, nullable)
Mileage (int)
CreatedAt (timestamp)
```

### Таблица `Orders`
```sql
Id (UUID, PK)
OrderNumber (varchar, auto-generated)
CustomerId (UUID, FK → Customers)
VehicleId (UUID, FK → Vehicles)
MechanicId (UUID, FK → Mechanics, nullable)
ServiceDescription (text)
Status (enum: intake, diagnosis, estimate, approved, inprogress, waiting, ready, completed)
Priority (enum: normal, high, urgent)
LaborCost (decimal)
PartsCost (decimal)
TotalCost (decimal)
Note (text, nullable)
CreatedAt (timestamp)
UpdatedAt (timestamp)
CompletedAt (timestamp, nullable)
BranchId (UUID, FK → Branches)
```

### Таблица `OrderTimeline`
```sql
Id (UUID, PK)
OrderId (UUID, FK → Orders)
Status (varchar)
Note (text)
UserId (UUID, FK → Users)
CreatedAt (timestamp)
```

### Таблица `Mechanics`
```sql
Id (UUID, PK)
UserId (UUID, FK → Users, nullable)
FullName (varchar)
Phone (varchar)
Specialization (enum: engine, suspension, electrical, body, general)
Level (enum: junior, middle, senior, expert)
Salary (decimal)
IsActive (bool)
Rating (decimal, default 0)
CreatedAt (timestamp)
```

### Таблица `Parts`
```sql
Id (UUID, PK)
Name (varchar)
SKU (varchar, unique, nullable)
Category (enum: filters, oils, brakes, suspension, engine, electrical, body, consumables)
Quantity (int)
MinQuantity (int)
CostPrice (decimal)
SalePrice (decimal)
BranchId (UUID, FK → Branches)
CreatedAt (timestamp)
UpdatedAt (timestamp)
```

### Таблица `Transactions`
```sql
Id (UUID, PK)
Type (enum: income, expense)
Amount (decimal)
Description (varchar)
Category (varchar)
Date (date)
OrderId (UUID, FK → Orders, nullable)
CreatedBy (UUID, FK → Users)
BranchId (UUID, FK → Branches)
CreatedAt (timestamp)
```

### Таблица `Bookings`
```sql
Id (UUID, PK)
ClientName (varchar)
Phone (varchar)
ServiceDescription (varchar)
BookingDate (date)
BookingTime (time)
MechanicId (UUID, nullable)
Status (enum: pending, confirmed, cancelled)
BranchId (UUID, FK → Branches)
CreatedAt (timestamp)
```

### Таблица `Branches`
```sql
Id (UUID, PK)
Name (varchar)
Address (varchar)
Phone (varchar)
Type (enum: own, franchise, whitelabel, partner)
IsActive (bool)
CreatedAt (timestamp)
```

### Таблица `LoyaltyTransactions`
```sql
Id (UUID, PK)
CustomerId (UUID, FK → Customers)
Points (int)
Type (enum: award, redeem)
Reason (varchar)
OrderId (UUID, nullable)
CreatedAt (timestamp)
```

### Таблица `AuditLogs`
```sql
Id (UUID, PK)
UserId (UUID, FK → Users)
Action (varchar)
ObjectType (varchar)
ObjectId (varchar)
Details (text)
BranchId (UUID)
CreatedAt (timestamp)
```

### Таблица `MysteryAudits`
```sql
Id (UUID, PK)
Scores (jsonb)
TotalScore (decimal)
UserId (UUID, FK → Users)
BranchId (UUID, FK → Branches)
CreatedAt (timestamp)
```

### Таблица `Settings`
```sql
Id (UUID, PK)
BranchId (UUID, FK → Branches)
ServiceName (varchar)
Address (varchar)
Phone (varchar)
Currency (varchar, default 'TJS')
Theme (varchar, default 'dark')
Language (varchar, default 'ru')
UpdatedAt (timestamp)
```

### Таблица `MarketingCampaigns`
```sql
Id (UUID, PK)
Channel (enum: telegram, whatsapp, sms, email)
Segment (varchar)
MessageText (text)
SentCount (int)
Status (enum: draft, sent)
BranchId (UUID)
CreatedAt (timestamp)
```

### Таблица `PromoCodes`
```sql
Id (UUID, PK)
Code (varchar, unique)
Discount (varchar)
UsageCount (int, default 0)
MaxUsage (int, nullable)
ExpiresAt (timestamp, nullable)
IsActive (bool)
BranchId (UUID)
CreatedAt (timestamp)
```

---

## 5. API ЭНДПОИНТЫ (сводная таблица)

### Auth
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| POST | `/api/auth/login` | Вход в систему | Public |
| POST | `/api/auth/refresh` | Обновление токена | Public |
| POST | `/api/auth/logout` | Выход | Authenticated |
| GET | `/api/auth/me` | Текущий пользователь | Authenticated |

### Dashboard
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| GET | `/api/dashboard/stats` | KPI метрики | Admin, Manager |
| GET | `/api/dashboard/revenue` | Выручка по дням | Admin, Manager |
| GET | `/api/dashboard/services-distribution` | Распределение услуг | Admin |

### Orders
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| GET | `/api/orders` | Список заказов | Admin, Manager |
| POST | `/api/orders` | Создать заказ | Admin, Manager |
| GET | `/api/orders/{id}` | Детали заказа | Admin, Manager, Mechanic |
| PUT | `/api/orders/{id}` | Обновить заказ | Admin, Manager |
| PATCH | `/api/orders/{id}/status` | Сменить статус | Admin, Manager, Mechanic |
| DELETE | `/api/orders/{id}` | Удалить заказ | Admin |
| GET | `/api/orders/kanban` | Kanban данные | Admin, Manager, Mechanic |
| GET | `/api/orders/export/csv` | Экспорт CSV | Admin, Accountant |

### Customers
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| GET | `/api/customers` | Список клиентов | Admin, Manager |
| POST | `/api/customers` | Создать клиента | Admin, Manager |
| GET | `/api/customers/{id}` | Детали клиента | Admin, Manager |
| PUT | `/api/customers/{id}` | Обновить | Admin, Manager |
| DELETE | `/api/customers/{id}` | Удалить | Admin |

### Vehicles
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| GET | `/api/vehicles` | Список авто | Admin, Manager |
| POST | `/api/vehicles` | Добавить авто | Admin, Manager |
| GET | `/api/vehicles/{id}` | Детали авто | Admin, Manager |
| PUT | `/api/vehicles/{id}` | Обновить | Admin, Manager |
| DELETE | `/api/vehicles/{id}` | Удалить | Admin |

### Mechanics
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| GET | `/api/mechanics` | Список мастеров | Admin, Manager |
| POST | `/api/mechanics` | Добавить мастера | Admin |
| GET | `/api/mechanics/{id}/kpi` | KPI мастера | Admin |
| PUT | `/api/mechanics/{id}` | Обновить | Admin |
| DELETE | `/api/mechanics/{id}` | Удалить | Admin |

### Parts (Склад)
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| GET | `/api/parts` | Список запчастей | Admin, Manager |
| POST | `/api/parts` | Добавить | Admin, Manager |
| PUT | `/api/parts/{id}` | Обновить | Admin, Manager |
| PATCH | `/api/parts/{id}/quantity` | Изменить остаток | Admin, Manager |
| DELETE | `/api/parts/{id}` | Удалить | Admin |

### Finance
| Метод | URL | Описание | Роль |
|-------|-----|----------|------|
| GET | `/api/finance/stats` | P&L сводка | Admin, Accountant |
| GET | `/api/transactions` | Транзакции | Admin, Accountant |
| POST | `/api/transactions` | Добавить транзакцию | Admin, Accountant |

---

## 6. АВТОРИЗАЦИЯ И БЕЗОПАСНОСТЬ

### JWT токены
- Access Token: время жизни 15 минут
- Refresh Token: время жизни 7 дней
- Алгоритм: HS256
- Хранение: Access — Memory (React) / Refresh — HttpOnly Cookie

### Middleware
- `[Authorize(Roles = "Admin")]` — только для администратора
- `[Authorize(Roles = "Admin,Manager")]` — менеджерские функции
- `[Authorize]` — любой авторизованный пользователь

### Пароли
- Хеширование: BCrypt (rounds = 12)
- Минимальная длина: 8 символов

### Rate Limiting
- Auth эндпоинты: 5 запросов/мин на IP
- Прочие: 100 запросов/мин на пользователя

---

## 7. FRONTEND АРХИТЕКТУРА (React)

### Структура проекта
```
src/
├── pages/
│   ├── Dashboard/
│   ├── Orders/
│   ├── Kanban/
│   ├── Schedule/
│   ├── Customers/
│   ├── Vehicles/
│   ├── Loyalty/
│   ├── Mechanics/
│   ├── Warehouse/
│   ├── Finance/
│   ├── Analytics/
│   ├── AI/
│   ├── Marketing/
│   ├── QRSystem/
│   ├── ClientPortal/
│   ├── Operations/
│   ├── Branches/
│   ├── BI/
│   ├── DigitalTwin/
│   ├── Voice/
│   ├── Reports/
│   └── Settings/
├── components/
│   ├── Layout/
│   │   ├── Sidebar.tsx
│   │   ├── Topbar.tsx
│   │   └── Layout.tsx
│   ├── UI/
│   │   ├── StatCard.tsx
│   │   ├── Badge.tsx
│   │   ├── Modal.tsx
│   │   ├── DataTable.tsx
│   │   ├── KanbanBoard.tsx
│   │   ├── FilterChips.tsx
│   │   ├── SearchBar.tsx
│   │   ├── Toast.tsx
│   │   ├── ProgressBar.tsx
│   │   ├── Timeline.tsx
│   │   ├── Avatar.tsx
│   │   ├── LicensePlate.tsx
│   │   └── AIPanel.tsx
│   ├── Charts/
│   │   ├── RevenueChart.tsx
│   │   ├── PieChart.tsx
│   │   └── ForecastChart.tsx
│   └── Forms/
│       ├── OrderForm.tsx
│       ├── CustomerForm.tsx
│       ├── VehicleForm.tsx
│       ├── MechanicForm.tsx
│       ├── PartForm.tsx
│       └── TransactionForm.tsx
├── services/
│   ├── api.ts (axios instance)
│   ├── auth.service.ts
│   ├── orders.service.ts
│   ├── customers.service.ts
│   ├── vehicles.service.ts
│   ├── mechanics.service.ts
│   ├── parts.service.ts
│   ├── finance.service.ts
│   └── analytics.service.ts
├── hooks/
│   ├── useAuth.ts
│   ├── useOrders.ts
│   └── useToast.ts
├── store/ (Zustand или Context)
│   ├── authStore.ts
│   └── appStore.ts
├── utils/
│   ├── formatters.ts (дата, валюта)
│   ├── validators.ts
│   └── i18n.ts
└── types/
    └── index.ts (все TypeScript интерфейсы)
```

---

## 8. BACKEND АРХИТЕКТУРА (.NET)

### Структура проекта
```
AutoMaster.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── OrdersController.cs
│   ├── CustomersController.cs
│   ├── VehiclesController.cs
│   ├── MechanicsController.cs
│   ├── PartsController.cs
│   ├── FinanceController.cs
│   ├── AnalyticsController.cs
│   ├── AIController.cs
│   ├── MarketingController.cs
│   ├── BookingsController.cs
│   ├── BranchesController.cs
│   ├── LoyaltyController.cs
│   ├── OperationsController.cs
│   ├── ReportsController.cs
│   └── SettingsController.cs
├── Services/
│   ├── AuthService.cs
│   ├── OrderService.cs
│   ├── CustomerService.cs
│   ├── VehicleService.cs
│   ├── MechanicService.cs
│   ├── PartsService.cs
│   ├── FinanceService.cs
│   ├── AnalyticsService.cs
│   ├── AIService.cs
│   ├── PdfService.cs
│   ├── LoyaltyService.cs
│   └── AuditService.cs
├── Repositories/
│   ├── IOrderRepository.cs / OrderRepository.cs
│   ├── ICustomerRepository.cs / CustomerRepository.cs
│   └── ... (по аналогии для каждой сущности)
├── Models/
│   ├── Entities/ (Domain entities)
│   └── DTOs/ (Request/Response DTOs)
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Middleware/
│   ├── AuditMiddleware.cs
│   └── ExceptionMiddleware.cs
└── Program.cs
```

---

## 9. ADMIN PANEL АРХИТЕКТУРА (Blazor WASM)

### Структура проекта
```
AutoMaster.Admin/
├── Pages/
│   ├── Index.razor (Dashboard)
│   ├── Users.razor
│   ├── AllOrders.razor
│   ├── AllCustomers.razor
│   ├── AllBranches.razor
│   ├── FinanceOverview.razor
│   ├── SystemLogs.razor
│   └── Settings.razor
├── Components/
│   ├── DataGrid.razor
│   ├── StatCard.razor
│   ├── UserModal.razor
│   └── ConfirmDialog.razor
├── Services/
│   ├── ApiService.cs
│   └── AuthService.cs
└── Shared/
    ├── MainLayout.razor
    ├── NavMenu.razor
    └── AdminHeader.razor
```

**Функционал Admin Panel:**
- Управление пользователями (CRUD) + назначение ролей
- Просмотр всех заказов (all branches)
- Просмотр финансов по всем филиалам
- Системные логи (Audit Log)
- Управление настройками системы
- Дашборд с общей статистикой сети

---

## 10. НЕФУНКЦИОНАЛЬНЫЕ ТРЕБОВАНИЯ

| Требование | Значение |
|-----------|---------|
| Время отклика API | < 300 мс для 95% запросов |
| Доступность | 99.5% uptime |
| Поддержка браузеров | Chrome 90+, Firefox 90+, Edge 90+ |
| Адаптивность | Mobile (< 900px), Tablet, Desktop |
| Языки | Русский (основной), Таджикский |
| Темы | Тёмная (по умолчанию), Светлая |
| PDF генерация | jsPDF (дневные/недельные отчёты, гарантийные талоны) |
| QR коды | qrcodejs |
| Графики | Chart.js |

---

## 11. ПОЛЬЗОВАТЕЛЬСКИЕ СЦЕНАРИИ (User Flow)

### Сценарий 1: Приём нового клиента
1. Менеджер нажимает "+" в topbar или на Dashboard
2. Открывается модальное "НОВЫЙ ЗАКАЗ"
3. Вводит: имя, телефон, авто, гос. номер, описание проблемы
4. Выбирает мастера и приоритет
5. Указывает стоимость
6. Нажимает "Создать заказ"
7. Заказ появляется в таблице со статусом "Приём"
8. На Kanban-доске появляется карточка в колонке "Intake"
9. Клиент добавляется в базу (если новый)

### Сценарий 2: Прогресс ремонта
1. Мастер открывает Kanban
2. Перетаскивает карточку заказа из "Intake" → "Diagnosis"
3. Статус обновляется в таблице заказов
4. Клиент видит обновлённый статус в своём личном кабинете
5. После диагностики мастер перетаскивает в "Estimate"
6. После одобрения → "In Progress"
7. При нехватке запчасти → "Waiting"
8. После завершения → "Ready"

### Сценарий 3: Выдача авто и оплата
1. Статус "Ready" — менеджер уведомляет клиента
2. Открывает детали заказа
3. Генерирует QR для оплаты
4. После оплаты — меняет статус на "Completed"
5. Генерирует гарантийный PDF
6. В финансах автоматически создаётся транзакция "Доход"
7. Клиенту начисляются баллы лояльности

### Сценарий 4: AI Диагностика
1. Мастер переходит в "AI Механик"
2. Вводит VIN или гос. номер
3. Описывает симптомы
4. Нажимает "Запустить AI диагностику"
5. Получает: вероятные неисправности + рекомендации + список запчастей

### Сценарий 5: Конец рабочего дня
1. Менеджер переходит в "Операционный контроль"
2. Отмечает пункты чеклиста "Закрытие сервиса"
3. Переходит в Reports
4. Нажимает "Дневной отчёт" → скачивает PDF
5. Dashboard показывает итоги дня

---

## 12. ИНСТРУКЦИЯ ЗАПУСКА

### Backend (.NET)
```bash
cd AutoMaster.API
dotnet restore
# настроить appsettings.json: ConnectionStrings.DefaultConnection
dotnet ef database update
dotnet run
# API: https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

### Frontend (React)
```bash
cd automaster-frontend
npm install
# настроить .env: REACT_APP_API_URL=https://localhost:5001
npm start
# http://localhost:3000
```

### Admin Panel (Blazor)
```bash
cd AutoMaster.Admin
dotnet restore
# настроить: appsettings.json API URL
dotnet run
# http://localhost:5002
```

### База данных (PostgreSQL)
```sql
CREATE DATABASE automaster;
-- применить миграции через dotnet ef
```

---

## 13. ТЕСТОВЫЕ ДАННЫЕ

### Тестовые пользователи
| Логин | Пароль | Роль |
|-------|--------|------|
| admin@automaster.tj | Admin123! | Admin |
| manager@automaster.tj | Manager123! | Manager |
| mechanic@automaster.tj | Mechanic123! | Mechanic |
| accountant@automaster.tj | Account123! | Accountant |

### Тестовые данные для seed
- 10 клиентов (с таджикскими именами)
- 15 автомобилей (Toyota, Chevrolet, Hyundai)
- 5 мастеров (разные специализации)
- 20 заказов (разные статусы)
- 30 позиций склада
- 50 транзакций за 3 месяца

---

## 14. ДОРОЖНАЯ КАРТА УЛУЧШЕНИЙ (ROADMAP)

> **Статус текущей версии:** AutoMaster CRM v1 реализован как **frontend-прототип на `localStorage`** (single-file `AutoMaster_CRM_v3_GOLD.html`).
> Данный раздел фиксирует план развития до полноценной системы (v2 — переход на REST API + PostgreSQL, описанный в разделах 4–8).
> Улучшения сгруппированы по приоритету: 🔴 обязательные → 🔵 стратегические.

---

### 14.0 Связка «ТЗ ↔ реализация» (важно для приёмки)

Чтобы исключить расхождение между ТЗ и фактическим прототипом, в документе и защите явно фиксируется двухэтапность:

| Версия | Хранение данных | Авторизация | Статус |
|--------|-----------------|-------------|--------|
| **v1 (текущая)** | `localStorage` браузера | Локальные роли (без сервера) | Прототип / демо |
| **v2 (целевая)** | PostgreSQL + EF Core | JWT (Access+Refresh), см. разд. 6 | Продакшн |

Все API-эндпоинты из разделов 3–5 относятся к **целевой архитектуре v2**. В v1 они эмулируются слоем `localStorage` (CRUD-обёртки в `services/`).

---

### 🔴 Уровень 1 — Обязательно (база, без этого проект «сырой»)

#### 14.1.1 Экран входа (Login) и реальные роли
**URL:** `/login`

- Страница логина: email + пароль (в v1 — выбор тестового пользователя из таблицы разд. 13).
- После входа — определение роли (`Admin / Manager / Mechanic / Accountant / Client`).
- **Скрытие пунктов меню и страниц по правам доступа** согласно матрице ролей (разд. 2 и 6):
  - `Mechanic` не видит Финансы, Аналитику, Маркетинг, Настройки, Филиалы.
  - `Accountant` видит только Финансы, Отчёты, Дашборд.
  - `Client` — только Кабинет клиента.
- Guard-обёртка маршрутов: `<ProtectedRoute roles={[...]}>` (React Router) — редирект на `/login`, если нет прав.
- Кнопка «Выйти» в Topbar, отображение текущего пользователя (имя + роль).

**Матрица доступа к страницам:**

| Страница | Admin | Manager | Mechanic | Accountant | Client |
|----------|:-----:|:-------:|:--------:|:----------:|:------:|
| Dashboard | ✅ | ✅ | — | ✅ | — |
| Orders / Kanban | ✅ | ✅ | ✅ (свои) | — | — |
| Customers / Vehicles | ✅ | ✅ | — | — | — |
| Warehouse | ✅ | ✅ | — | — | — |
| Finance / Reports | ✅ | — | — | ✅ | — |
| Analytics / Marketing / Branches / BI | ✅ | — | — | — | — |
| Operations | ✅ | ✅ | — | — | — |
| Client Portal | ✅ (симуляция) | — | — | — | ✅ |
| Super-Admin | ✅ | — | — | — | — |

#### 14.1.2 Валидация форм
Единый модуль `utils/validators.ts`. Проверки во всех модальных окнах:
- **Телефон:** маска и формат `+992 XX XXX XX XX` (9 цифр после кода страны).
- **Гос. номер:** uppercase, формат РТ (например, `1234 AB 01` / `01 1234 AB`), проверка длины.
- **Обязательные поля:** подсветка красным + сообщение под полем, блокировка кнопки сохранения.
- **Проверка дублей:** клиент по телефону, авто по гос. номеру / VIN, запчасть по SKU — предупреждение перед сохранением.
- **Числовые поля:** пробег, суммы, остатки ≥ 0; авто-пересчёт «Итого».
- **Email:** валидный формат (если заполнен).

---

### 🟠 Уровень 2 — Супер-админ / Управление системой

#### 14.2.1 Страница «Супер-админ»
**URL:** `/super-admin`
**Роли:** Admin

**Вкладки и функционал:**

1. **Пользователи (CRUD):**
   - Таблица: Имя / Email / Роль / Филиал / Статус / Последний вход / Действия.
   - Создание, редактирование, назначение/смена роли.
   - Кнопка «Заблокировать / Разблокировать пользователя» (`IsActive`).
2. **Аудит-лог системы:**
   - Таблица из `AuditLogs` (разд. 3.16, 4): Время / Пользователь / Действие / Объект / Детали.
   - Фильтр по пользователю, типу объекта, диапазону дат.
3. **Управление филиалами сети** — связь с разд. 3.17 (CRUD филиалов на уровне сети).
4. **Системные действия:**
   - «Сбросить демо-данные» (восстановление seed-набора из разд. 13).
   - «Импорт / Экспорт всей базы (JSON)» — полный дамп и восстановление.
   - «Войти как другой пользователь» (**impersonation**) — временная сессия под выбранным пользователем с баннером «Вы вошли как …» и кнопкой «Вернуться к админу».

**API (v2):**
- `GET/POST/PUT /api/admin/users`, `PATCH /api/admin/users/{id}/block`
- `POST /api/admin/users/{id}/impersonate`
- `GET /api/audit/logs` (фильтры)
- `POST /api/admin/reset-demo`, `GET /api/data/export`, `POST /api/data/import`

---

### 🟡 Уровень 3 — Автоматизация (фактор «вау»)

#### 14.3.1 Авто-смена статусов и SLA-таймеры
- Для каждого статуса заказа задаётся **норматив SLA** (часы). Конфигурируется в Настройках.
- Таймер в Kanban-карточке (разд. 3.3): зелёный → жёлтый → **красный** при превышении.
- При нарушении — авто-запись в таблицу «SLA Нарушения» (разд. 3.16, вкладка 3) + событие в колокольчик.

**Нормативы SLA по умолчанию (часы):**

| Статус | intake | diagnosis | estimate | approved | inprogress | waiting | ready |
|--------|:------:|:---------:|:--------:|:--------:|:----------:|:-------:|:-----:|
| SLA, ч | 2 | 4 | 8 | 6 | 24 | 48 | 12 |

#### 14.3.2 Авто-начисление лояльности и авто-транзакция
Реализация Сценария 3 (разд. 11) «по-настоящему»: при смене статуса заказа на `Completed` автоматически:
- создаётся транзакция «Доход» в Финансах (`Transactions`, привязка `OrderId`);
- начисляются баллы лояльности (`100 TJS = 1 балл`, разд. 3.7) с записью в `LoyaltyTransactions`;
- пересчитывается `TotalSpent` клиента и его статус (`regular / vip / corporate`);
- событие «Заказ завершён» уходит в колокольчик.

#### 14.3.3 Авто-алерты склада
- При `Quantity < MinQuantity` (разд. 3.9) — уведомление в колокольчик + бейдж «Низкий остаток».
- Предложение «Дозаказать» — формирование черновика заявки на закупку (список позиций ниже минимума).

#### 14.3.4 Predictive Maintenance (честная автоматизация, не «AI»)
Реальная формула вместо заглушки (разд. 3.12):
- Прогноз даты следующего ТО = `дата последнего ТО + интервал` **и/или** `пробег последнего ТО + 10 000 км`.
- Список «Кому пора на ТО»: сортировка по «дней/км до ТО», подсветка просроченных.
- Источник данных — `Vehicles.Mileage` + история заказов авто.

#### 14.3.5 Центр уведомлений (Notification Center)
- Единый «колокольчик» в Topbar со счётчиком непрочитанных.
- Типы событий: новый заказ, низкий остаток склада, нарушение SLA, заказ завершён, новая запись (booking), скоро ТО у авто.
- Список с фильтром по типу, отметка «прочитано», переход к связанному объекту по клику.

**API (v2):**
- `GET /api/notifications`, `PATCH /api/notifications/{id}/read`
- `GET /api/ai/predictive-maintenance` (реальная формула)
- `GET /api/orders/sla-violations`

---

### 🟢 Уровень 4 — Дизайн и UX

#### 14.4.1 Командная палитра (Ctrl+K)
- Глобальный поиск по приложению: страницы, заказы, клиенты, авто, запчасти.
- Быстрые команды: «Новый заказ», «Новый клиент», переход к любому разделу.
- Горячая клавиша `Ctrl+K` / `Cmd+K`, навигация стрелками + Enter.

#### 14.4.2 Адаптивность для телефона
- Гарантировать корректную работу на мобильном (< 900px, разд. 10):
  - Sidebar → выезжающее бургер-меню.
  - Таблицы → карточный режим (стек) либо горизонтальный скролл.
  - Модальные окна на всю ширину экрана.

#### 14.4.3 Скелетоны и анимации загрузки
- Skeleton-плейсхолдеры для таблиц, карточек и графиков вместо мгновенной отрисовки.
- Плавные переходы появления контента — ощущение «дорогого» продукта.

#### 14.4.4 Онбординг и пустые состояния (Empty States)
- Дружелюбные пустые состояния: «Заказов пока нет — создайте первый», с CTA-кнопкой.
- Подсказки для первого запуска (мини-онбординг на Дашборде).

---

### 🔵 Уровень 5 — Новое / стратегическое (отстройка от аналогов)

#### 14.5.1 Реальный AI через Claude API
Замена заглушки «AI Механик» (разд. 3.12) и «AI генерация текста» (разд. 3.13) на реальные запросы к **Claude API** (модель `claude-opus-4-8` или `claude-sonnet-4-6`).

- **Диагностика по симптомам:** структурированный промпт (марка/модель, пробег, описание проблемы) → вероятные неисправности, рекомендуемые работы, список запчастей, ориентир по стоимости.
- **Генерация маркетинговых текстов:** по каналу и сегменту (разд. 3.13) — готовый текст рассылки.
- **Архитектура:** запрос идёт **через backend** (ключ API хранится на сервере, не на фронте), эндпоинты `POST /api/ai/diagnose`, `POST /api/ai/generate-marketing-text`.
- В v1-прототипе — опциональный режим с пользовательским ключом, вводимым в Настройках (только для демо).

#### 14.5.2 PWA (Progressive Web App)
- Манифест + Service Worker → установка на телефон как приложение.
- Офлайн-работа (данные уже в `localStorage` — основа готова).
- Кэширование статики, иконка на рабочем столе.

#### 14.5.3 Telegram-бот для клиента
- Бот по гос. номеру авто возвращает текущий статус ремонта.
- Актуально для Душанбе: клиенту не нужен веб-кабинет — статус приходит в Telegram.
- Интеграция: webhook → `GET /api/client/order-status?plate=...`.

#### 14.5.4 QR-портал клиента (без логина)
- QR-код на воротах/стойке сервиса → публичная страница статуса авто.
- Клиент сканирует и видит статус своего заказа без авторизации.
- Защита токеном заказа: `/client/{token}/status` (разд. 3.15).
- Связка с модулем «QR Система» (разд. 3.14).

---

### 14.6 Приоритизация (сводка)

| Уровень | Что | Приоритет | Эффект |
|---------|-----|-----------|--------|
| 🔴 1 | Login + роли, валидация, связка ТЗ↔реализация | Обязательно | Базовая зрелость проекта |
| 🟠 2 | Супер-админ, аудит-лог, impersonation | Высокий | Полнота администрирования |
| 🟡 3 | SLA, авто-лояльность, алерты, уведомления | Высокий | Реальная автоматизация |
| 🟢 4 | Ctrl+K, адаптив, скелетоны, онбординг | Средний | Качество UX |
| 🔵 5 | Claude AI, PWA, Telegram, QR-портал | Стратегический | Отстройка / «вау» |

---

*Документ составлен на основе анализа AutoMaster_CRM_v3_GOLD.html*  
*Версия ТЗ: 1.1 (добавлен раздел 14 «Дорожная карта улучшений»)*  
*Дата: 2026-06-18*
