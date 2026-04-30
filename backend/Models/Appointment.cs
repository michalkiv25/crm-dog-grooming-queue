namespace DogQueueApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string DogName { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string Username { get; set; } = string.Empty; // מי יצר את התור
    }
}