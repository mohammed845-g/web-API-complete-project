using Microsoft.AspNetCore.Mvc;
using web_API_project.Common;
using web_API_project.Dtos;
using web_API_project.Services;

namespace web_API_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController(IAppointmentService appointments) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<AppointmentDto>>>> GetAll(
            [FromQuery] string? search, [FromQuery] int? patientId, [FromQuery] int? doctorId,
            [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] PaginationParams pagination)
        {
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
                return BadRequest(ApiResponse<PaginatedResponse<AppointmentDto>>.FailResponse("Appointment date range is invalid."));
            var result = await appointments.SearchAsync(search, patientId, doctorId, dateFrom, dateTo, pagination);
            return Ok(ApiResponse<PaginatedResponse<AppointmentDto>>.SuccessResponse(result));
        }

        [HttpGet("{patientId:int}/{doctorId:int}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetByKey(
            int patientId, int doctorId, [FromQuery] DateTime? appointmentDate)
        {
            if (!appointmentDate.HasValue)
                return BadRequest(ApiResponse<AppointmentDto>.FailResponse("appointmentDate is required."));
            var appointment = await appointments.GetByKeyAsync(patientId, doctorId, appointmentDate.Value);
            return appointment is null
                ? NotFound(ApiResponse<AppointmentDto>.FailResponse("Appointment was not found.", 404))
                : Ok(ApiResponse<AppointmentDto>.SuccessResponse(appointment));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> Create(CreateAppointmentDto dto)
        {
            var appointment = await appointments.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByKey),
                new { patientId = appointment.PatientId, doctorId = appointment.DoctorId, appointmentDate = appointment.AppointmentDate },
                ApiResponse<AppointmentDto>.SuccessResponse(appointment, "Appointment created.", 201));
        }

        [HttpPut("{patientId:int}/{doctorId:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(
            int patientId, int doctorId, [FromQuery] DateTime? appointmentDate, UpdateAppointmentDto dto)
        {
            if (!appointmentDate.HasValue)
                return BadRequest(ApiResponse<string>.FailResponse("appointmentDate is required."));
            var updated = await appointments.UpdateAsync(patientId, doctorId, appointmentDate.Value, dto);
            return updated
                ? Ok(ApiResponse<string>.SuccessResponse("Appointment updated."))
                : NotFound(ApiResponse<string>.FailResponse("Appointment was not found.", 404));
        }

        [HttpDelete("{patientId:int}/{doctorId:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(
            int patientId, int doctorId, [FromQuery] DateTime? appointmentDate)
        {
            if (!appointmentDate.HasValue)
                return BadRequest(ApiResponse<string>.FailResponse("appointmentDate is required."));
            return await appointments.DeleteAsync(patientId, doctorId, appointmentDate.Value)
                ? Ok(ApiResponse<string>.SuccessResponse("Appointment deleted."))
                : NotFound(ApiResponse<string>.FailResponse("Appointment was not found.", 404));
        }
    }
}
