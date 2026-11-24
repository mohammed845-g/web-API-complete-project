using AutoMapper;
using web_API_project.Dtos;
using web_API_project.Entities;

namespace web_API_project.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Patient, PatientDto>();
            CreateMap<CreatePatientDto, Patient>();
            CreateMap<UpdatePatientDto, Patient>();
            CreateMap<Doctor, DoctorDto>();
            CreateMap<CreateDoctorDto, Doctor>();
            CreateMap<UpdateDoctorDto, Doctor>();
            CreateMap<CreateAppointmentDto, Appointment>();
            CreateMap<UpdateAppointmentDto, Appointment>();
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dto => dto.PatientName, options => options.MapFrom(appointment => appointment.Patient.Name))
                .ForMember(dto => dto.DoctorName, options => options.MapFrom(appointment => appointment.Doctor.Name));
        }
    }
}
