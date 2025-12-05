using FacilityManagement.Application.DTOs.Request;
using FacilityManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Base;
using System.Net;

namespace FacilityManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilitySlotController : BaseController
    {
        private readonly IFacilitySlotService _facilitySlotService;

        public FacilitySlotController(IFacilitySlotService facilitySlotService) 
        {
            _facilitySlotService = facilitySlotService;
        }

        [HttpPost("add-facility-slot")]
        public async Task<IActionResult> AddFacilitySlotAsync([FromBody] AddFacilitySlotRequestDTO request)
        {
            try
            {
                var response = await _facilitySlotService.AddFacilitySlotAsync(request);
                return GenerateResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
