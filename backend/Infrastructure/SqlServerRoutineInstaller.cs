using System;
using DogQueueApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DogQueueApi.Infrastructure;

/// <summary>
/// Creates SQL Server VIEW + stored procedure required by the assignment (SQLite has no CREATE PROCEDURE).
/// Safe to call on every startup; uses DROP/CREATE when objects exist.
/// </summary>
public static class SqlServerRoutineInstaller
{
    private const string SqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";

    public static void Apply(AppDbContext db)
    {
        if (db.Database.ProviderName != SqlServerProvider)
            return;

        try
        {
            db.Database.ExecuteSqlRaw("""
                IF OBJECT_ID(N'dbo.vw_AppointmentsWithUsers', N'V') IS NOT NULL
                    DROP VIEW dbo.vw_AppointmentsWithUsers;
                """);

            db.Database.ExecuteSqlRaw("""
                CREATE VIEW dbo.vw_AppointmentsWithUsers AS
                SELECT
                    a.Id,
                    a.Username,
                    u.FullName AS FullName,
                    a.DogName,
                    a.DogSize,
                    a.Date,
                    a.CreatedAt,
                    a.Price,
                    a.DurationMinutes
                FROM dbo.Appointments AS a
                INNER JOIN dbo.Users AS u ON u.Username = a.Username;
                """);

            db.Database.ExecuteSqlRaw("""
                IF OBJECT_ID(N'dbo.sp_GetUserLoyaltyPreview', N'P') IS NOT NULL
                    DROP PROCEDURE dbo.sp_GetUserLoyaltyPreview;
                """);

            db.Database.ExecuteSqlRaw("""
                CREATE PROCEDURE dbo.sp_GetUserLoyaltyPreview
                    @Username NVARCHAR(450)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @cnt INT = (
                        SELECT COUNT(*)
                        FROM dbo.Appointments
                        WHERE Username = @Username
                    );
                    SELECT
                        @cnt AS AppointmentCount,
                        CASE WHEN @cnt >= 3 THEN 10 ELSE 0 END AS NextBookingDiscountPercent;
                END
                """);
        }
        catch (Exception ex)
        {
            // Tables may not exist yet on first migration; log and continue.
            Console.WriteLine($"SqlServerRoutineInstaller: {ex.Message}");
        }
    }
}
