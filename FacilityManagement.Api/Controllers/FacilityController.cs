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

        [HttpGet]
        public async Task<IActionResult> GetAllFacilities()
        {
            try
            {
                var response = await _facilityService.GetAllFacilites();
                return GenerateResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
