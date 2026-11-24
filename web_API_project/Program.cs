using Microsoft.EntityFrameworkCore;
using web_API_project.Data;
using web_API_project.Extensions;
using web_API_project.Filters;
using web_API_project.Middlewares;

namespace web_API_project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddConsole();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelAttribute>();
                options.Filters.Add<ExecutionTimerAttribute>();
            });
            builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
                options.SuppressModelStateInvalidFilter = true);
            builder.Services.AddOpenApi();
            builder.Services.AddCors(options => options.AddPolicy("ClientApps", policy =>
                policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()));
            builder.Services.AddDbContext<ApplicationDbContext>((services, options) =>
                options.UseInMemoryDatabase("HealthCareSystem")
                       .AddInterceptors(services.GetRequiredService<web_API_project.Data.Interceptors.AuditSaveChangesInterceptor>()));
            builder.Services.AddHealthCareServices();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                database.Database.EnsureCreated();
            }

            app.UseMiddleware<GlobalExceptionMiddleware>();
            if (app.Environment.IsDevelopment()) app.MapOpenApi();
            app.UseHttpsRedirection();
            app.UseCors("ClientApps");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
