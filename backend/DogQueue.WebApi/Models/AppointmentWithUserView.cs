namespace DogQueue.WebApi.Models;

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
