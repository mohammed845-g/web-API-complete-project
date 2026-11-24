using Microsoft.AspNetCore.Mvc;
using web_API_project.Common;
using web_API_project.Dtos;
using web_API_project.Services;

namespace web_API_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController(IPatientService patients) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PatientDto>>>> GetAll(
            [FromQuery] string? search, [FromQuery] DateTime? dateOfBirthFrom, [FromQuery] DateTime? dateOfBirthTo,
            [FromQuery] PaginationParams pagination)
        {
            if (dateOfBirthFrom.HasValue && dateOfBirthTo.HasValue && dateOfBirthFrom > dateOfBirthTo)
                return BadRequest(ApiResponse<PaginatedResponse<PatientDto>>.FailResponse("Date of birth range is invalid."));
            var result = await patients.SearchAsync(search, dateOfBirthFrom, dateOfBirthTo, pagination);
            return Ok(ApiResponse<PaginatedResponse<PatientDto>>.SuccessResponse(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PatientDto>>> GetById(int id)
        {
            var patient = await patients.GetByIdAsync(id);
            return patient is null
                ? NotFound(ApiResponse<PatientDto>.FailResponse($"Patient with ID {id} was not found.", 404))
                : Ok(ApiResponse<PatientDto>.SuccessResponse(patient));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PatientDto>>> Create(CreatePatientDto dto)
        {
            var patient = await patients.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = patient.Id },
                ApiResponse<PatientDto>.SuccessResponse(patient, "Patient created.", 201));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(int id, UpdatePatientDto dto) =>
            await patients.UpdateAsync(id, dto)
                ? Ok(ApiResponse<string>.SuccessResponse("Patient updated."))
                : NotFound(ApiResponse<string>.FailResponse($"Patient with ID {id} was not found.", 404));

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id) =>
            await patients.DeleteAsync(id)
                ? Ok(ApiResponse<string>.SuccessResponse("Patient deleted."))
                : NotFound(ApiResponse<string>.FailResponse($"Patient with ID {id} was not found.", 404));
    }
}
