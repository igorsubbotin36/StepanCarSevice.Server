# StepanCarSevice.Server

Мультитенантная платформа для автосервисов. Репозиторий содержит 4 микросервиса и общий слой с базовыми компонентами инфраструктуры.

**Languages:** [Русский](#russian) | [English](#english)

---

# Russian

## Архитектура и состав решения

Каждый микросервис реализован по принципам **Clean Architecture**:

- **API** — контроллеры, настройка пайплайна, Swagger/OpenAPI.
- **Application** — сценарии/сервисы, DTO, бизнес-кейсы, интерфейсы.
- **Domain/Core** — сущности и бизнес-правила.
- **Infrastructure** — EF Core, внешние интеграции (RabbitMQ, JWT), реализации репозиториев.

Общие зависимости вынесены в `Core` и переиспользуются всеми сервисами.

### Микросервисы

| Сервис | Назначение | Статус | Базовый адрес (dev) |
| --- | --- | --- | --- |
| Auth | Авторизация и управление пользователями | ✅ готов | http://localhost:5186 |
| Tenant | Управление тенантами (автосервисами) | ✅ готов | http://localhost:5010 |
| Detail | Управление складом запчастей | 🚧 в работе | http://localhost:5034 |
| Visit | Управление посещениями и услугами | 🚧 в работе | http://localhost:5183 |

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

---

# English

## Architecture and Solution Overview

Each microservice follows **Clean Architecture** principles:

- **API** — controllers, HTTP pipeline, Swagger/OpenAPI.
- **Application** — use cases/services, DTOs, contracts.
- **Domain/Core** — business entities and rules.
- **Infrastructure** — EF Core, external integrations (RabbitMQ, JWT), repository implementations.

Shared building blocks live in `Core` and are reused by all services.

### Microservices

| Service | Purpose | Status | Base URL (dev) |
| --- | --- | --- | --- |
| Auth | Authentication and user management | ✅ ready | http://localhost:5186 |
| Tenant | Tenant (workshop) management | ✅ ready | http://localhost:5010 |
| Detail | Parts inventory management | 🚧 in progress | http://localhost:5034 |
| Visit | Visits and services management | 🚧 in progress | http://localhost:5183 |

### Shared Libraries (Core)

- `StepanCarService.Common.API` — ASP.NET extensions (Swagger, pipeline, controllers).
- `StepanCarService.Common.Application` — shared models, events, contracts.
- `StepanCarService.Common.Core` — base entities and repositories.
- `StepanCarService.Common.Infrastructure` — infrastructure: EF Core, JWT, multi-tenancy, RabbitMQ.

## Tech Stack

- .NET 9 (ASP.NET Core)
- EF Core + PostgreSQL
- Finbuckle.MultiTenant (Host Strategy)
- JWT (Bearer)
- RabbitMQ
- NLog
- Swagger/OpenAPI

## Multi-tenancy

Uses **Host Strategy** from Finbuckle. Tenant is resolved from the host (subdomain), then the shared pipeline and token checks are applied.

Key rules:
- If the token contains `tenant_id`, it must match the request tenant.
- `GodMode` can work without `tenant_id` and will receive it from the request automatically.

## Event Bus (RabbitMQ)

- Tenant service publishes tenant registration events to `tenant.events.exchange`.
- Other services (Auth/Detail/Visit) subscribe via `TenantEventsConsumer`.

RabbitMQ settings are defined in each service's `appsettings.Development.json`.

## Quick Start

### Prerequisites

- .NET 9 SDK
- PostgreSQL 14+
- RabbitMQ 3+

### 1. Clone

```bash
git clone <repo-url>
cd StepanCarSevice.Server
```

### 2. Configuration

Check `appsettings.Development.json` in each service:

- `ConnectionStrings:PostgreSQL`
- `Jwt` (Issuer, Audience, Key, LifetimeMinutes)
- `RabbitMQ` (HostName, Port, UserName, Password, VirtualHost, ExchangeName, QueueName)

### 3. Start DB and RabbitMQ

Example RabbitMQ Docker run:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 4. Run services

You can run each service separately:

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

Swagger UI is available at the service root (e.g., http://localhost:5186/).

## API (core endpoints)

### Auth

- `POST /api/auth/register` — register user
- `POST /api/auth/login` — get token
- `GET /api/auth/getTokenClaims` — check current claims (Bearer)

### Tenant

- `POST /api/tenant/registerTenant` — create tenant (TenantOwner/GodMode)
- `PATCH /api/tenant/updateTenant` — update tenant (TenantOwner/GodMode)
- `DELETE /api/tenant/deleteTenant?id=...` — delete tenant (TenantOwner/GodMode)
- `GET /api/tenant/getAllTenants` — list all (GodMode)
- `GET /api/tenant/getTenantById?id=...` — get by ID

### Detail

- `POST /api/detail/addDetail` — add part
- `GET /api/detail/getAllDetails` — list
- `GET /api/detail/getDetailById` — by ID
- `GET /api/detail/getDetailsByCode` — by code
- `PATCH /api/detail/editDetail` — update

### Visit

No public endpoints yet (service under development).

## Roles and Access

| Role | Description |
| --- | --- |
| GodMode | Platform admin, access to all tenants |
| TenantOwner | Workshop owner |
| TenantModerator | Workshop admin/moderator |
| User | Employee/mechanic |

Access policies are defined in `StepanCarService.Common.Infrastructure`.

## Database migrations

Each service runs `MigrateDatabaseAsync()` on startup and applies migrations.

Auth and Tenant migrations are ready; Detail and Visit are still being completed.

## Repository Structure

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

## Development Status

- ✅ Auth — ready
- ✅ Tenant — ready
- 🚧 Detail — partially ready
- 🚧 Visit — under development

## License

MIT (replace before publishing).
