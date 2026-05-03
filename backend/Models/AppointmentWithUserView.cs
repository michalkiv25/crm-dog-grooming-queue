namespace DogQueueApi.Models;

/// <summary>
/// Keyless projection mapped to SQL VIEW <c>vw_AppointmentsWithUsers</c> (appointments joined with users for FullName).
/// </summary>
public class AppointmentWithUserView
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string DogName { get; set; } = "";
    public string DogSize { get; set; } = "";
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
}
