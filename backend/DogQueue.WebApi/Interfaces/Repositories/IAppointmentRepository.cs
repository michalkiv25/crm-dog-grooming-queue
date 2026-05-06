using DogQueue.WebApi.Models;

namespace DogQueue.WebApi.Interfaces.Repositories;

public interface IAppointmentRepository
{
    string DatabaseProviderName { get; }

    IQueryable<Appointment> QueryForUser(string canonicalUsername);

    Appointment? FindTracked(int id);

    void Add(Appointment appointment);

    void Remove(Appointment appointment);

    void SaveChanges();

    Appointment FirstNoTracking(int id);

    List<Appointment> ListUpcoming(DateTime now);

    /// <summary>Every saved appointment (all customers), ordered by date.</summary>
    List<Appointment> ListAllOrderByDate();

    List<AppointmentWithUserView> ListUpcomingWithUsers(DateTime now);

    /// <summary>Loyalty repricing: ascending scheduled <c>Date</c>, then <c>Id</c> for ties.</summary>
    List<Appointment> ListOrderedByDateThenIdNoTrackingForUser(string canonicalUsername);

    void ExecuteUpdatePriceAndDuration(int appointmentId, decimal price, int durationMinutes);
}
