using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.FacilityManagement;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? EmployeeCode { get; set; }

    public string? FullName { get; set; }

    public int? FacilityRoleId { get; set; }

    public int? DeptId { get; set; }

    public string? DeptName { get; set; }

    public string? OfficialEmailAddress { get; set; }

    public string? PhoneNo { get; set; }

    public string? MobileNo { get; set; }

    public string? WhatsappNo { get; set; }

    public int? EmployeeTypeId { get; set; }

    public int? DesignationId { get; set; }

    public string? DesignationName { get; set; }

    public int? LocationId { get; set; }

    public string? LocationName { get; set; }

    public string? EmpPhoto { get; set; }

    [ForeignKey(nameof(FacilityRoleId))]
    public FacilityRole FacilityRole { get; set; } = null!;
}
