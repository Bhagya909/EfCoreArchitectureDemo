# EfCoreArchitectureDemo

## Current Project Snapshot

EfCoreArchitectureDemo is a layered .NET 10 retail management API. The current solution is split into `API`, `Application`, `Domain`, `Infrastructure`, and `RetailProject.Tests`. It demonstrates Clean Architecture, EF Core persistence, SQL Server migrations, optimistic concurrency, transaction-safe retail workflows, audit logging, AI-assisted log enrichment, and API documentation through Swagger.

The main business areas are:

| Area | Current implementation |
| --- | --- |
| Catalog | Products, categories, product-category assignment, soft delete, restore, and bulk price/archive operations. |
| Inventory | Stock creation, stock validation, stock deduction, stock movement history, low-stock/out-of-stock views, and inventory summary. |
| Customers | Customer creation, update, soft delete, lookup, and customer order history. |
| Orders | Order creation, listing, lookup, cancellation, payment-state validation, and final completion. |
| Payments | Payment completion, stock deduction coordination, payment lookup, and payment failure logging. |
| Observability | Business change logs, automatic EF audit logs, AI summary status, background AI enrichment, and upgrade execution logs. |

## Architecture

The dependency direction is intentionally one-way: `Domain` contains the business model, `Application` defines use cases and ports around that model, `Infrastructure` implements persistence and external integrations for those ports, `API` hosts HTTP endpoints and composition root wiring, and `RetailProject.Tests` verifies behavior through integration and workflow coverage. Outer layers may depend on inner layers, but inner layers do not depend on outer delivery or persistence details.

| Project | Responsibility |
| --- | --- |
| `Domain` | Business entities, enums, and invariants. |
| `Application` | Use-case services, DTOs, validators, repository/service interfaces, and query models. |
| `Infrastructure` | EF Core DbContext, entity mappings, migrations, repositories, unit of work, data upgrades, and external services. |
| `API` | HTTP controllers, middleware, Swagger, startup configuration, dependency injection, and hosting. |
| `RetailProject.Tests` | Integration tests for persistence, API workflows, migrations, upgrades, and business behavior. |

EF Core stays in `Infrastructure` because database mapping, migrations, and provider-specific behavior are implementation details. `Application` works through interfaces and DTOs, so it can express business workflows without knowing whether data comes from SQL Server, SQLite tests, or another persistence implementation. It also has no HTTP dependency: controllers translate requests into DTOs, services execute use cases, and responses are mapped back at the boundary.

## API and Swagger

Swagger is enabled by the `API` project through Swashbuckle. The root endpoint redirects to `/swagger`, XML comments are included when generated, and endpoint groups are controlled with controller-level `[Tags(...)]` attributes.

| Swagger group | Route root | Controller |
| --- | --- | --- |
| `Catalog - Products` | `/api/products` | `ProductController` |
| `Catalog - Categories` | `/api/categories` | `CategoryController` |
| `Inventory` | `/api/inventory` | `InventoryController` |
| `Customers` | `/api/customers` | `CustomerController` |
| `Orders` | `/api/orders` | `OrderController` |
| `Payments` | `/api/payments` | `PaymentController` |

The current API surface includes product CRUD, product category assignment, category CRUD, category product listing, inventory stock operations, customer workflows, order workflows, payment completion, and payment lookup. Errors are normalized by `ExceptionMiddleware` into `ProblemDetails` responses.

## Database Setup

### Prerequisites

- .NET SDK installed.
- SQL Server or SQL Server LocalDB available.
- EF Core CLI installed: `dotnet tool install --global dotnet-ef`
- A valid `DefaultConnection` connection string in `API/appsettings.json`.

### Connection String

The API and EF Core design-time factory both use the `DefaultConnection` key:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RetailProjectDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Use your local SQL Server instance name if it differs from LocalDB.

### Apply Migrations

Run this from the solution root:

