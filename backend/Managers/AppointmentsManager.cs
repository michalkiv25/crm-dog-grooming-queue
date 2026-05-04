using DogQueueApi.Infrastructure;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Interfaces.Repositories;
using DogQueueApi.Models;
using DogQueueApi.Services;
using DogQueueApi.Validators;

namespace DogQueueApi.Managers;

/// <summary>
/// Appointment business rules (loyalty, slot conflicts, ownership). Controllers only delegate here;
/// EF/SQL lives in <see cref="Data.Repositories.AppointmentRepository"/>.
/// </summary>
public class AppointmentsManager : IAppointmentsManager
{
    private const string SqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";

    /// <summary>
    /// Loyalty: first three bookings (indices 0–2 by ascending Id) pay full catalog; index 3+ get 10% off.
    /// If the customer deletes down to three (or fewer), all remaining rows are repriced to full catalog.
    /// Enforced in <see cref="ReapplyLoyaltyPricesForUser"/> after create, update, and delete.
    /// </summary>
    private const int FullPriceBookingSlotCount = 3;

    private readonly IAppointmentRepository _appointments;

    public AppointmentsManager(IAppointmentRepository appointments)
    {
        _appointments = appointments;
    }

    public ServiceResult<List<Appointment>> GetAll(string username)
    {
        var appointments = AppointmentsForUser(username).ToList();

        return ServiceResult<List<Appointment>>.Ok(appointments);
    }

    public ServiceResult<Appointment> Create(string username, Appointment appointment)
    {
        appointment.Username = UsernameNormalizer.Canonical(username);

        var (isValid, errors) = AppointmentValidator.Validate(appointment);
        if (!isValid)
        {
            return ServiceResult<Appointment>.BadRequest("Validation failed", errors);
        }

        if (IsAppointmentSlotTaken(appointment.Date))
        {
            return ServiceResult<Appointment>.BadRequest(
                "That date and time is already booked",
                new[] { "This time slot is already taken. Choose another date or time." });
        }

        appointment.CreatedAt = DateTime.Now;

        appointment.CalculatePriceAndDuration();

        _appointments.Add(appointment);
        _appointments.SaveChanges();

        ReapplyLoyaltyPricesForUser(appointment.Username);

        var createdId = appointment.Id;
        var fresh = _appointments.FirstNoTracking(createdId);
        return ServiceResult<Appointment>.Ok(fresh);
    }

    public ServiceResult<Appointment> Update(string username, int id, Appointment updatedAppointment)
    {
        var appointment = _appointments.FindTracked(id);
        if (appointment == null)
        {
            return ServiceResult<Appointment>.NotFound();
        }

        if (!UsernameEquals(appointment.Username, username))
        {
            return ServiceResult<Appointment>.Forbid();
        }

        updatedAppointment.Username = appointment.Username;
        var (isValid, errors) = AppointmentValidator.Validate(updatedAppointment);
        if (!isValid)
        {
            return ServiceResult<Appointment>.BadRequest("Validation failed", errors);
        }

        if (IsAppointmentSlotTaken(updatedAppointment.Date, id))
        {
            return ServiceResult<Appointment>.BadRequest(
                "That date and time is already booked",
                new[] { "This time slot is already taken. Choose another date or time." });
        }

        appointment.DogName = updatedAppointment.DogName;
        appointment.DogSize = updatedAppointment.DogSize;
        appointment.Date = updatedAppointment.Date;
        appointment.CalculatePriceAndDuration();

        _appointments.SaveChanges();

        ReapplyLoyaltyPricesForUser(appointment.Username);

        var fresh = _appointments.FirstNoTracking(id);
        return ServiceResult<Appointment>.Ok(fresh);
    }

