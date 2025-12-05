using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FacilityManagement.Application.DTOs.Response;
using FacilityManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Base;
using Shared.Base.Responses;
using Shared.FacilityManagement;
using Shared.UnitOfWork;

namespace FacilityManagement.Application.Services
{
    public class FacilityService : BaseDatabaseService<AstrikFacilityContext>, IFacilityService
    {
        private readonly IMapper _mapper;
        private readonly IFacilityManagementUnitOfWork _facilityManagementUnitOfWork;

        public FacilityService(IMapper mapper ,AstrikFacilityContext context, IFacilityManagementUnitOfWork facilityManagementUnitOfWork, IConfiguration configuration)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _facilityManagementUnitOfWork = facilityManagementUnitOfWork;

        }

        public async Task<BaseResponse<List<FacilitesResponseDTO>?>> GetAllFacilitesAsync()
        {
            return await HandleActionAsync(async () =>
            {
                var facilities = await _context.Facilities
                .Where(f => f.IsActive == true)
                .Select(f => new FacilitesResponseDTO
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName ?? string.Empty,
                })
                .ToListAsync();

                if (facilities == null)
                {
                    InitMessageResponse("NotFound", "Facilities not found");
                    return null;
                }

                return facilities;
            });
        }

        public async Task<BaseResponse<List<FacilityResourcesResponseDTO>?>> GetFacilityResources(int facilityId)
        {
            return await HandleActionAsync(async () =>
            {
                var facilityName = await _context.Facilities
                    .Where(f => f.FacilityId == facilityId)
                    .Select(f => f.FacilityName)
                    .FirstOrDefaultAsync();

                if (facilityName == null)
                {
                    InitMessageResponse("NotFound", "Invalid Facility ID");
                    return null;
                }

                var resources = await _context.FacilityResources
                    .Where(fr => fr.FacilityId == facilityId && fr.IsActive == true)
                    .Select(fr => new FacilityResourcesResponseDTO
                    {
                        FacilityResourceId = fr.FacilityResourceId,
                        FacilityResourceName = fr.FacilityResourceName ?? string.Empty,
                        FacilityName = facilityName
                    })
                    .ToListAsync();

                if (!resources.Any())
                {
                    InitMessageResponse("NotFound", "Facility resources not found");
                    return null;
                }

                return resources;
            });
        }
    }
}
