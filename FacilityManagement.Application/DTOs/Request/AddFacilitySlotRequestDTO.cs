using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.DTOs.Request
{
    public class AddFacilitySlotRequestDTO
    {
        public int FacilityResourceId { get; set; }
        public DateOnly SlotStartDate { get; set; }
        public DateOnly SlotEndDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsWithWeekend { get; set; }
    }
}
