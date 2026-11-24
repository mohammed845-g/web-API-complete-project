using Microsoft.AspNetCore.Mvc;
using web_API_project.Common;
using web_API_project.Dtos;
using web_API_project.Services;

namespace web_API_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController(IDoctorService doctors) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DoctorDto>>>> GetAll(
            [FromQuery] string? search, [FromQuery] string? specialization, [FromQuery] PaginationParams pagination)
        {
            var result = await doctors.SearchAsync(search, specialization, pagination);
            return Ok(ApiResponse<PaginatedResponse<DoctorDto>>.SuccessResponse(result));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<DoctorDto>>> GetById(int id)
        {
            var doctor = await doctors.GetByIdAsync(id);
            return doctor is null
                ? NotFound(ApiResponse<DoctorDto>.FailResponse($"Doctor with ID {id} was not found.", 404))
                : Ok(ApiResponse<DoctorDto>.SuccessResponse(doctor));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DoctorDto>>> Create(CreateDoctorDto dto)
        {
            var doctor = await doctors.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = doctor.Id },
                ApiResponse<DoctorDto>.SuccessResponse(doctor, "Doctor created.", 201));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(int id, UpdateDoctorDto dto) =>
            await doctors.UpdateAsync(id, dto)
                ? Ok(ApiResponse<string>.SuccessResponse("Doctor updated."))
                : NotFound(ApiResponse<string>.FailResponse($"Doctor with ID {id} was not found.", 404));

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id) =>
            await doctors.DeleteAsync(id)
                ? Ok(ApiResponse<string>.SuccessResponse("Doctor deleted."))
                : NotFound(ApiResponse<string>.FailResponse($"Doctor with ID {id} was not found.", 404));
    }
}
