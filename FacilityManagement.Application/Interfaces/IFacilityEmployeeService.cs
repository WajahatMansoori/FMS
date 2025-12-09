using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityManagement.Application.DTOs.Response;
using Shared.Base.Responses;

namespace FacilityManagement.Application.Interfaces
{
    public interface IFacilityEmployeeService
    {
        Task<BaseResponse<EmployeeResponseDTO?>> GetEmployeeByIdAsync(int employeeId);
        Task<BaseResponse<List<EmployeeQuickSearchResponseDTO>>> GetEmployeeQuickSearch(string keyword);
    }
}
