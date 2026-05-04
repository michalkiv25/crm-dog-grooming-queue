using DogQueueApi.Interfaces.Repositories;
using DogQueueApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DogQueueApi.Data.Repositories;

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

    public List<AppointmentWithUserView> ListUpcomingWithUsers(DateTime now) =>
        _db.AppointmentWithUserViews
            .AsNoTracking()
            .Where(v => v.Date >= now)
            .OrderBy(v => v.Date)
            .ThenBy(v => v.Id)
            .ToList();

    public List<Appointment> ListOrderedByIdNoTrackingForUser(string canonicalUsername) =>
        _db.Appointments
            .AsNoTracking()
            .Where(a => a.Username == canonicalUsername)
            .OrderBy(a => a.Id)
            .ToList();

    public void ExecuteUpdatePriceAndDuration(int appointmentId, decimal price, int durationMinutes) =>
        _db.Database.ExecuteSqlInterpolated(
            $@"UPDATE ""Appointments"" SET ""Price"" = {price}, ""DurationMinutes"" = {durationMinutes} WHERE ""Id"" = {appointmentId}");

    public LoyaltyPreviewProcRow? ExecLoyaltyPreviewProcedure(string canonicalUsername)
    {
        var param = new SqlParameter("@Username", canonicalUsername);
        return _db.Database
            .SqlQueryRaw<LoyaltyPreviewProcRow>("EXEC dbo.sp_GetUserLoyaltyPreview @Username", param)
            .AsEnumerable()
            .FirstOrDefault();
    }

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
        var query = _db.Appointments.AsNoTracking().Where(a =>
            a.Date.Year == slot.Year &&
            a.Date.Month == slot.Month &&
            a.Date.Day == slot.Day &&
            a.Date.Hour == slot.Hour &&
            a.Date.Minute == slot.Minute);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return query.Any();
    }
}
