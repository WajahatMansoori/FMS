using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FacilityManagement.Application.DTOs.Request;
using FacilityManagement.Application.DTOs.Response;
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

        public async Task<BaseResponse<Task>> AddFacilitySlotAsync(AddFacilitySlotRequestDTO request)
        {
            return await HandleVoidActionAsync(async () =>
            {
                // Validation
                if (request == null)
                {
                    InitMessageResponse("BadRequest", "Request cannot be null.");
                    return;
                }

                if (request.SlotStartDate > request.SlotEndDate)
                {
                    InitMessageResponse("BadRequest", "Slot Start Date cannot be greater than Slot End Date.");
                    return;
                }

                if (request.StartTime >= request.EndTime)
                {
                    InitMessageResponse("BadRequest", "Start Time must be less than End Time.");
                    return;
                }

                // Get FacilityResource and Facility details
                var facilityResource = await _context.FacilityResources
                    .FirstOrDefaultAsync(fr => fr.FacilityResourceId == request.FacilityResourceId && fr.IsActive == true);

                if (facilityResource == null)
                {
                    InitMessageResponse("NotFound", "Facility Resource not found.");
                    return;
                }

                var facility = await _context.Facilities
                    .FirstOrDefaultAsync(f => f.FacilityId == facilityResource.FacilityId && f.IsActive == true);

                if (facility == null)
                {
                    InitMessageResponse("NotFound", "Facility not found.");
                    return;
                }

                if (!facility.SlotDurationMinutes.HasValue || facility.SlotDurationMinutes.Value <= 0)
                {
                    InitMessageResponse("BadRequest", "Invalid Slot Duration in Facility configuration.");
                    return;
                }

                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var slotGenerationConfig = new SlotGenerationConfig
                        {
                            FacilityId = facility.FacilityId,
                            IsWithWeekend = request.IsWithWeekend,
                            CreatedOn = DateTime.Now,
                            IsActive = true
                        };

                        await _context.SlotGenerationConfigs.AddAsync(slotGenerationConfig);
                        await _facilityManagementUnitOfWork.SaveChangesAsync();

                        // Generate slots
                        int totalSlotsCreated = 0;
                        int skippedSlots = 0;
                        var slotDuration = facility.SlotDurationMinutes.Value;

                        // Get existing slots for this resource in the date range
                        var existingSlots = await _context.Slots
                            .Where(s => s.FacilityResourceId == request.FacilityResourceId
                                        && s.IsActive == true
                                        && s.SlotDate >= request.SlotStartDate
                                        && s.SlotDate <= request.SlotEndDate)
                            .Select(s => new { s.SlotDate, s.StartTime, s.EndTime })
                            .ToListAsync();

                        // Loop through each date in the range
                        for (var currentDate = request.SlotStartDate; currentDate <= request.SlotEndDate; currentDate = currentDate.AddDays(1))
                        {
                            // Check if weekend should be excluded
                            if (!request.IsWithWeekend)
                            {
                                var dayOfWeek = currentDate.DayOfWeek;
                                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                                {
                                    continue; // Skip weekends
                                }
                            }

                            // Generate slots for this date
                            var currentTime = request.StartTime;

                            while (currentTime < request.EndTime)
                            {
                                var slotEndTime = currentTime.AddMinutes(slotDuration);

                                // Ensure we don't exceed the end time
                                if (slotEndTime > request.EndTime)
                                {
                                    break;
                                }

                                // Check if this slot already exists
                                var slotExists = existingSlots.Any(es =>
                                    es.SlotDate == currentDate
                                    && es.StartTime == currentTime
                                    && es.EndTime == slotEndTime);

                                if (slotExists)
                                {
                                    skippedSlots++;
                                }
                                else
                                {
                                    // Create new slot
                                    var slot = new Slot
                                    {
                                        FacilityResourceId = request.FacilityResourceId,
                                        SlotDate = currentDate,
                                        StartTime = currentTime,
                                        EndTime = slotEndTime,
                                        SlotGenerationConfigId = slotGenerationConfig.SlotGenerationConfigId,
                                        FacilitySlotStatusId = (int)Enums.FacilitySlotStatus.Available, // 1 = Available
                                        CreatedOn = DateTime.Now,
                                        IsActive = true
                                    };

                                    await _context.Slots.AddAsync(slot);
                                    totalSlotsCreated++;
                                }

                                currentTime = slotEndTime;
                            }
                        }

                        // If no unique slots to create, return error
                        if (totalSlotsCreated == 0)
                        {
                            InitMessageResponse("Conflict", "All slots in the specified date range already exist. No new slots were created.");
                            return;
                        }

                        // Save all slots
                        await _facilityManagementUnitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        InitMessageResponse("ServerError", ex.Message);
                        return;
                    }
                });

                // Create SlotGenerationConfig entry
              
            });
        }

        public async Task<PagedBaseResponse<List<FacilitySlotResponseDTO>>> GetAllAsync(FacilitySlotFilterRequestDTO filterRequest)
        {
            return await HandlePaginatedActionAsync<FacilitySlotResponseDTO, FacilitySlotResponseDTO>(async () =>
            {
                var startDate = filterRequest.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
                var endDate = filterRequest.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

                if (startDate > endDate)
                {
                    InitMessageResponse("BadRequest", "Start Date cannot be greater than End Date.");
                    return null!;
                }

                if (!filterRequest.EmployeeId.HasValue)
                {
                    InitMessageResponse("BadRequest", "EmployeeId is required.");
                    return null!;
                }

                // Start building query with navigation properties
                var query = _context.Slots
                    .Include(s => s.FacilityResource)
                        .ThenInclude(fr => fr.Facility)
                    .Where(s => s.IsActive == true
                        && s.SlotDate >= startDate
                        && s.SlotDate <= endDate);

                var facilityRoleId = _context.Employees
                    .Where(e => e.EmployeeId == filterRequest.EmployeeId && e.IsActive == true)
                    .Select(e => e.FacilityRoleId)
                    .FirstOrDefault();

                // Apply role-based filtering
                if (facilityRoleId == (int)Enums.FacilityRole.SuperAdmin)
                {
                    // Super Admin can view all slots (Available, Reserved, InProgress, Completed, Cancelled)
                    // No additional filtering needed - shows all slots
                }
                else if (facilityRoleId == (int)Enums.FacilityRole.Employee)
                {
                    // Employee can only view their own booked slots (Reserved, InProgress, Completed, Cancelled)
                    // Exclude Available slots for employees
                    query = query.Where(s => s.FacilitySlotStatusId != (int)Enums.FacilitySlotStatus.Available);
                }

                // For employee role, filter by their bookings
                if (facilityRoleId == (int)Enums.FacilityRole.Employee)
                {
                    var employeeSlotIds = await _context.Bookings
                        .Where(b => b.EmployeeId == filterRequest.EmployeeId && b.IsActive == true)
                        .Select(b => b.SlotId)
                        .ToListAsync();

                    query = query.Where(s => employeeSlotIds.Contains(s.SlotId));
                }

                // Get slot IDs for current page
                var slotIds = await query
                    .OrderBy(s => s.SlotDate)
                    .ThenBy(s => s.StartTime)
                    .Select(s => s.SlotId)
                    .ToListAsync();

                // Get total count
                var totalItems = slotIds.Count;

                // Apply pagination to slot IDs
                var pagedSlotIds = slotIds
                    .Skip((filterRequest.PageNumber - 1) * filterRequest.PageSize)
                    .Take(filterRequest.PageSize)
                    .ToList();

                // Load bookings and employees for the paginated slots
                var bookings = await _context.Bookings
                    .Include(b => b.Slot)
                    .Where(b => pagedSlotIds.Contains(b.SlotId.Value) && b.IsActive == true)
                    .ToListAsync();

                var employeeIds = bookings.Select(b => b.EmployeeId).Distinct().ToList();
                var employees = await _context.Employees
                    .Where(e => employeeIds.Contains(e.EmployeeId))
                    .ToListAsync();

                // Load facility slot statuses
                var statuses = await _context.FacilitySlotStatuses.ToListAsync();

                // Get paginated slots with all navigation properties
                var slots = await _context.Slots
                    .Include(s => s.FacilityResource)
                        .ThenInclude(fr => fr.Facility)
                    .Where(s => pagedSlotIds.Contains(s.SlotId))
                    .OrderBy(s => s.SlotDate)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

                // Map to response DTO
                var response = slots.Select(slot =>
                {
                    var booking = bookings.FirstOrDefault(b => b.SlotId == slot.SlotId);
                    var employee = booking != null ? employees.FirstOrDefault(e => e.EmployeeId == booking.EmployeeId) : null;
                    var status = statuses.FirstOrDefault(s => s.FacilitySlotStatusId == slot.FacilitySlotStatusId);

                    return new FacilitySlotResponseDTO
                    {
                        SlotId = slot.SlotId,
                        EmployeeId = employee?.EmployeeId,
                        EmployeeName = employee?.FullName,
                        EmployeePhoto = employee?.EmployeePhoto,
                        FacilityName = slot.FacilityResource?.Facility?.FacilityName,
                        FacilityResourceName = slot.FacilityResource?.FacilityResourceName,
                        SlotDate = slot.SlotDate,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        FacilitySlotStatusName = status?.FacilitySlotStatusName
                    };
                }).ToList();

                return new PaginatedResult<FacilitySlotResponseDTO>
                {
                    Items = response,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize,
                    TotalItems = totalItems
                };
            });
        }

        public async Task<BaseResponse<List<AvailableSlotsResponseDTO>>> GetAllAvailableSlotsAsync(int facilityResourceId, DateOnly date)
        {
            return await HandleActionAsync(async () =>
            {
                var availableSlots = await _context.Slots
                        .Where(s => s.FacilityResourceId == facilityResourceId
                                    && s.SlotDate == date
                                    && s.FacilitySlotStatusId == (int)Enums.FacilitySlotStatus.Available
                                    && s.IsActive == true)
                        .Select(s => new AvailableSlotsResponseDTO
                        {
                            SlotId = s.SlotId,
                            SlotStartTime = s.StartTime.HasValue ? s.StartTime.Value : default,
                            SlotEndTime = s.EndTime.HasValue ? s.EndTime.Value : default

                        })
                        .ToListAsync();

                if (availableSlots == null || availableSlots.Count == 0)
                {
                    InitMessageResponse("NotFound", "No available slots found for the specified resource and date.");
                    return null!;
                }

                return availableSlots;
            });
        }
    }
}
