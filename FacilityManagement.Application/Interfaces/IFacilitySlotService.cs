using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityManagement.Application.DTOs.Request;
using FacilityManagement.Application.DTOs.Response;
using Shared.Base.Responses;

namespace FacilityManagement.Application.Interfaces
{
    public interface IFacilitySlotService
    {
        Task<BaseResponse<Task>> AddFacilitySlotAsync(AddFacilitySlotRequestDTO request);

        Task<BaseResponse<List<AvailableSlotsResponseDTO>>> GetAllAvailableSlotsAsync(int facilityResourceId, DateOnly date);
    }
}
