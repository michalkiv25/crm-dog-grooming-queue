using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Models;

namespace DogQueueApi.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private static List<Appointment> appointments = new();
        private static int nextId = 1;

        // ➕ יצירת תור (רק מחובר)
        [Authorize]
        [HttpPost]
        public IActionResult Create(Appointment appt)
        {
            var username = User.Identity?.Name;

            appt.Id = nextId++;
            appt.Username = username!;

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
  


        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var username = User.Identity?.Name;

            var appointment = appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            // רק בעל התור יכול למחוק
            if (appointment.Username != username)
                return Forbid("You can only delete your own appointments");

            appointments.Remove(appointment);

            return Ok(new { message = "Deleted successfully" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(int id, Appointment updated)
        {
            var username = User.Identity?.Name;

            var appointment = appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.Username != username)
                return Forbid("You can only edit your own appointments");

            // עדכון שדות
            appointment.DogName = updated.DogName;
            appointment.Date = updated.Date;

            return Ok(appointment);
        }
        }  }