```powershell
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

The `--project` flag points to the project that owns the migrations. The `--startup-project` flag points to the API project that provides runtime configuration.

### Migration Summary

| Migration | What it added or changed | Why |
| --- | --- | --- |
| `20260509151033_InitialCreate` | Created the initial catalog, inventory, order, payment, customer, category, product-category, and change-log tables with indexes, relationships, decimal precision, soft-delete flags, and rowversion columns. | Establishes the first complete retail schema. |
| `20260517081618_AddPaymentReferenceNumber` | Added nullable `Payments.ReferenceNumber`, temporarily added `InventoryTransactions.InventoryId`, and changed inventory-product delete behavior to restrict. | Supports payment reference tracking and begins refining inventory relationships. |
| `20260518071606_RemoveInventoryTransactionInventoryRelation` | Removed the temporary `InventoryTransactions.InventoryId` relationship and index. | Keeps inventory transactions tied to products without an unnecessary direct inventory dependency. |
| `20260519062119_EnhanceChangeLogStructure` | Added change-log category, severity, changed fields, old values, and new values. | Makes audit records more useful for explaining business changes. |
| `20260521063508_RefactorChangeLogForAiObservability` | Replaced `Summary` with AI-specific summary fields, added correlation/source/status fields, added related indexes, and changed product SKU uniqueness to ignore soft-deleted rows. | Separates AI observability from base audit data and supports soft-delete restore/recreate workflows. |
| `20260521071452_AddAiSummaryErrorColumn` | Added `ChangeLogs.AiSummaryError`. | Preserves AI enrichment failure details without losing the base audit log. |
| `20260525041919_UpdateCustomerEmailFilteredIndex` | Changed customer email uniqueness to filter out soft-deleted rows. | Allows a deleted customer email to be reused while active customers remain unique. |
| `20260525123452_AddCategoryFilteredUniqueIndex` | Changed category name uniqueness to filter out soft-deleted rows. | Allows a deleted category name to be reused while active categories remain unique. |

### Startup Auto-Migration

On application startup, `API/Program.cs` runs `Database.MigrateAsync()` and then `UpgradeRunner.RunUpgradesAsync()` for every environment except `Testing`. This is intentional for the demo: a new or outdated database is brought to the current schema before the API starts serving normal requests. Integration tests skip this path and control database creation explicitly.

### Fresh Empty Database

To start from an empty database, create or point `DefaultConnection` at a database that does not yet exist, then run:

```powershell
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

EF Core creates the database if needed and applies every migration in timestamp order. The integration test `MigrationTests.AllMigrations_ApplyCleanly_ToFreshDatabase` is the proof test for this path because it creates a fresh SQL Server LocalDB database, applies all migrations, verifies no migrations remain pending, and checks key additive columns.

## Database Upgrade Pipeline

`UpgradeRunner` runs registered `IDataUpgrade` implementations after schema migrations have completed. Each upgrade exposes a stable `Name`, and the runner processes the registered upgrades in dependency-injection order.

Idempotency is enforced through `ChangeLogs`. Before an upgrade runs, `UpgradeRunner` checks for a log entry with `ActionType = "UPGRADE_EXECUTED"` and `RawData = "Upgrade={upgrade.Name}"`. If that entry exists, the upgrade is skipped. After a successful run, the runner writes that log entry in the same transaction as the upgrade work.

`GeneratePaymentReferenceNumbersUpgrade` is the first concrete data-upgrade example. Before it runs, older payment rows can have `ReferenceNumber = null`. The upgrade loads those payments and sets deterministic values such as `PAY-000123` from the payment id. After it runs, those rows have reference numbers, and a `ChangeLogs` record marks the upgrade as executed. Running the pipeline again does not change already-upgraded rows because the execution log causes the upgrade to be skipped.

`BackfillAiSummaryStatusUpgrade` is the second upgrade. Its stable `Name` is `BackfillAiSummaryStatus`, and it backfills legacy change-log rows that already have an AI summary but still have `AiSummaryStatus = NotRequested`. It uses `ExecuteUpdateAsync()` so the backfill is performed as a set-based database update. The upgrade is idempotent because it only touches rows that still match the legacy state, and the runner also records one `UPGRADE_EXECUTED` marker for the upgrade name.

Upgrade execution order is controlled by dependency-injection registration order. `GeneratePaymentReferenceNumbersUpgrade` is registered before `BackfillAiSummaryStatusUpgrade`, and `UpgradeRunner` iterates the registered `IDataUpgrade` implementations in that order.

Each upgrade runs inside a unit-of-work transaction. If the upgrade changes multiple rows and then fails, `UpgradeRunner` rolls the transaction back and rethrows the error. If it succeeds, the data changes and the `UPGRADE_EXECUTED` log are committed together.

Upgrade tests that mix raw SQL with EF-tracked entities must clear the EF change tracker before asserting refreshed state. For example, the `BackfillAiSummaryStatus` test uses raw SQL to create legacy database state, then calls `ChangeTracker.Clear()` so EF does not return stale in-memory values after the upgrade runs.

## Application Layer

