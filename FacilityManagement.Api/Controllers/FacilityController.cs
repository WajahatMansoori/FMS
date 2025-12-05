using System.Net;
using FacilityManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Base;

namespace FacilityManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilityController : BaseController
    {
        private readonly IFacilityService _facilityService;

        public FacilityController(IFacilityService facilityService) 
        {
            _facilityService = facilityService;
        }

        [HttpGet("get-all-facilities")]
        public async Task<IActionResult> GetAllFacilitiesAsync()
        {
            try
            {
                var response = await _facilityService.GetAllFacilitesAsync();
                return GenerateResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet("get-facility-resources")]
        public async Task<IActionResult> GetFacilityResourcesAsync(int facilityId)
        {
            try
            {
                var response = await _facilityService.GetFacilityResources(facilityId);
                return GenerateResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
