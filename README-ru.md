# VendingMachine 2.0

Обновленная платформа торгового автомата: разделенный backend по слоям и отдельное frontend-приложение.

## Состав решения
- `VendingMachine.WebAPI` — REST API (ASP.NET Core, JWT, versioning).
- `VendingMachine.WebApp` — клиентская и админская часть (Blazor WebAssembly).
- `VendingMachine.BLL.Logic` + `VendingMachine.BLL.Logic.Contracts` — бизнес-логика и контракты.
- `VendingMachine.DAL.Repositories` + `VendingMachine.Repositories.Contracts` — доступ к данным (PostgreSQL, EF Core).
- `VendingMachine.DAL.Cache.Redis` + `VendingMachine.DAL.Cache.Contracts` — Redis-кэш корзин.
- `VendingMachine.DAL.Storage` + `VendingMachine.DAL.Storage.Contracts` — абстракция и провайдеры хранилища изображений.
- `VendingMachine.Common.Entities` — доменные сущности.

## Архитектура
Поток запроса:
`WebApp -> WebAPI -> BLL -> DAL (Repositories/Cache/Storage)`

Цели разделения:
- управляемые зависимости между слоями;
- замена инфраструктуры без переписывания бизнес-логики;
- поддержка legacy-импорта.

## Технологии: Было / Стало

| Область | Было (legacy) | Стало (2.0) |
|---|---|---|
| UI | ASP.NET MVC + Razor Views | Blazor WebAssembly (`VendingMachine.WebApp`) |
| Backend | MVC-контроллеры вместе с UI | Отдельный Web API (`VendingMachine.WebAPI`) |
| Авторизация | «секретная» ссылка в query | JWT-токены |
| Корзина/сессии | ASP.NET Session | Redis-кэш |
| Доступ к данным | тесно связанная логика в приложении | репозитории через контракты |
| Хранение изображений | только локальная ФС | провайдерная модель (`FileSystem`, `S3`, расширяемо) |
| Импорт/экспорт | только legacy-формат | современный формат + legacy-совместимость |
| Архитектурный стиль | монолит | clean layered architecture |

## Быстрый старт

### 1. Требования
- .NET SDK 8.0+
- PostgreSQL
- Redis

### 2. Настройка API
Отредактируйте `VendingMachine.WebAPI/appsettings.json`:
- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:RedisConnection`
- `AuthOptions` (учетка администратора и JWT)
- `Storage` (провайдер и его параметры)

### 3. Запуск backend
```bash
dotnet run --project "VendingMachine.WebAPI/VendingMachine.WebAPI.csproj"
```
Dev URL: `https://localhost:7234`, `http://localhost:5126`.
Swagger: `https://localhost:7234/swagger`

### 4. Настройка frontend
Отредактируйте `VendingMachine.WebApp/wwwroot/appsettings.json`:
- `Api:BaseUrl` (например `https://localhost:7234/`)

### 5. Запуск frontend
```bash
dotnet run --project "VendingMachine.WebApp/VendingMachine.WebApp.csproj"
```
Dev URL: `https://localhost:7264`, `http://localhost:5021`.

## Ключевые возможности 2.0
- управление напитками (включая загрузку изображения);
- покупка: внесение монет, заказ, чек, сдача;
- админ-доступ через JWT;
- импорт/экспорт с поддержкой legacy;
- переключаемые хранилища изображений.

## Примечания
- Для полного редактирования монет через админку может потребоваться отдельный admin endpoint в API (если ещё не добавлен).

Оригинальный README проекта сейчас находится в ветке `master`.
