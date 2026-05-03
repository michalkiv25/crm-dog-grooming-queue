# SQL objects (assignment)

## When does SQL Server run?

- **Default** `appsettings.json` uses **Microsoft SQL Server LocalDB** (assignment: procedure + VIEW in DB). `Program.cs` still treats any connection string starting with `Data Source=` as **SQLite** (tests + optional overrides).
- **`appsettings.Production.json`** overrides to **SQLite** (`dogqueue.db`) for Docker / Render on Linux (no LocalDB there). Override with `ConnectionStrings__DefaultConnection` in the host if you use Azure SQL instead.
- **Mac / Linux dev:** LocalDB is unavailable — use Docker SQL Server and set `ConnectionStrings__DefaultConnection` (see `appsettings.SqlServer.example.json`), or use `Data Source=...` for SQLite-only dev.
- Launch profile **`sqlserver`** in `launchSettings.json` still forces LocalDB if you want an explicit profile on Windows.
- On startup, when the provider is **not** SQLite, the app runs **`Database.Migrate()`** then **`SqlServerRoutineInstaller`** (creates VIEW + `sp_GetUserLoyaltyPreview`).

## VIEW: `vw_AppointmentsWithUsers`

- **SQLite:** created on every API startup by `SqliteSchemaFixer` (`DROP VIEW IF EXISTS` + `CREATE VIEW`).
- **SQL Server:** created on startup by `SqlServerRoutineInstaller`.
- **Read in code:** `AppointmentsManager.GetUpcomingAppointmentsWithUserInfo()` → `GET /api/appointments/upcoming-with-user-info`.

## Stored procedure: `dbo.sp_GetUserLoyaltyPreview` (SQL Server only)

SQLite does not support `CREATE PROCEDURE`. The procedure is created only when `ConnectionStrings:DefaultConnection` points at **SQL Server** (not `Data Source=` SQLite).

- **Install:** `SqlServerRoutineInstaller.Apply` (runs from `Program.cs` after `SqliteSchemaFixer`).
- **Call:** `AppointmentsManager.GetLoyaltyBookingPreview` uses `EXEC dbo.sp_GetUserLoyaltyPreview @Username` when the provider is SqlServer; otherwise the same logic runs in LINQ.

Script copy: `sp_GetUserLoyaltyPreview.sql`.