| Service | Use case ownership |
| --- | --- |
| `ProductService` | Product creation, updates, soft delete, restore, bulk price changes, archive operations, and product queries. |
| `CategoryService` | Category lifecycle, product-category assignment, category product queries, and category-based archive operations. |
| `InventoryService` | Inventory creation, stock validation, stock deduction, transaction history, low-stock views, and inventory summaries. |
| `OrderService` | Order creation, order queries, order cancellation, order completion, and coordination with inventory stock checks. |
| `PaymentService` | Payment completion, payment lookup, order payment status changes, and payment reference handling. |
| `CustomerService` | Customer creation, updates, soft delete, customer lookup, and customer order history. |
| `ChangeLogService` | Audit log creation, audit queries, AI status views, summaries, and bulk audit cleanup. |

Validation is split by responsibility. Validators catch request/input shape problems such as missing names, invalid ids, negative quantities, and malformed email values. Domain entities protect invariants that must always hold, such as positive prices, valid order totals, valid payment amounts, and legal status transitions. Services enforce business rules that require data access or workflow context, such as duplicate SKU checks, insufficient stock, missing related records, existing category assignments, and payment/order consistency.

| Exception type | Meaning | HTTP status |
| --- | --- | --- |
| `KeyNotFoundException` | A requested resource does not exist. | `404 Not Found` |
| `ArgumentException` | The request shape or supplied value is invalid. | `400 Bad Request` |
| `ConcurrencyConflictException` | A rowversion-protected record was changed by another request before the current write completed. | `409 Conflict` |
| `InvalidOperationException` | The request conflicts with current business state, including duplicate data, invalid workflow transitions, or insufficient stock. | `409 Conflict` |
| Any other `Exception` | Unexpected server failure. | `500 Internal Server Error` |

DTOs are the application boundary. Controllers receive and return DTOs, services map DTOs to domain operations, and domain entities stay inside the service layer. This keeps persistence navigation properties, rowversion fields, soft-delete internals, and domain behavior from leaking directly into HTTP contracts.

## Retail Workflow

The main business flow is: create a product, add inventory for that product, create a customer, create an order for the customer, complete payment for the order, then complete the order. Product, customer, and read operations are independent use cases; inventory addition opens its own transaction because it changes inventory, writes an inventory transaction, and writes an audit log together. Order creation validates products and stock first, then opens a transaction to create the order, order items, and order audit log. Payment completion owns the payment transaction: it loads the order, deducts stock for each order item, creates the payment, marks the payment completed, marks the order paid, writes inventory/payment audit logs, and commits all of those changes together.

Final order completion is a separate use case exposed by `PUT /api/orders/{id}/complete`. `OrderService.CompleteOrderAsync()` loads the order and its payment, requires `Payment.Status == Completed`, delegates the state transition to `Order.MarkAsCompleted()`, writes an `ORDER_COMPLETED` log, and commits through `IUnitOfWork`. The endpoint returns `200 OK` with `OrderResponseDto` on success, `404 Not Found` when the order does not exist, and `409 Conflict` when the payment is not completed or a concurrency conflict occurs.

`InventoryTransaction` is encapsulated as a domain record: public setters are private, quantity changes cannot be zero, transaction type must be a defined enum value, `In` transactions must be positive, and `Out` transactions must be negative. This keeps stock movement history valid regardless of whether it is created from inventory addition or stock deduction.

If inventory addition fails, the inventory row, inventory transaction, and audit log are rolled back. If order creation fails after its transaction opens, the order, order items, and audit log are rolled back. If payment completion fails, stock deductions, inventory transactions, payment creation, order status changes, and payment audit logging are rolled back together. After rollback, `PaymentService` clears the change tracker and writes a `PAYMENT_FAILED` log outside the aborted transaction, so the failure is observable without committing partial payment state. Inventory validation happens before the order transaction opens because it is a read-only precondition check and avoids holding a database transaction while rejecting an order that cannot be fulfilled.

## Transaction Boundaries

`IUnitOfWork` owns transaction boundaries and exposes `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()`, `SaveChangesAsync()`, and `ClearChanges()`. `ClearChanges()` calls `DbContext.ChangeTracker.Clear()` through the infrastructure implementation. It exists for rollback paths where EF may still be tracking entities mutated inside an aborted transaction; clearing those tracked entities prevents stale in-memory state from leaking into the next save, such as a failure log written after rollback.

## Optimistic Concurrency

`Product`, `Inventory`, `Order`, and `Payment` have `RowVersion` columns because they are mutable business records where lost updates would matter: product price/details, stock quantity, order status, and payment status should not silently overwrite concurrent changes. EF Core maps those properties with `IsRowVersion()`, so SQL Server checks the original rowversion during updates and raises `DbUpdateConcurrencyException` when another write has already changed the row.

