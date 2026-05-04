using System.Globalization;
using DogQueue.WebApi.Interfaces.Managers;
using DogQueue.WebApi.Interfaces.Providers;
using DogQueue.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogQueue.WebApi.Controllers;

/// <summary>HTTP API for appointments — delegates to <see cref="IAppointmentsManager"/>.</summary>
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
    [HttpGet("upcoming-queue")]
    public IActionResult GetUpcomingQueue()
    {
        var username = _currentUserProvider.GetUsername(User);
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(new { message = "Invalid token" });

        var result = _appointmentsManager.GetUpcomingQueue();
        return ToActionResult(result);
    }

    /// <summary>Full salon list: every customer’s appointments (read-only for others; edit/delete via ownership checks).</summary>
    [Authorize]
    [HttpGet("all-appointments")]
    public IActionResult GetAllSalonAppointments()
    {
        var username = _currentUserProvider.GetUsername(User);
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(new { message = "Invalid token" });

        var result = _appointmentsManager.GetSalonAppointments();
        return ToActionResult(result);
    }

    /// <summary>Check if a calendar minute is free (salon-wide). Pass <c>at</c> as ISO-8601 (e.g. from <c>Date.toISOString()</c>).</summary>
    [Authorize]
    [HttpGet("check-slot")]
    public IActionResult CheckSlot([FromQuery] string at, [FromQuery] int? excludeId)
    {
        var username = _currentUserProvider.GetUsername(User);
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(new { message = "Invalid token" });

        if (string.IsNullOrWhiteSpace(at))
            return BadRequest(new { message = "Query 'at' is required." });

        if (!DateTime.TryParse(at, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
            return BadRequest(new { message = "Invalid ISO datetime for 'at'." });

        var taken = _appointmentsManager.IsSlotTaken(parsed, excludeId);
        return Ok(new { available = !taken, taken });
    }

    /// <summary>All booked start times (minute precision, ISO), for greying out times in the date picker.</summary>
    [Authorize]
    [HttpGet("booked-slot-times")]
    public IActionResult GetBookedSlotTimes()
    {
        var username = _currentUserProvider.GetUsername(User);
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(new { message = "Invalid token" });

        var result = _appointmentsManager.GetBookedSlotIsoTimes();
        return ToActionResult(result);
    }

    [Authorize]
    [HttpGet("upcoming-with-user-info")]
    public IActionResult GetUpcomingWithUserInfo()
    {
        var username = _currentUserProvider.GetUsername(User);
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(new { message = "Invalid token" });

        var result = _appointmentsManager.GetUpcomingAppointmentsWithUserInfo();
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
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Appointment updatedAppt)
    {
        var username = _currentUserProvider.GetUsername(User);
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized(new { message = "Invalid token" });

        var result = _appointmentsManager.Update(username, id, updatedAppt);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
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
