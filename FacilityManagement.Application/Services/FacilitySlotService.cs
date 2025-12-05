using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FacilityManagement.Application.DTOs.Request;
using FacilityManagement.Application.Enums;
using FacilityManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Base;
using Shared.Base.Responses;
using Shared.FacilityManagement;
using Shared.UnitOfWork;

namespace FacilityManagement.Application.Services
{
    public class FacilitySlotService : BaseDatabaseService<AstrikFacilityContext>, IFacilitySlotService
    {
        private readonly IMapper _mapper;
        private readonly IFacilityManagementUnitOfWork _facilityManagementUnitOfWork;

        public FacilitySlotService(IMapper mapper, AstrikFacilityContext context, IFacilityManagementUnitOfWork facilityManagementUnitOfWork, IConfiguration configuration)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _facilityManagementUnitOfWork = facilityManagementUnitOfWork;
        }

        public async Task<BaseResponse<Task>> AddFacilityBookingAsync(BookingRequestDTO bookingRequestDTO)
        {
            return await HandleVoidActionAsync(async () =>
            {
                // Validate input
                if (bookingRequestDTO == null)
                {
                    InitMessageResponse("BadRequest", "Booking information not found.");
                    return;
                }

                // Get the slot with related data
                var slot = await _context.Slots
                    .Include(s => s.FacilityResource)
                    .ThenInclude(fr => fr.Facility)
                    .ThenInclude(f => f.Location)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SlotId == bookingRequestDTO.SlotId && s.IsActive == true);

                if (slot == null)
                {
                    InitMessageResponse("NotFound", "Slot not found.");
                    return;
                }

                // Check if slot is already booked
                if (slot.IsBooked == true)
                {
                    InitMessageResponse("BadRequest", "This slot is already booked.");
                    return;
                }

                // Validate that booking is at least 15 minutes before slot start time
                var slotStartDateTime = slot.SlotDate.Value.ToDateTime(slot.StartTime.Value);
                var currentDateTime = DateTime.UtcNow;
                var timeDifference = slotStartDateTime - currentDateTime;

                if (timeDifference.TotalMinutes < 15 && timeDifference.TotalMinutes > 0)
                {
                    InitMessageResponse("BadRequest", "You can only book a slot at least 15 minutes before its start time.");
                    return;
                }

                // Check employee's bookings for current day
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var restrictedStatuses = new List<int>
                {
                    (int)Enums.FacilitySlotStatus.Reserved,
                    (int)Enums.FacilitySlotStatus.InProgress,
                    (int)Enums.FacilitySlotStatus.Completed
                };

                var todayBookingsCount = await _context.Bookings
                    .Join(_context.Slots,
                        booking => booking.SlotId,
                        slot => slot.SlotId,
                        (booking, slot) => new { Booking = booking, Slot = slot })
                    .Where(x => x.Booking.EmployeeId == bookingRequestDTO.EmployeeId
                        && x.Slot.SlotDate == today
                        && x.Booking.IsActive == true
                        && x.Booking.FacilitySlotStatusId.HasValue
                        && restrictedStatuses.Contains(x.Booking.FacilitySlotStatusId.Value))
                    .CountAsync();

                if (todayBookingsCount >= 2 && slot.SlotDate == today)
                {
                    InitMessageResponse("BadRequest", "You have already booked 2 slots for today. You can book slots for future days.");
                    return;
                }

                // Create the booking
                var booking = new Booking
                {
                    SlotId = bookingRequestDTO.SlotId,
                    EmployeeId = bookingRequestDTO.EmployeeId,
                    BookingDate = DateTime.UtcNow,
                    FacilitySlotStatusId = (int)Enums.FacilitySlotStatus.Reserved,
                    Remarks = bookingRequestDTO.Remarks,
                    CreatedBy = bookingRequestDTO.EmployeeId,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.Bookings.AddAsync(booking);

                // Update slot to mark as booked
                slot.IsBooked = true;
                slot.UpdatedBy = bookingRequestDTO.EmployeeId;
                slot.UpdatedOn = DateTime.UtcNow;
                _context.Slots.Update(slot);

                await _facilityManagementUnitOfWork.SaveChangesAsync(bookingRequestDTO.EmployeeId);
            });
        }
    }
}
