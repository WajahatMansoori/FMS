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

                // Create SlotGenerationConfig entry
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
                                FacilitySlotStatusId = 1, // 1 = Available
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
