using AutoMapper;
using web_API_project.Common;
using web_API_project.Dtos;
using web_API_project.Entities;
using web_API_project.Repositories;

namespace web_API_project.Services
{
    public interface IPatientService
    {
        Task<PaginatedResponse<PatientDto>> SearchAsync(string? search, DateTime? dateOfBirthFrom, DateTime? dateOfBirthTo, PaginationParams pagination);
        Task<PatientDto?> GetByIdAsync(int id);
        Task<PatientDto> CreateAsync(CreatePatientDto dto);
        Task<bool> UpdateAsync(int id, UpdatePatientDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public class PatientService(IPatientRepository repository, IAppointmentRepository appointments, IMapper mapper) : IPatientService
    {
        public async Task<PaginatedResponse<PatientDto>> SearchAsync(string? search, DateTime? dateOfBirthFrom, DateTime? dateOfBirthTo, PaginationParams pagination)
        {
            var (items, count) = await repository.SearchAsync(search, dateOfBirthFrom, dateOfBirthTo, pagination);
            return new PaginatedResponse<PatientDto>
            {
                PageNumber = pagination.PageNumber, PageSize = pagination.PageSize, TotalCount = count,
                Items = mapper.Map<List<PatientDto>>(items)
            };
        }

        public async Task<PatientDto?> GetByIdAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id);
            return entity is null ? null : mapper.Map<PatientDto>(entity);
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
        {
            var entity = mapper.Map<Patient>(dto);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            return mapper.Map<PatientDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdatePatientDto dto)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity is null) return false;
            mapper.Map(dto, entity);
            repository.Update(entity);
            await repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity is null) return false;
            await appointments.DeleteByPatientIdAsync(id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            return true;
        }
    }

    public interface IDoctorService
    {
        Task<PaginatedResponse<DoctorDto>> SearchAsync(string? search, string? specialization, PaginationParams pagination);
        Task<DoctorDto?> GetByIdAsync(int id);
        Task<DoctorDto> CreateAsync(CreateDoctorDto dto);
        Task<bool> UpdateAsync(int id, UpdateDoctorDto dto);
        Task<bool> DeleteAsync(int id);
    }

    public class DoctorService(IDoctorRepository repository, IAppointmentRepository appointments, IMapper mapper) : IDoctorService
    {
        public async Task<PaginatedResponse<DoctorDto>> SearchAsync(string? search, string? specialization, PaginationParams pagination)
        {
            var (items, count) = await repository.SearchAsync(search, specialization, pagination);
            return new PaginatedResponse<DoctorDto>
            {
                PageNumber = pagination.PageNumber, PageSize = pagination.PageSize, TotalCount = count,
                Items = mapper.Map<List<DoctorDto>>(items)
            };
        }

        public async Task<DoctorDto?> GetByIdAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id);
            return entity is null ? null : mapper.Map<DoctorDto>(entity);
        }

        public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
        {
            var entity = mapper.Map<Doctor>(dto);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            return mapper.Map<DoctorDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateDoctorDto dto)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity is null) return false;
            mapper.Map(dto, entity);
            repository.Update(entity);
            await repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity is null) return false;
            await appointments.DeleteByDoctorIdAsync(id);
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            return true;
        }
    }

    public interface IAppointmentService
    {
        Task<PaginatedResponse<AppointmentDto>> SearchAsync(string? search, int? patientId, int? doctorId, DateTime? dateFrom, DateTime? dateTo, PaginationParams pagination);
        Task<AppointmentDto?> GetByKeyAsync(int patientId, int doctorId, DateTime appointmentDate);
        Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
        Task<bool> UpdateAsync(int patientId, int doctorId, DateTime oldAppointmentDate, UpdateAppointmentDto dto);
        Task<bool> DeleteAsync(int patientId, int doctorId, DateTime appointmentDate);
    }

    public class AppointmentService(
        IAppointmentRepository repository, IPatientRepository patients, IDoctorRepository doctors, IMapper mapper) : IAppointmentService
    {
        public async Task<PaginatedResponse<AppointmentDto>> SearchAsync(string? search, int? patientId, int? doctorId, DateTime? dateFrom, DateTime? dateTo, PaginationParams pagination)
        {
            var (items, count) = await repository.SearchAsync(search, patientId, doctorId, dateFrom, dateTo, pagination);
            return new PaginatedResponse<AppointmentDto>
            {
                PageNumber = pagination.PageNumber, PageSize = pagination.PageSize, TotalCount = count,
                Items = mapper.Map<List<AppointmentDto>>(items)
            };
        }

        public async Task<AppointmentDto?> GetByKeyAsync(int patientId, int doctorId, DateTime appointmentDate)
        {
            var entity = await repository.GetByKeyAsync(patientId, doctorId, appointmentDate);
            return entity is null ? null : mapper.Map<AppointmentDto>(entity);
        }

        public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
        {
            await EnsureReferencesExistAsync(dto.PatientId, dto.DoctorId);
            if (await repository.GetByKeyAsync(dto.PatientId, dto.DoctorId, dto.AppointmentDate) is not null)
                throw new InvalidOperationException("An appointment already exists for this patient, doctor, and date.");

            var entity = mapper.Map<Appointment>(dto);
            await repository.AddAsync(entity);
            await repository.SaveChangesAsync();
            return (await GetByKeyAsync(dto.PatientId, dto.DoctorId, dto.AppointmentDate))!;
        }

        public async Task<bool> UpdateAsync(int patientId, int doctorId, DateTime oldAppointmentDate, UpdateAppointmentDto dto)
        {
            var current = await repository.GetByKeyAsync(patientId, doctorId, oldAppointmentDate);
            if (current is null) return false;
            await EnsureReferencesExistAsync(dto.PatientId, dto.DoctorId);

            var target = await repository.GetByKeyAsync(dto.PatientId, dto.DoctorId, dto.AppointmentDate);
            if (target is not null && (target.PatientId != patientId || target.DoctorId != doctorId || target.AppointmentDate != oldAppointmentDate))
                throw new InvalidOperationException("An appointment already exists for the requested patient, doctor, and date.");

            if (target is not null) return true;
            repository.Delete(current);
            await repository.SaveChangesAsync();
            await repository.AddAsync(mapper.Map<Appointment>(dto));
            await repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int patientId, int doctorId, DateTime appointmentDate)
        {
            var entity = await repository.GetByKeyAsync(patientId, doctorId, appointmentDate);
            if (entity is null) return false;
            repository.Delete(entity);
            await repository.SaveChangesAsync();
            return true;
        }

        private async Task EnsureReferencesExistAsync(int patientId, int doctorId)
        {
            if (await patients.GetByIdAsync(patientId) is null)
                throw new InvalidOperationException($"Patient with ID {patientId} does not exist.");
            if (await doctors.GetByIdAsync(doctorId) is null)
                throw new InvalidOperationException($"Doctor with ID {doctorId} does not exist.");
        }
    }
}
