using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.FacilityManagement;

public partial class Slot
{
    public int SlotId { get; set; }

    public int? FacilityResourceId { get; set; }

    public DateOnly? SlotDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public int? SlotGenerationConfigId { get; set; }

    public bool? IsBooked { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }
    
    [ForeignKey(nameof(SlotGenerationConfigId))]
    public virtual SlotGenerationConfig SlotGenerationConfig { get; set; } = null!;

    [ForeignKey(nameof(FacilityResourceId))]
    public virtual FacilityResource FacilityResource { get; set; } = null!;
}
