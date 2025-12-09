using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.DTOs.Response
{
    public class EmployeeQuickSearchResponseDTO
    {
        public int EmployeeId { get; set; }
        public string? FullName { get; set; }
        public int FacilityRoleId { get; set; }
        public string? FacilityRoleName { get; set; }
        public string? EmployeePhoto { get; set; }
    }
}
