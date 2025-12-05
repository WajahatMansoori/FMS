using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.DTOs.Response
{
    public class AddFacilitySlotResponseDTO
    {
        public int TotalSlotsCreated { get; set; }
        public int SkippedSlots { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
