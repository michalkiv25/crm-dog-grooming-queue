namespace DogQueueApi.Models;

/// <summary>Scalar row returned by <c>dbo.sp_AppointmentSlotTaken</c> (SQL Server).</summary>
public class SlotTakenProcRow
{
    public int Taken { get; set; }
}
