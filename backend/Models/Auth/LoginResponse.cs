namespace DogQueueApi.Models.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Username { get; set; } = "";
        public string Fullname { get; set; } = "";
    }
}
