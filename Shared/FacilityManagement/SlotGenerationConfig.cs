using System;
using System.Collections.Generic;

namespace Shared.FacilityManagement;

public partial class SlotGenerationConfig
{
    public int SlotGenerationConfigId { get; set; }

    public int? FacilityId { get; set; }

    public bool? IsWithWeekend { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }
}
