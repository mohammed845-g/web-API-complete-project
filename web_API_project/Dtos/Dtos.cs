using System.ComponentModel.DataAnnotations;

namespace web_API_project.Dtos
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }

    public class CreatePatientDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }
    }

    public class UpdatePatientDto : CreatePatientDto { }

    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
    }

    public class CreateDoctorDto
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 2)]
        public string Specialization { get; set; } = string.Empty;
    }

    public class UpdateDoctorDto : CreateDoctorDto { }

    public class AppointmentDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
    }

    public class CreateAppointmentDto
    {
        [Range(1, int.MaxValue)]
        public int PatientId { get; set; }

        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }
    }

    public class UpdateAppointmentDto : CreateAppointmentDto { }
}
