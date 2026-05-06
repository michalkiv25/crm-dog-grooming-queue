using DogQueue.WebApi.Infrastructure;
using DogQueue.WebApi.Interfaces.Managers;
using DogQueue.WebApi.Interfaces.Repositories;
using DogQueue.WebApi.Models;
using DogQueue.WebApi.Validators;
using Microsoft.EntityFrameworkCore;

namespace DogQueue.WebApi.Managers;

/// <summary>
/// Appointment business rules. EF/SQL in <see cref="DogQueue.WebApi.Data.Repositories.AppointmentRepository"/>.
/// </summary>
public class AppointmentsManager : IAppointmentsManager
{
    /// <summary>
    /// First three saved rows per user (by ascending scheduled <c>Date</c>, then <c>Id</c>) pay list price; fourth and later get 10% off list
    /// for that size. If the customer deletes until fewer than four remain, all remaining rows are repriced to full list.
    /// </summary>
    private const int FullPriceBookingSlotCount = 3;

    private readonly IAppointmentRepository _appointments;

    public AppointmentsManager(IAppointmentRepository appointments)
    {
        _appointments = appointments;
    }

    public ServiceResult<List<Appointment>> GetAll(string username)
    {
        var key = UsernameNormalizer.Canonical(username);
        ReapplyLoyaltyPricesForUser(key);
        var appointments = AppointmentsForUser(username).ToList();
        return ServiceResult<List<Appointment>>.Ok(appointments);
    }

    public ServiceResult<Appointment> Create(string username, Appointment appointment)
    {
        appointment.Username = UsernameNormalizer.Canonical(username);
        appointment.Date = TruncateToMinute(appointment.Date);

        var (isValid, errors) = AppointmentValidator.ValidateForCreate(appointment);
        if (!isValid)
        {
            return ServiceResult<Appointment>.BadRequest("Validation failed", errors);
        }

        appointment.CreatedAt = DateTime.Now;
        appointment.CalculatePriceAndDuration();

        try
        {
            _appointments.Add(appointment);
            _appointments.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            return ServiceResult<Appointment>.BadRequest(
                "Failed to save appointment",
                new[]
                {
                    ex.InnerException?.Message ?? ex.Message
                });
        }

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
        updatedAppointment.Date = TruncateToMinute(updatedAppointment.Date);

        var (isValid, errors) = AppointmentValidator.ValidateForUpdate(updatedAppointment, appointment);
        if (!isValid)
        {
            return ServiceResult<Appointment>.BadRequest("Validation failed", errors);
        }

        appointment.DogName = updatedAppointment.DogName;
        appointment.DogSize = updatedAppointment.DogSize;
        appointment.Date = updatedAppointment.Date;
        appointment.CalculatePriceAndDuration();

        try
        {
            _appointments.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            return ServiceResult<Appointment>.BadRequest(
                "Failed to save appointment",
                new[]
                {
                    ex.InnerException?.Message ?? ex.Message
                });
        }

        ReapplyLoyaltyPricesForUser(appointment.Username);

        var fresh = _appointments.FirstNoTracking(id);
        return ServiceResult<Appointment>.Ok(fresh);
    }

    private void ReapplyLoyaltyPricesForUser(string canonicalUsername)
    {
        var key = UsernameNormalizer.Canonical(canonicalUsername);
        var rows = _appointments.ListOrderedByDateThenIdNoTrackingForUser(key);

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

    private void ReapplyLoyaltyForDistinctOwners(IEnumerable<string?> owners)
    {
        foreach (var o in owners.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase))
            ReapplyLoyaltyPricesForUser(o!);
    }

    private IQueryable<Appointment> AppointmentsForUser(string? username)
    {
        var key = UsernameNormalizer.Canonical(username);
        return _appointments.QueryForUser(key);
    }

    private static bool UsernameEquals(string stored, string? fromToken) =>
        UsernameNormalizer.Canonical(stored) == UsernameNormalizer.Canonical(fromToken);

    /// <summary>Deletes one row the user owns. There is no same-day or "cannot delete on the day of" rule.</summary>
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

    public ServiceResult<List<Appointment>> GetUpcomingQueue()
    {
        var now = DateTime.Now.AddMinutes(-1);
        var list = _appointments.ListUpcoming(now);
        ReapplyLoyaltyForDistinctOwners(list.Select(a => a.Username));
        list = _appointments.ListUpcoming(now);
        return ServiceResult<List<Appointment>>.Ok(list);
    }

    public ServiceResult<List<Appointment>> GetSalonAppointments()
    {
        var list = _appointments.ListAllOrderByDate();
        ReapplyLoyaltyForDistinctOwners(list.Select(a => a.Username));
        list = _appointments.ListAllOrderByDate();
        return ServiceResult<List<Appointment>>.Ok(list);
    }

    public ServiceResult<List<AppointmentWithUserView>> GetUpcomingAppointmentsWithUserInfo()
    {
        var now = DateTime.Now.AddMinutes(-1);
        var list = _appointments.ListUpcomingWithUsers(now);
        ReapplyLoyaltyForDistinctOwners(list.Select(v => v.Username));
        list = _appointments.ListUpcomingWithUsers(now);
        return ServiceResult<List<AppointmentWithUserView>>.Ok(list);
    }

    private static DateTime TruncateToMinute(DateTime dt) =>
        new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
}
