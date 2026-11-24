using Microsoft.EntityFrameworkCore;
using web_API_project.Common;
using web_API_project.Data;
using web_API_project.Entities;

namespace web_API_project.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveChangesAsync();
    }

    public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext Context = context;
        protected readonly DbSet<T> DbSet = context.Set<T>();

        public async Task<IEnumerable<T>> GetAllAsync() => await DbSet.AsNoTracking().ToListAsync();
        public async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);
        public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);
        public void Update(T entity) => DbSet.Update(entity);
        public void Delete(T entity) => DbSet.Remove(entity);
        public async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();
    }

    public interface IPatientRepository : IGenericRepository<Patient>
    {
        Task<(IEnumerable<Patient> Items, int TotalCount)> SearchAsync(
            string? search, DateTime? dateOfBirthFrom, DateTime? dateOfBirthTo, PaginationParams pagination);
    }

    public class PatientRepository(ApplicationDbContext context)
        : GenericRepository<Patient>(context), IPatientRepository
    {
        public async Task<(IEnumerable<Patient> Items, int TotalCount)> SearchAsync(
            string? search, DateTime? dateOfBirthFrom, DateTime? dateOfBirthTo, PaginationParams pagination)
        {
            var query = DbSet.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(patient => patient.Name.ToLower().Contains(term));
            }
            if (dateOfBirthFrom.HasValue) query = query.Where(patient => patient.DateOfBirth >= dateOfBirthFrom.Value);
            if (dateOfBirthTo.HasValue) query = query.Where(patient => patient.DateOfBirth <= dateOfBirthTo.Value);
            var total = await query.CountAsync();
            var items = await query.OrderBy(patient => patient.Id)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
            return (items, total);
        }
    }

    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        Task<(IEnumerable<Doctor> Items, int TotalCount)> SearchAsync(
            string? search, string? specialization, PaginationParams pagination);
    }

    public class DoctorRepository(ApplicationDbContext context)
        : GenericRepository<Doctor>(context), IDoctorRepository
    {
        public async Task<(IEnumerable<Doctor> Items, int TotalCount)> SearchAsync(
            string? search, string? specialization, PaginationParams pagination)
        {
            var query = DbSet.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(doctor => doctor.Name.ToLower().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(specialization))
            {
                var term = specialization.Trim().ToLower();
                query = query.Where(doctor => doctor.Specialization.ToLower().Contains(term));
            }
            var total = await query.CountAsync();
            var items = await query.OrderBy(doctor => doctor.Id)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
            return (items, total);
        }
    }

    public interface IAppointmentRepository
    {
        Task<(IEnumerable<Appointment> Items, int TotalCount)> SearchAsync(
            string? search, int? patientId, int? doctorId, DateTime? dateFrom, DateTime? dateTo, PaginationParams pagination);
        Task<Appointment?> GetByKeyAsync(int patientId, int doctorId, DateTime appointmentDate);
        Task AddAsync(Appointment appointment);
        Task DeleteByPatientIdAsync(int patientId);
        Task DeleteByDoctorIdAsync(int doctorId);
        void Update(Appointment appointment);
        void Delete(Appointment appointment);
        Task<int> SaveChangesAsync();
    }

    public class AppointmentRepository(ApplicationDbContext context) : IAppointmentRepository
    {
        public async Task<(IEnumerable<Appointment> Items, int TotalCount)> SearchAsync(
            string? search, int? patientId, int? doctorId, DateTime? dateFrom, DateTime? dateTo, PaginationParams pagination)
        {
            var query = context.Appointments.Include(item => item.Patient).Include(item => item.Doctor)
                .AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(item => item.Patient.Name.ToLower().Contains(term) ||
                                           item.Doctor.Name.ToLower().Contains(term) ||
                                           item.Doctor.Specialization.ToLower().Contains(term));
            }
            if (patientId.HasValue) query = query.Where(item => item.PatientId == patientId.Value);
            if (doctorId.HasValue) query = query.Where(item => item.DoctorId == doctorId.Value);
            if (dateFrom.HasValue) query = query.Where(item => item.AppointmentDate >= dateFrom.Value);
            if (dateTo.HasValue) query = query.Where(item => item.AppointmentDate <= dateTo.Value);
            var total = await query.CountAsync();
            var items = await query.OrderBy(item => item.AppointmentDate)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
            return (items, total);
        }

        public async Task<Appointment?> GetByKeyAsync(int patientId, int doctorId, DateTime appointmentDate) =>
            await context.Appointments.Include(item => item.Patient).Include(item => item.Doctor)
                .FirstOrDefaultAsync(item => item.PatientId == patientId && item.DoctorId == doctorId && item.AppointmentDate == appointmentDate);

        public async Task AddAsync(Appointment appointment) => await context.Appointments.AddAsync(appointment);
        public async Task DeleteByPatientIdAsync(int patientId)
        {
            var appointments = await context.Appointments.Where(item => item.PatientId == patientId).ToListAsync();
            context.Appointments.RemoveRange(appointments);
        }

        public async Task DeleteByDoctorIdAsync(int doctorId)
        {
            var appointments = await context.Appointments.Where(item => item.DoctorId == doctorId).ToListAsync();
            context.Appointments.RemoveRange(appointments);
        }

        public void Update(Appointment appointment) => context.Appointments.Update(appointment);
        public void Delete(Appointment appointment) => context.Appointments.Remove(appointment);
        public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
    }
}
