using System.Text.RegularExpressions;
using DogQueue.WebApi.Models;

namespace DogQueue.WebApi.Validators;

public static class AppointmentValidator
{
    /// <summary>
    /// New bookings must be strictly in the future (vs server clock).
    /// </summary>
    public static (bool isValid, string[] errors) ValidateForCreate(Appointment appointment)
    {
        var errors = ValidateSharedFields(appointment);

        if (appointment.Date == default)
            errors.Add("Appointment date is required");
        else if (appointment.Date <= DateTime.Now)
            errors.Add("Appointment date must be in the future");

        return (errors.Count == 0, errors.ToArray());
    }

    /// <summary>
    /// Updates: rescheduling to a new slot requires a future time; keeping the same slot (or tiny skew)
    /// allows edits even when that slot is already in the past (e.g. same-day cancellation metadata, name fix).
    /// </summary>
    public static (bool isValid, string[] errors) ValidateForUpdate(Appointment updated, Appointment existing)
    {
        var errors = ValidateSharedFields(updated);

        if (updated.Date == default)
            errors.Add("Appointment date is required");
        else if (!SameBookingSlot(updated.Date, existing.Date) && updated.Date <= DateTime.Now)
            errors.Add("Appointment date must be in the future");

        return (errors.Count == 0, errors.ToArray());
    }

    /// <summary>
    /// Same wall slot if instants are within a few minutes (JSON Kind / timezone skew between client and DB).
    /// </summary>
    private static bool SameBookingSlot(DateTime incoming, DateTime existing)
    {
        if (incoming == default || existing == default) return false;
        try
        {
            return Math.Abs((incoming - existing).TotalMinutes) < 6;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static List<string> ValidateSharedFields(Appointment appointment)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(appointment.DogName))
            errors.Add("Dog name is required");
        else if (appointment.DogName.Length < 2)
            errors.Add("Dog name must be at least 2 characters");
        else if (appointment.DogName.Length > 50)
            errors.Add("Dog name must not exceed 50 characters");
        else if (!Regex.IsMatch(appointment.DogName.Trim(), @"^[\p{L}\s'\-]+$", RegexOptions.None, TimeSpan.FromSeconds(1)))
            errors.Add("Dog name must contain letters, spaces, hyphen or apostrophe only");

        if (string.IsNullOrWhiteSpace(appointment.DogSize))
            errors.Add("Dog size is required");
        else if (!new[] { "small", "medium", "large" }.Contains(appointment.DogSize.ToLower()))
            errors.Add("Dog size must be one of: small, medium, large");

        return errors;
    }
}
