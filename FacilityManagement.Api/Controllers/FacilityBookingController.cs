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
    public class FacilityBookingController : BaseController
    {
        private readonly IFacilityBookingService _facilityBookingService;

        public FacilityBookingController(IFacilityBookingService facilityBookingService)
        {
            _facilityBookingService = facilityBookingService;
        }

        [HttpPost()]
        public async Task<IActionResult> AddAsync([FromBody] BookingRequestDTO bookingRequestDTO)
        {
            try
            {
                var response = await _facilityBookingService.AddAsync(bookingRequestDTO);
                return GenerateResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("cancel-slot")]
        public async Task<IActionResult> CancelSlotAsync([FromBody] CancelSlotRequestDTO cancelSlotRequestDTO)
        {
            try
            {
                var response = await _facilityBookingService.CancelSlotAsync(cancelSlotRequestDTO);
                return GenerateResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
