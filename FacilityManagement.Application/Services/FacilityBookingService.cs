using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FacilityManagement.Application.DTOs.Request;
using FacilityManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Base;
using Shared.Base.Responses;
using Shared.FacilityManagement;
using Shared.Helpers;
using Shared.UnitOfWork;

namespace FacilityManagement.Application.Services
{
    internal class FacilityBookingService : BaseDatabaseService<AstrikFacilityContext>, IFacilityBookingService
    {
        private readonly IMapper _mapper;
        private readonly IFacilityManagementUnitOfWork _facilityManagementUnitOfWork;
        private readonly ConvertUtcToPakistanTimeHelper _convertUtcToPakistanTimeHelper;

        public FacilityBookingService(IMapper mapper, AstrikFacilityContext context, IFacilityManagementUnitOfWork facilityManagementUnitOfWork,
            ConvertUtcToPakistanTimeHelper convertUtcToPakistanTimeHelper, IConfiguration configuration)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _facilityManagementUnitOfWork = facilityManagementUnitOfWork;
            _convertUtcToPakistanTimeHelper = convertUtcToPakistanTimeHelper;
        }

        public async Task<BaseResponse<Task>> AddAsync(BookingRequestDTO bookingRequestDTO)
        {
            return await HandleVoidActionAsync(async () =>
            {
                if (bookingRequestDTO == null)
                {
                    InitMessageResponse("BadRequest", "Booking information not found.");
                    return;
                }

                // Get current Pakistan time (UTC conversion)
                DateTime currentPakistanTime = _convertUtcToPakistanTimeHelper.ConvertUtcToPakistanTime(DateTime.UtcNow);

                var slot = await _context.Slots
                    .Include(s => s.FacilityResource)
                    .ThenInclude(fr => fr.Facility)
                    .FirstOrDefaultAsync(s => s.SlotId == bookingRequestDTO.SlotId && s.IsActive==true);

                if (slot == null)
                {
                    InitMessageResponse("NotFound", "Slot not found.");
                    return;
                }

                if (!slot.SlotDate.HasValue || !slot.StartTime.HasValue || !slot.EndTime.HasValue)
                {
                    InitMessageResponse("BadRequest", "Slot date or time is missing.");
                    return;
                }

                var targetDate = slot.SlotDate.Value;
                var slotStartDateTime = slot.SlotDate.Value.ToDateTime(slot.StartTime.Value);

                // Prevent booking if slot is in the past
                if (slotStartDateTime <= currentPakistanTime)
                {
                    InitMessageResponse("BadRequest", "Cannot book a slot in the past.");
                    return;
                }

                // Check booking window
                int bookingWindowMinutes = slot.FacilityResource.Facility.BookingWindowMinutes ?? 15;
                var timeDifference = slotStartDateTime - currentPakistanTime;
                if (timeDifference.TotalMinutes < bookingWindowMinutes)
                {
                    InitMessageResponse("BadRequest", $"You can only book a slot at least {bookingWindowMinutes} minutes before its start time.");
                    return;
                }

                // Check max slots per employee per day
                var restrictedStatuses = new List<int>
                {
                    (int)Enums.FacilitySlotStatus.Reserved,
                    (int)Enums.FacilitySlotStatus.InProgress,
                    (int)Enums.FacilitySlotStatus.Completed
                };

                var todayBookingsCount = await _context.Bookings
                    .Include(b => b.Slot)
                    .Where(b => b.EmployeeId == bookingRequestDTO.EmployeeId
                                && b.Slot.SlotDate == targetDate
                                && b.IsActive == true
                                && b.CancelledDate==null
                                && b.Slot.FacilitySlotStatusId.HasValue
                                && restrictedStatuses.Contains(b.Slot.FacilitySlotStatusId.Value))
                    .CountAsync();

                var maxPerDay = slot.FacilityResource.Facility.MaxSlotsPerEmployeePerDay;
                if (maxPerDay.HasValue && todayBookingsCount >= maxPerDay.Value)
                {
                    InitMessageResponse("BadRequest",
                        $"You have already booked {todayBookingsCount} slot(s) for {targetDate:yyyy-MM-dd}. You cannot book more than {maxPerDay.Value} slot(s) on the same day.");
                    return;
                }

                // EF Core Execution Strategy + Transaction
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Set the slot status to Reserved using RowVersion to prevent concurrency issues
                        slot.FacilitySlotStatusId = (int)Enums.FacilitySlotStatus.Reserved;
                        slot.UpdatedBy = bookingRequestDTO.EmployeeId;
                        slot.UpdatedOn = currentPakistanTime;

                        _context.Slots.Update(slot);

                        // Save changes to slot first
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            InitMessageResponse("ServiceUnavailable", "This slot was just booked by another user. Please select another slot.");
                            return;
                        }

                        // Add booking
                        var booking = new Booking
                        {
                            SlotId = bookingRequestDTO.SlotId,
                            EmployeeId = bookingRequestDTO.EmployeeId,
                            BookingDate = currentPakistanTime,
                            CreatedBy = bookingRequestDTO.EmployeeId,
                        };

                        await _context.Bookings.AddAsync(booking);
                        await _facilityManagementUnitOfWork.SaveChangesAsync(bookingRequestDTO.EmployeeId);

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        InitMessageResponse("ServerError", ex.Message);
                        return;
                    }
                });
            });
        }

        public async Task<BaseResponse<Task>> CancelSlotAsync(CancelSlotRequestDTO cancelSlotRequestDTO)
        {
            return await HandleVoidActionAsync(async () =>
            {
                string? UserName = string.Empty;
                if (cancelSlotRequestDTO == null)
                {
                    InitMessageResponse("BadRequest", "Cancel request information not found.");
                    return;
                }

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeId == cancelSlotRequestDTO.EmployeeId);
                
                if (employee == null)
                {
                    InitMessageResponse("NotFound", "Employee not found.");
                    return;
                }
                UserName = employee.FullName;

                if (!employee.FacilityRoleId.HasValue)
                {
                    InitMessageResponse("BadRequest", "Employee role not assigned.");
                    return;
                }

                var slot = await _context.Slots
                    .Include(s => s.FacilityResource)
                    .ThenInclude(fr => fr.Facility)
                    .FirstOrDefaultAsync(s => s.SlotId == cancelSlotRequestDTO.SlotId && s.IsActive == true);

                if (slot == null)
                {
                    InitMessageResponse("NotFound", "Slot not found.");
                    return;
                }

                // Get Pakistan date (no DateTime.Now)
                var currentPakistanTime = _convertUtcToPakistanTimeHelper.ConvertUtcToPakistanTime(DateTime.UtcNow);
                var currentPakistanDate = DateOnly.FromDateTime(currentPakistanTime);

                if (slot.SlotDate.HasValue && slot.SlotDate.Value < currentPakistanDate)
                {
                    InitMessageResponse("BadRequest", "Cannot cancel a slot from a previous date.");
                    return;
                }

                if (slot.FacilitySlotStatusId == (int)Enums.FacilitySlotStatus.Cancelled)
                {
                    InitMessageResponse("BadRequest", "This slot is already cancelled.");
                    return;
                }

                if (slot.FacilitySlotStatusId == (int)Enums.FacilitySlotStatus.Available)
                {
                    InitMessageResponse("BadRequest", "This slot is not booked yet.");
                    return;
                }

                if (slot.FacilitySlotStatusId == (int)Enums.FacilitySlotStatus.InProgress)
                {
                    InitMessageResponse("BadRequest", "Cannot cancel a slot that is in progress.");
                    return;
                }

                if (slot.FacilitySlotStatusId == (int)Enums.FacilitySlotStatus.Completed)
                {
                    InitMessageResponse("BadRequest", "Cannot cancel a slot that has been completed.");
                    return;
                }

                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.SlotId == cancelSlotRequestDTO.SlotId && b.IsActive == true);

                if (booking == null)
                {
                    InitMessageResponse("NotFound", "Booking not found for this slot.");
                    return;
                }

                int cancellationWindow = slot.FacilityResource.Facility.CancellationWindowMinutes.Value;

                // Regular employee cancellation logic
                if (employee.FacilityRoleId.Value == (int)Enums.FacilityRole.Employee)
                {
                    if (booking.EmployeeId != cancelSlotRequestDTO.EmployeeId)
                    {
                        InitMessageResponse("Forbidden", "You can only cancel your own bookings.");
                        return;
                    }

                    var slotStartDateTime = slot.SlotDate.Value.ToDateTime(slot.StartTime.Value);
                    var currentDateTime = currentPakistanTime;

                    var timeDifference = slotStartDateTime - currentDateTime;

                    if (timeDifference.TotalMinutes < cancellationWindow)
                    {
                        InitMessageResponse("BadRequest", $"You can only cancel a slot at least {cancellationWindow} minutes before its start time.");
                        return;
                    }
                }
                // SuperAdmin cancellation logic
                if (employee.FacilityRoleId.Value == (int)Enums.FacilityRole.SuperAdmin)
                {
                    var slotStartDateTime = slot.SlotDate.Value.ToDateTime(slot.StartTime.Value);
                    var currentDateTime = currentPakistanTime;

                    if (currentDateTime >= slotStartDateTime)
                    {
                        InitMessageResponse("BadRequest", "Cannot cancel a slot that has already started.");
                        return;
                    }
                }

                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var pakTimeNow = _convertUtcToPakistanTimeHelper.ConvertUtcToPakistanTime(DateTime.UtcNow);

                        slot.FacilitySlotStatusId = (int)Enums.FacilitySlotStatus.Cancelled;
                        slot.UpdatedBy = cancelSlotRequestDTO.EmployeeId;
                        slot.UpdatedOn = pakTimeNow;

                        _context.Slots.Update(slot);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            InitMessageResponse("ServiceUnavailable", $"This slot was just Cancelled by {UserName}.");
                            return;
                        }

                        booking.CancelledDate = pakTimeNow;
                        booking.CancelledBy = cancelSlotRequestDTO.EmployeeId;
                        booking.UpdatedBy = cancelSlotRequestDTO.EmployeeId;
                        booking.UpdatedOn = pakTimeNow;

                        booking.Remarks = employee.FacilityRoleId.Value == (int)Enums.FacilityRole.SuperAdmin
                                          ? cancelSlotRequestDTO.Remarks
                                          : "Slot Cancelled";

                        _context.Bookings.Update(booking);
                        await _facilityManagementUnitOfWork.SaveChangesAsync(cancelSlotRequestDTO.EmployeeId);
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        InitMessageResponse("ServerError", ex.Message);
                        return;
                    }
                });
            });
        }

    }
}
