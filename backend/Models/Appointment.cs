namespace DogQueueApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";

        public string DogName { get; set; } = "";
        public string DogSize { get; set; } = ""; // small / medium / large

        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }

        // Calculate price and duration based on dog size
        public void CalculatePriceAndDuration()
        {
            switch (DogSize.ToLower())
            {
                case "small":
                    DurationMinutes = 30;
                    Price = 100;
                    break;
                case "medium":
                    DurationMinutes = 45;
                    Price = 150;
                    break;
                case "large":
                    DurationMinutes = 60;
                    Price = 200;
                    break;
                default:
                    DurationMinutes = 30;
                    Price = 100;
                    break;
            }
        }
    }
}