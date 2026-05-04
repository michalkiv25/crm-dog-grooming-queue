using System;
using System.Linq;
using DogQueueApi.Data;
using DogQueueApi.Infrastructure;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Models;
using DogQueueApi.Services;
using DogQueueApi.Validators;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DogQueueApi.Managers
{
    public class AppointmentsManager : IAppointmentsManager
    {
        private const string SqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";

        /// <summary>
        /// Loyalty: first three bookings (indices 0–2 by ascending Id) pay full catalog; index 3+ get 10% off.
        /// If the customer deletes down to three (or fewer), all remaining rows are repriced to full catalog.
        /// Enforced in <see cref="ReapplyLoyaltyPricesForUser"/> after create, update, and delete.
        /// </summary>
        private const int FullPriceBookingSlotCount = 3;

        private readonly AppDbContext _context;

        public AppointmentsManager(AppDbContext context)
        {
            _context = context;
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

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            ReapplyLoyaltyPricesForUser(appointment.Username);

            var createdId = appointment.Id;
            var fresh = _context.Appointments.AsNoTracking().First(a => a.Id == createdId);
            return ServiceResult<Appointment>.Ok(fresh);
        }

        public ServiceResult<Appointment> Update(string username, int id, Appointment updatedAppointment)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
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

            _context.SaveChanges();

            ReapplyLoyaltyPricesForUser(appointment.Username);

            var fresh = _context.Appointments.AsNoTracking().First(a => a.Id == id);
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
            var rows = _context.Appointments
                .AsNoTracking()
                .Where(a => a.Username == key)
                .OrderBy(a => a.Id)
                .ToList();

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

                _context.Database.ExecuteSqlInterpolated(
                    $@"UPDATE ""Appointments"" SET ""Price"" = {newPrice}, ""DurationMinutes"" = {scratch.DurationMinutes} WHERE ""Id"" = {row.Id}");
            }
        }

        /// <summary>
        /// Match appointments by canonical username (same value stored on create).
        /// </summary>
        private IQueryable<Appointment> AppointmentsForUser(string? username)
        {
            var key = UsernameNormalizer.Canonical(username);
            return _context.Appointments.Where(a => a.Username == key);
        }

        private static bool UsernameEquals(string stored, string? fromToken) =>
            UsernameNormalizer.Canonical(stored) == UsernameNormalizer.Canonical(fromToken);

        public ServiceResult<object?> Delete(string username, int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
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
            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
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

            if (_context.Database.ProviderName == SqlServerProvider)
            {
                try
                {
                    var param = new SqlParameter("@Username", key);
                    var row = _context.Database
                        .SqlQueryRaw<LoyaltyPreviewProcRow>(
                            "EXEC dbo.sp_GetUserLoyaltyPreview @Username",
                            param)
                        .AsEnumerable()
                        .FirstOrDefault();

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
            var list = _context.Appointments
                .AsNoTracking()
                .Where(a => a.Date >= now)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Id)
                .ToList();

            return ServiceResult<List<Appointment>>.Ok(list);
        }

        public ServiceResult<List<AppointmentWithUserView>> GetUpcomingAppointmentsWithUserInfo()
        {
            var now = DateTime.Now.AddMinutes(-1);
            var list = _context.AppointmentWithUserViews
                .AsNoTracking()
                .Where(v => v.Date >= now)
                .OrderBy(v => v.Date)
                .ThenBy(v => v.Id)
                .ToList();

            return ServiceResult<List<AppointmentWithUserView>>.Ok(list);
        }

        /// <summary>
        /// True if another appointment already uses this calendar slot (same year/month/day/hour/minute).
        /// SQL Server: stored procedure <c>dbo.sp_AppointmentSlotTaken</c>; SQLite: LINQ (no CREATE PROCEDURE).
        /// </summary>
        private bool IsAppointmentSlotTaken(DateTime slot, int? excludeAppointmentId = null)
        {
            if (_context.Database.ProviderName == SqlServerProvider)
            {
                try
                {
                    var row = _context.Database
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

                    if (row != null)
                        return row.Taken != 0;
                }
                catch
                {
                    // Procedure missing — fall back to LINQ
                }
            }

            return AppointmentSlotTakenByLinq(slot, excludeAppointmentId);
        }

        private bool AppointmentSlotTakenByLinq(DateTime slot, int? excludeAppointmentId)
        {
            var query = _context.Appointments.AsNoTracking().Where(a =>
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
}
