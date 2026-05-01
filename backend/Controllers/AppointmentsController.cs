using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Data;
using DogQueueApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace DogQueueApi.Controllers
{
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
        public IActionResult GetMyAppointments()
        {
            var username = User.Identity?.Name;

            var result = _context.Appointments
                .Where(a => a.Username == username)
                .ToList();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(Appointment appt)
        {
            appt.Username = User.Identity?.Name;

            _context.Appointments.Add(appt);
            _context.SaveChanges();

            return Ok(appt);
        }
    }
}