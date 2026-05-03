namespace DogQueueApi.Models;

/// <summary>
/// Shown on "create appointment" so the user sees if the next booking qualifies for loyalty pricing before submitting.
/// </summary>
public class LoyaltyBookingPreview
{
    /// <summary>How many appointments this user already has (before creating the next one).</summary>
    public int AppointmentCount { get; set; }

    /// <summary>0 or 10 — discount applied to the next booking when AppointmentCount is at least 3.</summary>
    public int NextBookingDiscountPercent { get; set; }
}
