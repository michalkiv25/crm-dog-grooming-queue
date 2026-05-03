-- Reference script (applied automatically by SqlServerRoutineInstaller on startup).
-- SQL Server only — SQLite has no stored procedures.

IF OBJECT_ID(N'dbo.sp_GetUserLoyaltyPreview', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserLoyaltyPreview;
GO

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
GO
