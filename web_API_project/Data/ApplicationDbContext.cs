using Microsoft.EntityFrameworkCore;
using web_API_project.Entities;

namespace web_API_project.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<Patient>().HasData(
                new Patient { Id = 1, Name = "Ahmed Ali", DateOfBirth = new DateTime(1990, 1, 15) },
                new Patient { Id = 2, Name = "Mona Hassan", DateOfBirth = new DateTime(1985, 6, 20) });

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { Id = 1, Name = "Omar Khaled", Specialization = "Cardiology" },
                new Doctor { Id = 2, Name = "Sara Mostafa", Specialization = "Pediatrics" });

            modelBuilder.Entity<Appointment>().HasData(
                new Appointment { PatientId = 1, DoctorId = 1, AppointmentDate = new DateTime(2026, 12, 1, 10, 0, 0) },
                new Appointment { PatientId = 2, DoctorId = 2, AppointmentDate = new DateTime(2026, 12, 2, 11, 0, 0) });
        }
    }
}
