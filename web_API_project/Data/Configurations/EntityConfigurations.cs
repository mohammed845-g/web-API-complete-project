using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using web_API_project.Entities;

namespace web_API_project.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.HasKey(patient => patient.Id);
            builder.Property(patient => patient.Name).IsRequired().HasMaxLength(100);
            builder.Property(patient => patient.DateOfBirth).IsRequired();
        }
    }

    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.HasKey(doctor => doctor.Id);
            builder.Property(doctor => doctor.Name).IsRequired().HasMaxLength(100);
            builder.Property(doctor => doctor.Specialization).IsRequired().HasMaxLength(100);
        }
    }

    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasKey(appointment => new { appointment.PatientId, appointment.DoctorId, appointment.AppointmentDate });

            builder.HasOne(appointment => appointment.Patient)
                   .WithMany(patient => patient.Appointments)
                   .HasForeignKey(appointment => appointment.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(appointment => appointment.Doctor)
                   .WithMany(doctor => doctor.Appointments)
                   .HasForeignKey(appointment => appointment.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
