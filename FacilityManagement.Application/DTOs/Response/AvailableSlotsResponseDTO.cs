using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.DTOs.Response
{
    public class AvailableSlotsResponseDTO
    {
        public int SlotId { get; set; }
        public TimeOnly? SlotStartTime { get; set; }
        public TimeOnly? SlotEndTime { get; set; }
    }
}
