using System;
using System.Collections.Generic;

namespace Shared.FacilityManagement;

public partial class FacilityRole
{
    public int FacilityRoleId { get; set; }

    public string? FacilityRoleName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
