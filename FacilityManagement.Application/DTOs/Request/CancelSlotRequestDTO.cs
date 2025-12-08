using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.DTOs.Request
{
    public class CancelSlotRequestDTO
    {
        public int SlotId { get; set; }
        public int EmployeeId { get; set; }
        public string? Remarks { get; set; }
    }
}
