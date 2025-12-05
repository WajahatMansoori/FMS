using System;
using System.Collections.Generic;

namespace Shared.FacilityManagement;

public partial class Facility
{
    public int FacilityId { get; set; }

    public string? FacilityName { get; set; }

    public string? Description { get; set; }

    public int? LocationId { get; set; }

    public int? SlotDurationMinutes { get; set; }

    public int? MaxSlotsPerEmployeePerDay { get; set; }

    public int? CancellationWindowMinutes { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }
}