`UnitOfWork.SaveChangesAsync()` catches `DbUpdateConcurrencyException` and rethrows it as a dedicated `ConcurrencyConflictException` with the message `A concurrency conflict occurred. The record was modified by another request.` This keeps concurrency failures distinct from normal business-rule conflicts.

`ExceptionMiddleware` maps `ConcurrencyConflictException` directly to `409 Conflict` and returns a `ProblemDetails` response. This is separate from the generic `InvalidOperationException` path, which also maps to `409 Conflict` but represents ordinary workflow conflicts such as duplicate data, invalid transitions, or insufficient stock.

Product updates round-trip concurrency tokens through DTOs. `ProductResponseDto` returns the current `RowVersion`, and `UpdateProductDto` requires the client to send that `RowVersion` back with the update. `ProductService` validates that the token is present and calls `product.SetRowVersion(dto.RowVersion)` before saving, so EF Core can compare the client's version with the current database row.

Payment and inventory concurrency conflicts use a fail-fast strategy. The service rolls back the active transaction, clears tracked changes, writes an observable failure log where appropriate, and rethrows the concurrency exception so the API returns `409 Conflict`. Automatic retry was intentionally removed because retrying a payment or stock mutation inside the service could hide stale client state and repeat side effects. Clients should retry by reloading current state and submitting a fresh command.

`RowVersion` concurrency behavior is SQL Server specific. SQLite integration tests cover the workflow shape and DTO/service behavior, but they do not prove SQL Server rowversion conflict detection; that behavior is covered by the production SQL Server mapping.

## AI Observability

`AuditSaveChangesInterceptor` captures persistence facts during `SaveChangesAsync`: entity name, action type, changed fields, old values, new values, soft-delete detection, severity, category, correlation id, and raw audit data. It does not call AI services. Its job is fact capture only, so database writes are not blocked on external AI latency.

`AiEnrichmentBackgroundService` runs after commits in a hosted background loop. Every cycle it creates a scope, resolves `IAiEnrichmentProcessor`, loads pending `ChangeLogs`, and asks the configured AI summary service to enrich those logs asynchronously. Business services explicitly request AI summaries for high-value actions: `PRODUCT_UPDATED`, `BULK_PRICE_UPDATE`, `BULK_PRODUCT_ARCHIVE`, `CATEGORY_PRODUCTS_ARCHIVED`, and `PAYMENT_COMPLETED`. The audit interceptor also requests AI enrichment when an automatic audit record has two or more changed fields, because those changes benefit from a readable summary.

AI runs outside business transactions so a slow or failed AI call cannot roll back product, inventory, order, payment, or audit data. If AI enrichment fails, the processor records the failure on the `ChangeLog` with `AiSummaryStatus = Failed` and stores the error message in `AiSummaryError`; the original audit/business log remains useful even without an AI summary. If the Gemini API key or model name is missing from configuration, the summary service logs a warning and returns the original description as a fallback so the background service continues running without crashing.

## EF Core Features

Read paths use `AsNoTracking()` when entities are not being modified. This keeps query materialization cheaper and avoids accidental state tracking for list, detail, summary, and lookup screens. Include-heavy queries use `AsSplitQuery()` for order item/product and inventory/product reads so EF Core avoids large cartesian joins when loading related data.

Compiled queries are used for hot lookups and detail reads: products by SKU, customers by email/id/detail, categories by id/name/detail, orders by id/customer, inventory by product id, and payments by id/order id. These paths are small and repeated often, so compiled queries reduce EF query translation overhead.

Bulk operations use set-based database commands. `ExecuteUpdateAsync()` powers product price updates, product archive/restore, and category-based product archive operations. `ExecuteDeleteAsync()` powers bulk deletion of old change logs and action-type-specific change logs. These operations avoid loading every affected row into memory.

Repositories project directly to DTO read models where the query is purely read-oriented and the response shape is stable. Write paths still load domain entities and apply behavior through entity methods. Paged reads apply `Skip()` and `Take()` at the database level for products, orders, categories, change logs, AI log views, and inventory transaction views.

## Testing

The test suite covers write/read validation, category and customer workflows, inventory workflows, payments, transaction behavior, migrations, concurrency, upgrade pipeline behavior, pure domain invariant tests, and API-level workflow conflict tests.

Recent coverage additions include order completion workflow tests, API-level `409 Conflict` tests for invalid order completion states, payment rollback and `PAYMENT_FAILED` logging tests, inventory concurrency conflict simulation, pure domain tests for the order state machine and inventory transaction invariants, and upgrade pipeline idempotency for `BackfillAiSummaryStatus`. The upgrade idempotency test also covers the stale change-tracker case by clearing EF tracking after raw SQL setup before running the upgrade runner.
