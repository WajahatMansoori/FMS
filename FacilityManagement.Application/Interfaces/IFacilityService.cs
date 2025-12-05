using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityManagement.Application.DTOs.Response;
using Shared.Base.Responses;

namespace FacilityManagement.Application.Interfaces
{
    public interface IFacilityService
    {
        Task<BaseResponse<List<FacilitesResponseDTO>?>> GetAllFacilitesAsync();

        Task<BaseResponse<List<FacilityResourcesResponseDTO>?>> GetFacilityResources(int facilityId);
    }
}
