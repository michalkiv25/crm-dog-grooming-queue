using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Data;
using DogQueueApi.Models;
using DogQueueApi.Validators;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DogQueueApi.Controllers
{
    public class DiscountResult
    {
        public int TotalAppointments { get; set; }
        public decimal Discount { get; set; }
    }

    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var username = User.Identity?.Name;
            var appointments = _context.Appointments.Where(a => a.Username == username).ToList();
            return Ok(appointments);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] Appointment appt)
        {
            // Validate appointment input
            var (isValid, errors) = AppointmentValidator.Validate(appt);
            if (!isValid)
            {
                return BadRequest(new { message = "Validation failed", errors });
            }

            appt.Username = User.Identity?.Name ?? appt.Username;
            appt.CalculatePriceAndDuration();

            // Get discount using stored procedure
            var discountResults = _context.Database.SqlQuery<DiscountResult>(
                $"EXEC sp_GetUserDiscount @Username = {appt.Username}").ToList();
            var discount = discountResults.FirstOrDefault()?.Discount ?? 0;
            appt.Price *= (1 - discount);

            _context.Appointments.Add(appt);
            _context.SaveChanges();

            return Ok(appt);
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Appointment updatedAppt)
        {
            // Validate appointment input
            var (isValid, errors) = AppointmentValidator.Validate(updatedAppt);
            if (!isValid)
            {
                return BadRequest(new { message = "Validation failed", errors });
            }

            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt == null) return NotFound();

            if (appt.Username != User.Identity?.Name) return Forbid();

            appt.DogName = updatedAppt.DogName;
            appt.DogSize = updatedAppt.DogSize;
            appt.Date = updatedAppt.Date;
            appt.CalculatePriceAndDuration();

            // Recalculate discount
            var discountResults = _context.Database.SqlQuery<DiscountResult>(
                $"EXEC sp_GetUserDiscount @Username = {appt.Username}").ToList();
            var discount = discountResults.FirstOrDefault()?.Discount ?? 0;
            appt.Price *= (1 - discount);

            _context.SaveChanges();
            return Ok(appt);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt == null) return NotFound();

            if (appt.Username != User.Identity?.Name) return Forbid();

            if (appt.Date.Date == DateTime.Now.Date) return BadRequest("Cannot delete appointments for today");

            _context.Appointments.Remove(appt);
            _context.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpGet("filter")]
        public IActionResult GetFiltered(DateTime? date, string? customerName)
        {
            var username = User.Identity?.Name;
            var query = _context.Appointments.Where(a => a.Username == username);

            if (date.HasValue)
            {
                query = query.Where(a => a.Date.Date == date.Value.Date);
            }

            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(a => a.Username.Contains(customerName));
            }

            return Ok(query.ToList());
        }
    }
}