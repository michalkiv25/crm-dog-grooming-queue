using DogQueue.WebApi.Interfaces.Repositories;
using DogQueue.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DogQueue.WebApi.Data.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _db;

    public AppointmentRepository(AppDbContext db) => _db = db;

    public string DatabaseProviderName => _db.Database.ProviderName ?? "";

    public IQueryable<Appointment> QueryForUser(string canonicalUsername) =>
        _db.Appointments.Where(a => a.Username == canonicalUsername);

    public Appointment? FindTracked(int id) =>
        _db.Appointments.FirstOrDefault(a => a.Id == id);

    public void Add(Appointment appointment) => _db.Appointments.Add(appointment);

    public void Remove(Appointment appointment) => _db.Appointments.Remove(appointment);

    public void SaveChanges() => _db.SaveChanges();

    public Appointment FirstNoTracking(int id) =>
        _db.Appointments.AsNoTracking().First(a => a.Id == id);

    public List<Appointment> ListUpcoming(DateTime now) =>
        _db.Appointments
            .AsNoTracking()
            .Where(a => a.Date >= now)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Id)
            .ToList();

    public List<Appointment> ListAllOrderByDate() =>
        _db.Appointments
            .AsNoTracking()
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Id)
            .ToList();

    public List<DateTime> ListAllAppointmentStartTimes() =>
        _db.Appointments.AsNoTracking().Select(a => a.Date).ToList();

    public List<AppointmentWithUserView> ListUpcomingWithUsers(DateTime now) =>
        _db.AppointmentWithUserViews
            .AsNoTracking()
            .Where(v => v.Date >= now)
            .OrderBy(v => v.Date)
            .ThenBy(v => v.Id)
            .ToList();

    public List<Appointment> ListOrderedByDateThenIdNoTrackingForUser(string canonicalUsername) =>
        _db.Appointments
            .AsNoTracking()
            .Where(a => a.Username == canonicalUsername)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Id)
            .ToList();

    public void ExecuteUpdatePriceAndDuration(int appointmentId, decimal price, int durationMinutes)
    {
        /** Provider-agnostic (SQLite has no dbo schema; raw dbo.Appointments breaks loyalty repricing on Render fallback). */
        _db.Appointments
            .Where(a => a.Id == appointmentId)
            .ExecuteUpdate(setters => setters
                .SetProperty(a => a.Price, price)
                .SetProperty(a => a.DurationMinutes, durationMinutes));
    }

    public bool SlotTakenByLinq(DateTime slot, int? excludeAppointmentId)
    {
        /** Align with unique index on calendar minute: any row whose instant falls in [start, start+1 minute). */
        var start = new DateTime(slot.Year, slot.Month, slot.Day, slot.Hour, slot.Minute, 0, slot.Kind);
        var end = start.AddMinutes(1);

        var query = _db.Appointments.AsNoTracking().Where(a => a.Date >= start && a.Date < end);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return query.Any();
    }
}
