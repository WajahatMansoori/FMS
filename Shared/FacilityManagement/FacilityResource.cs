using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.FacilityManagement;

public partial class FacilityResource
{
    public int FacilityResourceId { get; set; }

    public string? FacilityResourceName { get; set; }

    public int? FacilityId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey(nameof(FacilityId))]
    public Facility Facility { get; set; } = null!;
}
