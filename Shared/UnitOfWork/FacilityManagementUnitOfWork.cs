using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.FacilityManagement;
using Shared.Helpers;

namespace Shared.UnitOfWork
{
    public class FacilityManagementUnitOfWork : IFacilityManagementUnitOfWork
    {
        private readonly FacilityManagementContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FacilityManagementUnitOfWork(FacilityManagementContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetDefaultValues();
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(int userId, CancellationToken cancellationToken = default)
        {
            SetDefaultValues(userId);
            return await _context.SaveChangesAsync(cancellationToken);
        }

        private void SetDefaultValues(int? userId = null)
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var ipAddress = IpAddressHelper.GetClientIp(_httpContextAccessor.HttpContext);

                if (entry.State == EntityState.Added)
                {
                    if (entry.Property("CreatedBy") != null)
                    {
                        if (userId.HasValue)
                        {
                            entry.Property("CreatedBy").CurrentValue = userId.Value;
                        }
                        else
                        {
                            entry.Property("CreatedBy").CurrentValue = 0;
                        }
                    }

                    if (entry.Property("CreatedOn") != null)
                    {
                        entry.Property("CreatedOn").CurrentValue = DateTime.Now;
                    }

                    if (entry.Property("IsActive") != null)
                    {
                        entry.Property("IsActive").CurrentValue = true;
                    }

                    if (entry.Property("IPAddress") != null)
                    {
                        entry.Property("IPAddress").CurrentValue = ipAddress;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    if (entry.Property("UpdatedBy") != null)
                    {
                        if (userId.HasValue)
                        {
                            entry.Property("UpdatedBy").CurrentValue = userId.Value;
                        }
                        else
                        {
                            entry.Property("UpdatedBy").CurrentValue = 0;
                        }
                    }

                    if (entry.Property("UpdatedOn") != null)
                    {
                        entry.Property("UpdatedOn").CurrentValue = DateTime.Now;
                    }

                    if (entry.Property("IPAddress") != null)
                    {
                        entry.Property("IPAddress").CurrentValue = ipAddress;
                    }
                }
            }
        }
    }
}
