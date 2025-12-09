using FacilityManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Base;

namespace FacilityManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FacilityEmployeeController : BaseController
    {
        private readonly IFacilityEmployeeService _facilityEmployeeService;

        public FacilityEmployeeController(IFacilityEmployeeService facilityEmployeeService)
        {
            _facilityEmployeeService = facilityEmployeeService;
        }

        [HttpGet("get-employee-by-id")]
        public async Task<IActionResult> GetEmployeeByIdAsync(int employeeId)
        {
            try
            {
                var response = await _facilityEmployeeService.GetEmployeeByIdAsync(employeeId);
                return GenerateResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
