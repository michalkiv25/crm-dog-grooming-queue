-- Loyalty preview for next booking (same logic as C# fallback).
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
    -- Next booking will be 0-based index @cnt; 10% only from 4th appointment (@cnt >= 3).
    DECLARE @minExistingBeforeNextGetsLoyalty INT = 3;
    SELECT
        @cnt AS AppointmentCount,
        CASE WHEN @cnt >= @minExistingBeforeNextGetsLoyalty THEN 10 ELSE 0 END AS NextBookingDiscountPercent;
END
GO
