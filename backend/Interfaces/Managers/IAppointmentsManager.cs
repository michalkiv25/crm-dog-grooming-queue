using DogQueueApi.Models;
using DogQueueApi.Services;

namespace DogQueueApi.Interfaces.Managers
{
    public interface IAppointmentsManager
    {
        ServiceResult<List<Appointment>> GetAll(string username);
        ServiceResult<Appointment> Create(string username, Appointment appointment);
        ServiceResult<Appointment> Update(string username, int id, Appointment updatedAppointment);
        ServiceResult<object?> Delete(string username, int id);
        ServiceResult<List<Appointment>> GetFiltered(string username, DateTime? date, string? customerName);

        ServiceResult<LoyaltyBookingPreview> GetLoyaltyBookingPreview(string? username);

        /// <summary>All customers’ appointments from now onward (salon-wide queue). Edit/delete still enforced per-user on PUT/DELETE.</summary>
        ServiceResult<List<Appointment>> GetUpcomingQueue();

        /// <summary>Same queue window as <see cref="GetUpcomingQueue"/> but rows read from SQL VIEW <c>vw_AppointmentsWithUsers</c> (includes FullName).</summary>
        ServiceResult<List<AppointmentWithUserView>> GetUpcomingAppointmentsWithUserInfo();
    }
}
