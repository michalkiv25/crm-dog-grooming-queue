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
    }
}
