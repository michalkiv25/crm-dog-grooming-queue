using DogQueueApi.Models;

namespace DogQueueApi.Interfaces.Repositories;

/// <summary>
/// Persistence for appointments (EF + raw SQL). Used only from appointment business logic
/// (<see cref="Managers.AppointmentsManager"/>).
/// </summary>
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

    List<AppointmentWithUserView> ListUpcomingWithUsers(DateTime now);

    List<Appointment> ListOrderedByIdNoTrackingForUser(string canonicalUsername);

    void ExecuteUpdatePriceAndDuration(int appointmentId, decimal price, int durationMinutes);

    LoyaltyPreviewProcRow? ExecLoyaltyPreviewProcedure(string canonicalUsername);

    SlotTakenProcRow? ExecSlotTakenProcedure(DateTime slot, int? excludeAppointmentId);

    bool SlotTakenByLinq(DateTime slot, int? excludeAppointmentId);
}
