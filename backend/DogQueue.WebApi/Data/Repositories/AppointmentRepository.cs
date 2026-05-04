using DogQueue.WebApi.Interfaces.Repositories;
using DogQueue.WebApi.Models;
using Microsoft.Data.SqlClient;
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

    public void ExecuteUpdatePriceAndDuration(int appointmentId, decimal price, int durationMinutes) =>
        _db.Database.ExecuteSqlInterpolated(
            $"UPDATE dbo.Appointments SET Price = {price}, DurationMinutes = {durationMinutes} WHERE Id = {appointmentId}");

    public SlotTakenProcRow? ExecSlotTakenProcedure(DateTime slot, int? excludeAppointmentId)
    {
        return _db.Database
            .SqlQueryRaw<SlotTakenProcRow>(
                "EXEC dbo.sp_AppointmentSlotTaken @Year, @Month, @Day, @Hour, @Minute, @ExcludeAppointmentId",
                new SqlParameter("@Year", slot.Year),
                new SqlParameter("@Month", slot.Month),
                new SqlParameter("@Day", slot.Day),
                new SqlParameter("@Hour", slot.Hour),
                new SqlParameter("@Minute", slot.Minute),
                new SqlParameter("@ExcludeAppointmentId", (object?)excludeAppointmentId ?? DBNull.Value))
            .AsEnumerable()
            .FirstOrDefault();
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
