using DogQueue.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace DogQueue.WebApi.Infrastructure;

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
                    -- Next booking will be 0-based index @cnt; 10% only from 4th appointment (@cnt >= 3).
                    DECLARE @minExistingBeforeNextGetsLoyalty INT = 3;
                    SELECT
                        @cnt AS AppointmentCount,
                        CASE WHEN @cnt >= @minExistingBeforeNextGetsLoyalty THEN 10 ELSE 0 END AS NextBookingDiscountPercent;
                END
                """);

            // Allow multiple appointments at the same minute slot.
            db.Database.ExecuteSqlRaw("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Appointments_AppointmentSlot' AND object_id = OBJECT_ID(N'dbo.Appointments', N'U')
                )
                    DROP INDEX IX_Appointments_AppointmentSlot ON dbo.Appointments;
                """);

            db.Database.ExecuteSqlRaw("""
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'dbo.Appointments', N'U') AND name = N'AppointmentSlot'
                )
                    ALTER TABLE dbo.Appointments DROP COLUMN AppointmentSlot;
                """);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SqlServerRoutineInstaller: {ex.Message}");
        }
    }
}
