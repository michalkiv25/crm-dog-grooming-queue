using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace DogQueueApi.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private static List<Appointment> appointments = new();
        private static int nextId = 1;

        // ➕ יצירת תור
        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] Appointment appt)
        {
            var username = User.Identity?.Name;

            appt.Id = nextId++;
            appt.Username = username;

            appointments.Add(appt);

            return Ok(appt);
        }

        // 📋 קבלת תורים של המשתמש
        [Authorize]
        [HttpGet]
        public IActionResult GetMyAppointments()
        {
            var username = User.Identity?.Name;

            var userAppointments = appointments
                .Where(a => a.Username == username)
                .ToList();

            return Ok(userAppointments);
        }

        // ❌ מחיקה
        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var username = User.Identity?.Name;

            var appointment = appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.Username != username)
                return Forbid("You can only delete your own appointments");

            appointments.Remove(appointment);

            return Ok(new { message = "Deleted successfully" });
        }

        // ✏️ עדכון
        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Appointment updated)
        {
            var username = User.Identity?.Name;

            var appointment = appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.Username != username)
                return Forbid("You can only edit your own appointments");

            appointment.DogName = updated.DogName;
            appointment.Date = updated.Date;

            return Ok(appointment);
        }
    }
}