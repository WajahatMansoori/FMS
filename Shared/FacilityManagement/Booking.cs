using System;
using System.Collections.Generic;

namespace Shared.FacilityManagement;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? SlotId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime? BookingDate { get; set; }

    public int? FacilitySlotStatusId { get; set; }

    public DateTime? CancelledDate { get; set; }

    public int? CancelledBy { get; set; }

    public string? Remarks { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }
}
