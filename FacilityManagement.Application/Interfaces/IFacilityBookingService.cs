using FacilityManagement.Application.DTOs.Request;
using Shared.Base.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.Interfaces
{
    public interface IFacilityBookingService
    {
        Task<BaseResponse<Task>> AddAsync(BookingRequestDTO bookingRequestDTO);
    }
}
