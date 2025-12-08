using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.DTOs.Response
{
    public class FacilitySlotResponseDTO
    {
        public int SlotId { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeePhoto { get; set; }
        public string? FacilityName { get; set; }
        public string? FacilityResourceName { get; set; }
        public DateOnly? SlotDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string? FacilitySlotStatusName { get; set; }
    }
}
