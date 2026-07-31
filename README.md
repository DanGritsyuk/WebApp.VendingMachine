# VendingMachine 2.0

Refactored vending machine platform with clean layered backend and separate frontend application.

## Projects
- `VendingMachine.WebAPI` — REST API (ASP.NET Core, JWT auth, API versioning).
- `VendingMachine.WebApp` — client/admin frontend (Blazor WebAssembly).
- `VendingMachine.BLL.Logic` + `VendingMachine.BLL.Logic.Contracts` — business use-cases and contracts.
- `VendingMachine.DAL.Repositories` + `VendingMachine.Repositories.Contracts` — persistence layer (PostgreSQL via EF Core).
- `VendingMachine.DAL.Cache.Redis` + `VendingMachine.DAL.Cache.Contracts` — cart/session cache in Redis.
- `VendingMachine.DAL.Storage` + `VendingMachine.DAL.Storage.Contracts` — image storage abstraction and providers.
- `VendingMachine.Common.Entities` — domain entities and enums.

## Architecture
Request flow:
`WebApp -> WebAPI -> BLL -> DAL (Repositories/Cache/Storage)`

Separation goals:
- predictable dependencies between layers;
- replaceable infrastructure (storage/cache/providers);
- compatibility mode for legacy import.

## Technology: Was / Became

| Area | Was (legacy) | Became (2.0) |
|---|---|---|
| UI | ASP.NET MVC + Razor Views | Blazor WebAssembly (`VendingMachine.WebApp`) |
| Backend | MVC controllers mixed with UI | Dedicated Web API (`VendingMachine.WebAPI`) |
| Auth | secret admin URL with query params | JWT authentication |
| Session/cart | ASP.NET Session | Redis-backed cart cache |
| Data access | tightly coupled app data logic | repository contracts + implementations |
| Image storage | local files only | provider model (`FileSystem`, `S3`, extensible) |
| Import/export | legacy-only format | modern flow + legacy compatibility |
| Architecture style | monolith | clean layered architecture |

## Quick Start

### 1. Prerequisites
- .NET SDK 8.0+
- PostgreSQL
- Redis

### 2. Configure API
Edit `VendingMachine.WebAPI/appsettings.json`:
- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:RedisConnection`
- `AuthOptions` (admin credentials, JWT settings)
- `Storage` (choose provider and options)

### 3. Run backend
```bash
dotnet run --project "VendingMachine.WebAPI/VendingMachine.WebAPI.csproj"
```
Default dev URLs: `https://localhost:7234`, `http://localhost:5126`.
Swagger: `https://localhost:7234/swagger`

### 4. Configure frontend
Edit `VendingMachine.WebApp/wwwroot/appsettings.json`:
- `Api:BaseUrl` (for example `https://localhost:7234/`)

### 5. Run frontend
```bash
dotnet run --project "VendingMachine.WebApp/VendingMachine.WebApp.csproj"
```
Default dev URLs: `https://localhost:7264`, `http://localhost:5021`.

## Main Features (2.0)
- drink catalog management (+ image upload);
- purchase flow (insert coins, choose drinks, checkout, change);
- admin authorization via JWT;
- import/export with legacy support;
- pluggable image storage.

## Notes
- Coin admin editing screen in frontend may require dedicated admin coin endpoints if not yet exposed by API.

Original project README is currently in the `master` branch.
