using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.DTOs.Response
{
    public class EmployeeResponseDTO
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? EmployeePhoto { get; set; }
        public int FacilityRoleId { get; set; }
        public string? FacilityRoleName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? OfficialEmailAddress { get; set; }
        public string? PhoneNo { get; set; }
        public string? MobileNo { get; set; }
        public int DesignationId { get; set; }
        public string? DesignationName { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }

    }
}
