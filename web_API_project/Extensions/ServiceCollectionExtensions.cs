using web_API_project.Data.Interceptors;
using web_API_project.Filters;
using web_API_project.Mapping;
using web_API_project.Repositories;
using web_API_project.Services;

namespace web_API_project.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthCareServices(this IServiceCollection services)
        {
            services.AddAutoMapper(configuration => configuration.AddProfile<MappingProfile>());
            services.AddSingleton<AuditSaveChangesInterceptor>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<ValidateModelAttribute>();
            services.AddScoped<ExecutionTimerAttribute>();
            return services;
        }
    }
}
