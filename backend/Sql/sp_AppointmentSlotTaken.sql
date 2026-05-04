-- Slot conflict check (same calendar day + hour + minute). Used by API on SQL Server.
IF OBJECT_ID(N'dbo.sp_AppointmentSlotTaken', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AppointmentSlotTaken;
GO

CREATE PROCEDURE dbo.sp_AppointmentSlotTaken
    @Year INT,
    @Month INT,
    @Day INT,
    @Hour INT,
    @Minute INT,
    @ExcludeAppointmentId INT NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Taken INT = 0;

    IF EXISTS (
        SELECT 1
        FROM dbo.Appointments
        WHERE YEAR([Date]) = @Year
          AND MONTH([Date]) = @Month
          AND DAY([Date]) = @Day
          AND DATEPART(HOUR, [Date]) = @Hour
          AND DATEPART(MINUTE, [Date]) = @Minute
          AND (@ExcludeAppointmentId IS NULL OR [Id] <> @ExcludeAppointmentId)
    )
        SET @Taken = 1;

    SELECT @Taken AS Taken;
END
GO
