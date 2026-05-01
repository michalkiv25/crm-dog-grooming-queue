namespace DogQueueApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string DogName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}