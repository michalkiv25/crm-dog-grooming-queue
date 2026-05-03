using System;
using Microsoft.EntityFrameworkCore;
using DogQueueApi.Data;

namespace DogQueueApi.Infrastructure;

/// <summary>
/// Keeps the local SQLite file compatible with the current EF model when switching
/// connection paths or using an older dogqueue.db file.
/// </summary>
public static class SqliteSchemaFixer
{
    public static void Apply(AppDbContext db)
    {
        if (db.Database.ProviderName is "Microsoft.EntityFrameworkCore.Sqlite")
        {
            db.Database.OpenConnection();
            try
            {
                var connection = db.Database.GetDbConnection();

                EnsureUsersTable(db, connection);
                EnsureAppointmentsTable(db, connection);
                EnsureAppointmentsWithUsersView(db);
            }
            finally
            {
                db.Database.CloseConnection();
            }
        }

        NormalizeStoredUsernames(db);
    }

    /// <summary>
    /// Align legacy rows so Username matches the canonical form used on register/login (trim + lowercase).
    /// Runs on every startup; updates only rows that differ.
    /// </summary>
    private static void NormalizeStoredUsernames(AppDbContext db)
    {
        var changed = false;
        foreach (var u in db.Users.ToList())
        {
            var c = UsernameNormalizer.Canonical(u.Username);
            if (u.Username != c)
            {
                u.Username = c;
                changed = true;
            }
        }

        foreach (var a in db.Appointments.ToList())
        {
            var c = UsernameNormalizer.Canonical(a.Username);
            if (a.Username != c)
            {
                a.Username = c;
                changed = true;
            }
        }

        if (changed)
            db.SaveChanges();
    }

    private static HashSet<string> GetColumnNames(System.Data.Common.DbConnection connection, string table)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{table}\")";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static void EnsureUsersTable(AppDbContext db, System.Data.Common.DbConnection connection)
    {
        using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Users'";
        var exists = Convert.ToInt64(check.ExecuteScalar()) > 0;
        if (!exists)
        {
            db.Database.ExecuteSqlRaw(
                """
                CREATE TABLE "Users" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
                    "Username" TEXT NOT NULL,
                    "Password" TEXT NOT NULL,
                    "FullName" TEXT NOT NULL
                );
                """);
            return;
        }

        var cols = GetColumnNames(connection, "Users");
        void Add(string name, string sql)
        {
            if (!cols.Contains(name))
                db.Database.ExecuteSqlRaw(sql);
        }

        Add("Username", """ALTER TABLE "Users" ADD COLUMN "Username" TEXT NOT NULL DEFAULT '';""");
        Add("Password", """ALTER TABLE "Users" ADD COLUMN "Password" TEXT NOT NULL DEFAULT '';""");
        Add("FullName", """ALTER TABLE "Users" ADD COLUMN "FullName" TEXT NOT NULL DEFAULT '';""");
    }

    private static void EnsureAppointmentsTable(AppDbContext db, System.Data.Common.DbConnection connection)
    {
        using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Appointments'";
        var exists = Convert.ToInt64(check.ExecuteScalar()) > 0;
        if (!exists)
        {
            db.Database.ExecuteSqlRaw(
                """
                CREATE TABLE "Appointments" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Appointments" PRIMARY KEY AUTOINCREMENT,
                    "Username" TEXT NOT NULL,
                    "DogName" TEXT NOT NULL,
                    "DogSize" TEXT NOT NULL DEFAULT '',
                    "Date" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
                    "Price" REAL NOT NULL DEFAULT 0,
                    "DurationMinutes" INTEGER NOT NULL DEFAULT 0
                );
                """);
            return;
        }

        var cols = GetColumnNames(connection, "Appointments");
        void Add(string name, string sql)
        {
            if (!cols.Contains(name))
                db.Database.ExecuteSqlRaw(sql);
        }

        Add("Username", """ALTER TABLE "Appointments" ADD COLUMN "Username" TEXT NOT NULL DEFAULT '';""");
        Add("DogName", """ALTER TABLE "Appointments" ADD COLUMN "DogName" TEXT NOT NULL DEFAULT '';""");
        Add("DogSize", """ALTER TABLE "Appointments" ADD COLUMN "DogSize" TEXT NOT NULL DEFAULT '';""");
        Add("Date", """ALTER TABLE "Appointments" ADD COLUMN "Date" TEXT NOT NULL DEFAULT '';""");
        Add("CreatedAt", """ALTER TABLE "Appointments" ADD COLUMN "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now'));""");
        Add("Price", """ALTER TABLE "Appointments" ADD COLUMN "Price" REAL NOT NULL DEFAULT 0;""");
        Add("DurationMinutes", """ALTER TABLE "Appointments" ADD COLUMN "DurationMinutes" INTEGER NOT NULL DEFAULT 0;""");
    }

    /// <summary>Assignment requirement: SQL VIEW for reads — appointments joined to users (FullName).</summary>
    private static void EnsureAppointmentsWithUsersView(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw("""DROP VIEW IF EXISTS "vw_AppointmentsWithUsers";""");
        db.Database.ExecuteSqlRaw(
            """
            CREATE VIEW "vw_AppointmentsWithUsers" AS
            SELECT
                a."Id",
                a."Username",
                u."FullName" AS FullName,
                a."DogName",
                a."DogSize",
                a."Date",
                a."CreatedAt",
                a."Price",
                a."DurationMinutes"
            FROM "Appointments" AS a
            INNER JOIN "Users" AS u ON u."Username" = a."Username";
            """);
    }
}
