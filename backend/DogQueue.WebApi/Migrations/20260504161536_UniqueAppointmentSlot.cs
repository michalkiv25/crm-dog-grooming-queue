using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogQueue.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UniqueAppointmentSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // One booking per calendar minute (salon-wide). Remove duplicates keeping lowest Id so the unique index applies.
            migrationBuilder.Sql(
                """
                ;WITH numbered AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (
                               PARTITION BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, [Date]), 0)
                               ORDER BY Id
                           ) AS rn
                    FROM dbo.Appointments
                )
                DELETE FROM dbo.Appointments WHERE Id IN (SELECT Id FROM numbered WHERE rn > 1);
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'dbo.Appointments', N'U') AND name = N'AppointmentSlot'
                )
                BEGIN
                    ALTER TABLE dbo.Appointments
                    ADD AppointmentSlot AS (DATEADD(MINUTE, DATEDIFF(MINUTE, 0, [Date]), 0)) PERSISTED;
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Appointments_AppointmentSlot' AND object_id = OBJECT_ID(N'dbo.Appointments', N'U')
                )
                CREATE UNIQUE NONCLUSTERED INDEX IX_Appointments_AppointmentSlot
                ON dbo.Appointments(AppointmentSlot);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_Appointments_AppointmentSlot' AND object_id = OBJECT_ID(N'dbo.Appointments', N'U')
                )
                    DROP INDEX IX_Appointments_AppointmentSlot ON dbo.Appointments;
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'dbo.Appointments', N'U') AND name = N'AppointmentSlot'
                )
                    ALTER TABLE dbo.Appointments DROP COLUMN AppointmentSlot;
                """);
        }
    }
}
