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
    }
}