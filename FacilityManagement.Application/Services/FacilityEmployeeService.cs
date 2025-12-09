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
    public class FacilityEmployeeService : BaseDatabaseService<AstrikFacilityContext>, IFacilityEmployeeService
    {
        private readonly IMapper _mapper;
        private readonly IFacilityManagementUnitOfWork _facilityManagementUnitOfWork;

        public FacilityEmployeeService(IMapper mapper ,AstrikFacilityContext context, IFacilityManagementUnitOfWork facilityManagementUnitOfWork, IConfiguration configuration)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _facilityManagementUnitOfWork = facilityManagementUnitOfWork;

        }

        public async Task<BaseResponse<EmployeeResponseDTO?>> GetEmployeeByIdAsync(int employeeId)
        {
            return await HandleActionAsync(async () =>
            {
                var ticketEmployeeRole = await _context.Employees
                                             .Include(x=>x.FacilityRole)
                                            .Where(e => e.EmployeeId == employeeId && e.IsActive == true)
                                            .FirstOrDefaultAsync();


                var employee = await _context.Employees
                            .Where(x => x.EmployeeId == employeeId)
                            .Select(e => new EmployeeResponseDTO
                            {
                                EmployeeId = e.EmployeeId,
                                FullName = e.FullName,
                                EmployeePhoto = e.EmployeePhoto,
                                FacilityRoleId=(int)e.FacilityRoleId,
                                FacilityRoleName=e.FacilityRole.FacilityRoleName
                            })
                            .FirstOrDefaultAsync();
                if (employee == null)
                {
                    InitMessageResponse("NotFound", "No employee found.");
                    return null;
                }
                return employee;
            });
        }

        public async Task<BaseResponse<List<EmployeeQuickSearchResponseDTO>>> GetEmployeeQuickSearch(string keyword)
        {
            return await HandleActionAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    InitMessageResponse("NotFound", "keyword not empty");
                }

                var query = _context.Employees
                       .Include(x => x.FacilityRole)
                       .Where(e =>
                                e.IsActive == true 
                                && e.FacilityRoleId==(int)Enums.FacilityRole.Employee
                                &&
                                (
                                    e.FullName.ToLower().Contains(keyword.ToLower()) 
                                )
                            );

                var employees = await query
                       .Select(e => new EmployeeQuickSearchResponseDTO
                       {
                           EmployeeId = e.EmployeeId,
                           FullName = e.FullName,
                           EmployeePhoto = e.EmployeePhoto != null ? e.EmployeePhoto : null,
                           FacilityRoleId = (int)e.FacilityRoleId,
                           FacilityRoleName = e.FacilityRole != null ? e.FacilityRole.FacilityRoleName : null
                       })
                       .Take(10)
                       .ToListAsync();

                return employees;
            });
        }
    }
}
