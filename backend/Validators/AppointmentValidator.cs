using System.ComponentModel.DataAnnotations;

namespace DogQueueApi.Validators
{
    public static class AppointmentValidator
    {
        public static (bool isValid, string[] errors) Validate(Models.Appointment appointment)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(appointment.DogName))
                errors.Add("Dog name is required");
            else if (appointment.DogName.Length < 2)
                errors.Add("Dog name must be at least 2 characters");
            else if (appointment.DogName.Length > 50)
                errors.Add("Dog name must not exceed 50 characters");

            if (string.IsNullOrWhiteSpace(appointment.DogSize))
                errors.Add("Dog size is required");
            else if (!new[] { "small", "medium", "large" }.Contains(appointment.DogSize.ToLower()))
                errors.Add("Dog size must be one of: small, medium, large");

            if (appointment.Date == default)
                errors.Add("Appointment date is required");
            else if (appointment.Date <= DateTime.Now)
                errors.Add("Appointment date must be in the future");

            return (errors.Count == 0, errors.ToArray());
        }
    }

    public static class UserValidator
    {
        public static (bool isValid, string[] errors) ValidateRegister(Models.User user)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(user.Username))
                errors.Add("Username is required");
            else if (user.Username.Length < 3)
                errors.Add("Username must be at least 3 characters");
            else if (user.Username.Length > 30)
                errors.Add("Username must not exceed 30 characters");

            if (string.IsNullOrWhiteSpace(user.Password))
                errors.Add("Password is required");
            else if (user.Password.Length < 6)
                errors.Add("Password must be at least 6 characters");

            if (string.IsNullOrWhiteSpace(user.FullName))
                errors.Add("Full name is required");
            else if (user.FullName.Length < 2)
                errors.Add("Full name must be at least 2 characters");
            else if (user.FullName.Length > 100)
                errors.Add("Full name must not exceed 100 characters");

            return (errors.Count == 0, errors.ToArray());
        }

        public static (bool isValid, string[] errors) ValidateLogin(Models.LoginRequest login)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(login.Username))
                errors.Add("Username is required");

            if (string.IsNullOrWhiteSpace(login.Password))
                errors.Add("Password is required");

            return (errors.Count == 0, errors.ToArray());
        }
    }
}