    /// <summary>
    /// Re-price every row for the user: slots 0–2 full catalog, slot 3+ 10% off (by ascending Id = booking order).
    /// Called after create, update, and delete so prices never depend on “how many others exist” on a single row.
    /// Uses SQL UPDATE so SQLite always persists new prices (tracked SaveChanges was flaky after deletes).
    /// </summary>
    private void ReapplyLoyaltyPricesForUser(string canonicalUsername)
    {
        var key = UsernameNormalizer.Canonical(canonicalUsername);
        var rows = _appointments.ListOrderedByIdNoTrackingForUser(key);

        // Index i by ascending Id: i &lt; 3 full catalog, i ≥ 3 loyalty (fourth booking onward).
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var scratch = new Appointment { DogSize = row.DogSize };
            scratch.CalculatePriceAndDuration();

            var discount = i < FullPriceBookingSlotCount ? 0m : 0.10m;
            var newPrice = decimal.Round(
                scratch.Price * (1 - discount),
                2,
                MidpointRounding.AwayFromZero);

            _appointments.ExecuteUpdatePriceAndDuration(row.Id, newPrice, scratch.DurationMinutes);
        }
    }

    /// <summary>
    /// Match appointments by canonical username (same value stored on create).
    /// </summary>
    private IQueryable<Appointment> AppointmentsForUser(string? username)
    {
        var key = UsernameNormalizer.Canonical(username);
        return _appointments.QueryForUser(key);
    }

    private static bool UsernameEquals(string stored, string? fromToken) =>
        UsernameNormalizer.Canonical(stored) == UsernameNormalizer.Canonical(fromToken);

    public ServiceResult<object?> Delete(string username, int id)
    {
        var appointment = _appointments.FindTracked(id);
        if (appointment == null)
        {
            return ServiceResult<object?>.NotFound();
        }

        if (!UsernameEquals(appointment.Username, username))
        {
            return ServiceResult<object?>.Forbid();
        }

        if (appointment.Date.Date == DateTime.Now.Date)
        {
            return ServiceResult<object?>.BadRequest("Cannot delete appointments for today");
        }

        var owner = appointment.Username;
        _appointments.Remove(appointment);
        _appointments.SaveChanges();
        ReapplyLoyaltyPricesForUser(owner);
        return ServiceResult<object?>.Ok(null);
    }

    public ServiceResult<List<Appointment>> GetFiltered(string username, DateTime? date, string? customerName)
    {
        var query = AppointmentsForUser(username);

        if (date.HasValue)
        {
            query = query.Where(a => a.Date.Date == date.Value.Date);
        }

        if (!string.IsNullOrEmpty(customerName))
        {
            query = query.Where(a => a.Username.Contains(customerName));
        }

        return ServiceResult<List<Appointment>>.Ok(query.ToList());
    }

    public ServiceResult<LoyaltyBookingPreview> GetLoyaltyBookingPreview(string? username)
    {
        var key = UsernameNormalizer.Canonical(username);

        if (_appointments.DatabaseProviderName == SqlServerProvider)
        {
            try
            {
                var row = _appointments.ExecLoyaltyPreviewProcedure(key);

                if (row != null)
                {
                    return ServiceResult<LoyaltyBookingPreview>.Ok(new LoyaltyBookingPreview
                    {
                        AppointmentCount = row.AppointmentCount,
                        NextBookingDiscountPercent = row.NextBookingDiscountPercent
                    });
                }
            }
            catch
            {
                // Procedure or view not installed yet — fall back to LINQ.
            }
        }

        var count = AppointmentsForUser(username).Count();
        var discountPercent = count >= FullPriceBookingSlotCount ? 10 : 0;

        return ServiceResult<LoyaltyBookingPreview>.Ok(new LoyaltyBookingPreview
        {
            AppointmentCount = count,
            NextBookingDiscountPercent = discountPercent
        });
    }

    public ServiceResult<List<Appointment>> GetUpcomingQueue()
    {
        // Align with AppointmentValidator (uses DateTime.Now). UtcNow alone often excludes
        // valid “future” rows when JSON binds local wall-clock times without timezone.
        var now = DateTime.Now.AddMinutes(-1);
        var list = _appointments.ListUpcoming(now);

        return ServiceResult<List<Appointment>>.Ok(list);
    }

    public ServiceResult<List<AppointmentWithUserView>> GetUpcomingAppointmentsWithUserInfo()
    {
        var now = DateTime.Now.AddMinutes(-1);
        var list = _appointments.ListUpcomingWithUsers(now);

        return ServiceResult<List<AppointmentWithUserView>>.Ok(list);
    }

    /// <summary>
    /// True if another appointment already uses this calendar slot (same year/month/day/hour/minute).
    /// SQL Server: stored procedure <c>dbo.sp_AppointmentSlotTaken</c>; SQLite: LINQ (no CREATE PROCEDURE).
    /// </summary>
    private bool IsAppointmentSlotTaken(DateTime slot, int? excludeAppointmentId = null)
    {
        if (_appointments.DatabaseProviderName == SqlServerProvider)
        {
            try
            {
                var row = _appointments.ExecSlotTakenProcedure(slot, excludeAppointmentId);

                if (row != null)
                    return row.Taken != 0;
            }
            catch
            {
                // Procedure missing — fall back to LINQ
            }
        }

        return _appointments.SlotTakenByLinq(slot, excludeAppointmentId);
    }
}
