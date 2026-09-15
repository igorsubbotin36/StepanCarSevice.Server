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

Несекретные настройки RabbitMQ (хост, exchange, очередь) указываются в `appsettings.Development.json` каждого сервиса, логин и пароль — в user-secrets (см. «Конфигурация»).

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

Несекретные настройки лежат в `appsettings.Development.json` каждого сервиса:

- `Jwt` (Issuer, Audience, LifetimeMinutes)
- `RabbitMQ` (HostName, Port, VirtualHost, ExchangeName, QueueName)
- `AllowedHosts` — разрешённые значения заголовка `Host` (в Development: `localhost;*.localhost;127.0.0.1`)
- `RateLimiting:Auth` (необязательно) — лимит на вход, регистрацию и смену пароля с одного IP: `PermitLimit` (по умолчанию 10) за `WindowSeconds` (по умолчанию 60)
- `RateLimiting:Public` (необязательно) — лимит на публичные эндпоинты без авторизации (каталог автосервисов): `PermitLimit` (по умолчанию 60) за `WindowSeconds` (по умолчанию 60)

Вне Development `AllowedHosts` обязателен и должен содержать явный список доменов, например `example.com;*.example.com` (переменная окружения `AllowedHosts`). Со значением `*` или без него сервис не запустится: тенант определяется по заголовку `Host`.

Если сервисы стоят за обратным прокси, нужно настроить `ForwardedHeaders`, иначе лимит запросов будет считаться для IP прокси, а не клиентов.

**Секреты в репозиторий не коммитятся.** Локально они хранятся в [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), в окружениях — в переменных окружения (`ConnectionStrings__PostgreSQL`, `Jwt__Key`, `RabbitMQ__UserName`, `RabbitMQ__Password`).

`Jwt:Key` должен быть **одинаковым во всех сервисах** и не короче 32 байт — иначе сервис не запустится. Сгенерировать ключ (PowerShell):

```powershell
$b = New-Object byte[] 64; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

Для каждого API-проекта (`Services/Auth/StepanCarSevice.AuthService.API`, `Services/Tenant/StepanCarService.TenantService.API`, `Services/Detail/StepanCarSevice.DetailService.API`, `Services/Visit/StepanCarSevice.VisitService.API`) выполните, подставив свои значения и имя БД сервиса:

```bash
dotnet user-secrets set "ConnectionStrings:PostgreSQL" "host=localhost;port=5432;database=AuthService;User Id=postgres;password=<пароль>" --project <путь к API-проекту>
dotnet user-secrets set "Jwt:Key" "<сгенерированный ключ>" --project <путь к API-проекту>
dotnet user-secrets set "RabbitMQ:UserName" "<логин>" --project <путь к API-проекту>
dotnet user-secrets set "RabbitMQ:Password" "<пароль>" --project <путь к API-проекту>
```

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

Non-secret RabbitMQ settings (host, exchange, queue) are defined in each service's `appsettings.Development.json`; username and password are stored in user-secrets (see "Configuration").

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

Non-secret settings live in each service's `appsettings.Development.json`:

- `Jwt` (Issuer, Audience, LifetimeMinutes)
- `RabbitMQ` (HostName, Port, VirtualHost, ExchangeName, QueueName)
- `AllowedHosts` — permitted `Host` header values (Development: `localhost;*.localhost;127.0.0.1`)
- `RateLimiting:Auth` (optional) — per-IP limit for login, registration and password change: `PermitLimit` (default 10) per `WindowSeconds` (default 60)
- `RateLimiting:Public` (optional) — per-IP limit for anonymous endpoints (connected car services catalog): `PermitLimit` (default 60) per `WindowSeconds` (default 60)

Outside Development `AllowedHosts` is required and must list explicit domains, e.g. `example.com;*.example.com` (environment variable `AllowedHosts`). With `*` or no value the service won't start, because the tenant is resolved from the `Host` header.

Behind a reverse proxy configure `ForwardedHeaders`, otherwise the rate limit is counted for the proxy IP instead of clients.

**Secrets are never committed.** Locally they are kept in [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets); in deployed environments use environment variables (`ConnectionStrings__PostgreSQL`, `Jwt__Key`, `RabbitMQ__UserName`, `RabbitMQ__Password`).

`Jwt:Key` must be **the same in all services** and at least 32 bytes long, otherwise the service won't start. Generate a key (PowerShell):

```powershell
$b = New-Object byte[] 64; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

For each API project (`Services/Auth/StepanCarSevice.AuthService.API`, `Services/Tenant/StepanCarService.TenantService.API`, `Services/Detail/StepanCarSevice.DetailService.API`, `Services/Visit/StepanCarSevice.VisitService.API`) run, substituting your values and the service's database name:

```bash
dotnet user-secrets set "ConnectionStrings:PostgreSQL" "host=localhost;port=5432;database=AuthService;User Id=postgres;password=<password>" --project <path to API project>
dotnet user-secrets set "Jwt:Key" "<generated key>" --project <path to API project>
dotnet user-secrets set "RabbitMQ:UserName" "<username>" --project <path to API project>
dotnet user-secrets set "RabbitMQ:Password" "<password>" --project <path to API project>
```

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


