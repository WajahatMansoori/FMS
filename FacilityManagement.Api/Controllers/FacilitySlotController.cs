using FacilityManagement.Application.Interfaces;
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Base;

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
        public FacilitySlotController() 
        {
        }

    }
}
