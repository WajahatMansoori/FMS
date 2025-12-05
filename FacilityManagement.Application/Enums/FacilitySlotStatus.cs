using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacilityManagement.Application.Enums
{
    public enum FacilitySlotStatus
    {
        Available = 1,
        Reserved = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }
}
