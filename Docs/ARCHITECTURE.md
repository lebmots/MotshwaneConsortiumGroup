# Architecture

## Layers

Each layer only talks to the layer directly below it.

```
Views (.cshtml)                    Front end
   |
Controllers (MVC pages, later API) Front end (MVC) / Database & API (API controllers)
   |
Services (business logic)          Backend & hosting
   |
Data access (EF Core, DbContext)   Database & API
   |
SQL Server (Azure SQL)
```

## Rules

1. **Controllers depend on service interfaces only** (`IBookingService`, `IStaffJobService`, ...). Never on `DemoDataService` or a `DbContext`.
2. **Business rules live in services**, not in controllers or views. Controllers validate input, call a service and choose the view or response.
3. **Services never touch HTTP** (`HttpContext`, `TempData`, views). That keeps them testable with xUnit.
4. **All service registration happens in one place:** `Extensions/ServiceCollectionExtensions.cs`.
5. **Service methods are async** (`Task<...>`), because the EF Core versions will be async.
6. **Services are registered as Scoped** (one instance per web request), which is what EF Core needs.

## Folder ownership

| Folder | Contents | Owner |
|---|---|---|
| `Views/`, `wwwroot/` | Pages, CSS | Front end |
| `Controllers/` | MVC controllers | Front end |
| `Services/Interfaces/` | Service contracts | Backend & hosting |
| `Services/InMemory/` | Temporary implementations over `DemoDataService` | Backend & hosting |
| `Extensions/` | DI registration | Backend & hosting |
| `Models/`, `Data/` | Entities, `DbContext`, migrations | Database & API |
| `Api/` (later) | REST API controllers, Swagger | Database & API |
| `Docs/` | Documentation | Everyone |

If you need to change a file outside your folders, tell its owner first and keep the change small.

## Moving from demo data to the database

1. The Database & API owner adds the `DbContext`, entities and migrations.
2. The Backend & hosting owner writes `EfBookingService`, `EfStaffJobService`, etc. Each implements the same interface as its `InMemory` version and receives the `DbContext` through its constructor.
3. Swap the registrations in `AddApplicationServices`. Controllers and views do not change.
4. Delete `DemoDataService` and the `InMemory` folder.

## To agree before Week 3

- Final entity names and properties (a unit or inventory entity is needed for availability checks, and the current model has none).
- Allowed job statuses and the order they can change in.
- Whether the API controllers call the same services (recommended) or have their own logic.
