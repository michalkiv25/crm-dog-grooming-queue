using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Interfaces.Providers;
using DogQueueApi.Models;
using Microsoft.AspNetCore.Authorization;
using DogQueueApi.Services;

namespace DogQueueApi.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsManager _appointmentsManager;
        private readonly ICurrentUserProvider _currentUserProvider;

        public AppointmentsController(
            IAppointmentsManager appointmentsManager,
            ICurrentUserProvider currentUserProvider)
        {
            _appointmentsManager = appointmentsManager;
            _currentUserProvider = currentUserProvider;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var username = _currentUserProvider.GetUsername(User);
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized(new { message = "Invalid token" });

            var result = _appointmentsManager.GetAll(username);
            return ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] Appointment appt)
        {
            var username = _currentUserProvider.GetUsername(User);
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized(new { message = "Invalid token" });

            var result = _appointmentsManager.Create(username, appt);
            return ToActionResult(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Appointment updatedAppt)
        {
            var username = _currentUserProvider.GetUsername(User);
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized(new { message = "Invalid token" });

            var result = _appointmentsManager.Update(username, id, updatedAppt);
            return ToActionResult(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var username = _currentUserProvider.GetUsername(User);
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized(new { message = "Invalid token" });

            var result = _appointmentsManager.Delete(username, id);
            return ToActionResult(result);
        }

        [Authorize]
        [HttpGet("filter")]
        public IActionResult GetFiltered(DateTime? date, string? customerName)
        {
            var username = _currentUserProvider.GetUsername(User);
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized(new { message = "Invalid token" });

            var result = _appointmentsManager.GetFiltered(username, date, customerName);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(ServiceResult<T> result)
        {
            var payload = result.Errors?.Length > 0
                ? new { message = result.Message, errors = result.Errors }
                : result.Data ?? (object)new { message = result.Message };

            return result.StatusCode switch
            {
                200 => Ok(payload),
                400 => BadRequest(payload),
                401 => Unauthorized(payload),
                403 => StatusCode(403, payload),
                404 => NotFound(payload),
                _ => StatusCode(result.StatusCode, payload)
            };
        }
    }
}