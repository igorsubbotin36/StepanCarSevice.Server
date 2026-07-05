# StepanCarSevice.Server

Мультитенантная платформа для автосервисов. Репозиторий содержит 4 микросервиса и общий слой с базовыми компонентами инфраструктуры.

## Архитектура и состав решения

### Микросервисы

| Сервис | Назначение | Статус | Базовый адрес (dev) |
| --- | --- | --- | --- |
| Auth | Авторизация и управление пользователями | ✅ готов | 
| Tenant | Управление тенантами (автосервисами) | ✅ готов | 
| Detail | Управление складом запчастей | 🚧 в работе | 
| Visit | Управление посещениями и услугами | 🚧 в работе | 

### Общие библиотеки (Core)

- `StepanCarService.Common.API` — базовые расширения для ASP.NET (Swagger, пайплайн, контроллеры).
- `StepanCarService.Common.Application` — общие модели, события и контракты.
- `StepanCarService.Common.Core` — базовые сущности и репозитории.
- `StepanCarService.Common.Infrastructure` — инфраструктура: EF Core, JWT, мульти-тенантность, RabbitMQ.

## Технологии

- .NET 9 (ASP.NET Core)
- EF Core + PostgreSQL
- Finbuckle.MultiTenant (Host Strategy)
- JWT (Bearer)
- RabbitMQ
- NLog
- Swagger/OpenAPI

## Мультитенантность

Используется **Host Strategy** от Finbuckle. Тенант определяется по имени хоста (subdomain), далее применяется общий пайплайн и проверки токена.

Ключевые правила:
- Если в токене присутствует `tenant_id`, он должен совпадать с тенантом запроса.
- Роль `GodMode` может работать без `tenant_id` и автоматически получать его из запроса.

## Обмен событиями (RabbitMQ)

- Tenant сервис публикует события о регистрации тенанта в exchange `tenant.events.exchange`.
- Остальные сервисы (Auth/Detail/Visit) подписываются на эти события через `TenantEventsConsumer`.

Настройки RabbitMQ указываются в `appsettings.Development.json` каждого сервиса.

## Быстрый старт

### Предварительные требования

- .NET 9 SDK
- PostgreSQL 14+
- RabbitMQ 3+

### 1. Клонирование

```bash
git clone <repo-url>
cd StepanCarSevice.Server
```

### 2. Конфигурация

Проверьте `appsettings.Development.json` в каждом сервисе:

- `ConnectionStrings:PostgreSQL`
- `Jwt` (Issuer, Audience, Key, LifetimeMinutes)
- `RabbitMQ` (HostName, Port, UserName, Password, VirtualHost, ExchangeName, QueueName)

### 3. Запуск БД и RabbitMQ

Пример запуска RabbitMQ через Docker:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 4. Запуск сервисов

Можно запустить каждый сервис отдельно:

```bash
# Auth
cd Services/Auth/StepanCarSevice.AuthService.API
dotnet run

# Tenant
cd Services/Tenant/StepanCarService.TenantService.API
dotnet run

# Detail
cd Services/Detail/StepanCarSevice.DetailService.API
dotnet run

# Visit
cd Services/Visit/StepanCarSevice.VisitService.API
dotnet run
```

После старта Swagger доступен по корню сервиса (например, http://localhost:5186/).

## API (основные эндпоинты)

### Auth

- `POST /api/auth/register` — регистрация пользователя
- `POST /api/auth/login` — получение токена
- `GET /api/auth/getTokenClaims` — проверка текущих claims (Bearer)

### Tenant

- `POST /api/tenant/registerTenant` — создание тенанта (TenantOwner/GodMode)
- `PATCH /api/tenant/updateTenant` — обновление тенанта (TenantOwner/GodMode)
- `DELETE /api/tenant/deleteTenant?id=...` — удаление тенанта (TenantOwner/GodMode)
- `GET /api/tenant/getAllTenants` — список всех (GodMode)
- `GET /api/tenant/getTenantById?id=...` — получение по ID

### Detail

- `POST /api/detail/addDetail` — добавить запчасть
- `GET /api/detail/getAllDetails` — список
- `GET /api/detail/getDetailById` — по ID
- `GET /api/detail/getDetailsByCode` — по артикулу
- `PATCH /api/detail/editDetail` — обновление

### Visit

Пока без публичных эндпоинтов (сервис в разработке).

## Роли и доступы

| Роль | Описание |
| --- | --- |
| GodMode | Администратор платформы, доступ ко всем тенантам |
| TenantOwner | Владелец автосервиса |
| TenantModerator | Модератор/администратор автосервиса |
| User | Сотрудник/мастер |

Политики доступа описаны в `StepanCarService.Common.Infrastructure`.

## Миграции БД

При запуске каждого сервиса выполняется `MigrateDatabaseAsync()` и применение миграций.

Для Auth и Tenant миграции готовы; Detail и Visit находятся в процессе доработки.

## Структура репозитория

```
StepanCarSevice.Server.sln
Core/
  StepanCarService.Common.API/
  StepanCarService.Common.Application/
  StepanCarService.Common.Core/
  StepanCarService.Common.Infrastructure/
Services/
  Auth/
  Tenant/
  Detail/
  Visit/
```

## Статус разработки

- ✅ Auth — готов
- ✅ Tenant — готов
- 🚧 Detail — частично готов
- 🚧 Visit — в разработке