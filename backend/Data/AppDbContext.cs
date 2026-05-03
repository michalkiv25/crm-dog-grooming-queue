using Microsoft.EntityFrameworkCore;
using DogQueueApi.Models;

namespace DogQueueApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        /// <summary>SQL VIEW vw_AppointmentsWithUsers (created at startup by schema installers).</summary>
        public DbSet<AppointmentWithUserView> AppointmentWithUserViews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .Property(a => a.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AppointmentWithUserView>(e =>
            {
                e.HasNoKey();
                e.ToView("vw_AppointmentsWithUsers");
            });
        }
    }
}