using System;
using System.Collections.Generic;

namespace Shared.FacilityManagement;

public partial class Location
{
    public int LocationId { get; set; }

    public string? LocationName { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }
}
