using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.FacilityManagement;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? FullName { get; set; }

    public int? FacilityRoleId { get; set; }

    public string? EmployeePhoto { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey(nameof(FacilityRoleId))]
    public FacilityRole FacilityRole { get; set; } = null!;
}
