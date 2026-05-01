using DogQueueApi.Data;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Models;
using DogQueueApi.Services;
using DogQueueApi.Validators;
using Microsoft.EntityFrameworkCore;

namespace DogQueueApi.Managers
{
    public class AppointmentsManager : IAppointmentsManager
    {
        private readonly AppDbContext _context;

        public AppointmentsManager(AppDbContext context)
        {
            _context = context;
        }

        public ServiceResult<List<Appointment>> GetAll(string username)
        {
            var appointments = _context.Appointments
                .Where(a => a.Username == username)
                .ToList();

            return ServiceResult<List<Appointment>>.Ok(appointments);
        }

        public ServiceResult<Appointment> Create(string username, Appointment appointment)
        {
            appointment.Username = username;

            var (isValid, errors) = AppointmentValidator.Validate(appointment);
            if (!isValid)
            {
                return ServiceResult<Appointment>.BadRequest("Validation failed", errors);
            }

            appointment.CalculatePriceAndDuration();

            var discount = GetLoyaltyDiscountRate(appointment.Username);
            appointment.Price *= (1 - discount);

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return ServiceResult<Appointment>.Ok(appointment);
        }

        public ServiceResult<Appointment> Update(string username, int id, Appointment updatedAppointment)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
            {
                return ServiceResult<Appointment>.NotFound();
            }

            if (appointment.Username != username)
            {
                return ServiceResult<Appointment>.Forbid();
            }

            updatedAppointment.Username = appointment.Username;
            var (isValid, errors) = AppointmentValidator.Validate(updatedAppointment);
            if (!isValid)
            {
                return ServiceResult<Appointment>.BadRequest("Validation failed", errors);
            }

            appointment.DogName = updatedAppointment.DogName;
            appointment.DogSize = updatedAppointment.DogSize;
            appointment.Date = updatedAppointment.Date;
            appointment.CalculatePriceAndDuration();

            var discount = GetLoyaltyDiscountRate(appointment.Username);
            appointment.Price *= (1 - discount);

            _context.SaveChanges();
            return ServiceResult<Appointment>.Ok(appointment);
        }

        /// <summary>
        /// Same logic as sp_GetUserDiscount: 10% when user already has more than 3 appointments.
        /// Works with SQLite and SQL Server (no stored procedure required).
        /// </summary>
        private decimal GetLoyaltyDiscountRate(string username)
        {
            var count = _context.Appointments.Count(a => a.Username == username);
            return count > 3 ? 0.10m : 0m;
        }

        public ServiceResult<object?> Delete(string username, int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
            {
                return ServiceResult<object?>.NotFound();
            }

            if (appointment.Username != username)
            {
                return ServiceResult<object?>.Forbid();
            }

            if (appointment.Date.Date == DateTime.Now.Date)
            {
                return ServiceResult<object?>.BadRequest("Cannot delete appointments for today");
            }

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
            return ServiceResult<object?>.Ok(null);
        }

        public ServiceResult<List<Appointment>> GetFiltered(string username, DateTime? date, string? customerName)
        {
            var query = _context.Appointments.Where(a => a.Username == username);

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
    }
}
