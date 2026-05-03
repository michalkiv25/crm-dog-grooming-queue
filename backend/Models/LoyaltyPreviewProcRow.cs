namespace DogQueueApi.Models;

/// <summary>Result shape returned by stored procedure <c>sp_GetUserLoyaltyPreview</c> (SQL Server).</summary>
public class LoyaltyPreviewProcRow
{
    public int AppointmentCount { get; set; }
    public int NextBookingDiscountPercent { get; set; }
}
