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
        private readonly AstrikFacilityContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FacilityManagementUnitOfWork(AstrikFacilityContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(int userId, CancellationToken cancellationToken = default)
        {
            SetDefaultValues(userId);
            return await _context.SaveChangesAsync(cancellationToken);
        }

        private void SetDefaultValues(int userId)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    foreach (var property in entry.Properties)
                    {
                        switch (property.Metadata.Name)
                        {
                            case "IsActive":
                            case "IsEnabled":
                                property.CurrentValue = true;
                                break;
                            case "CreatedOn":
                                property.CurrentValue = DateTime.UtcNow;
                                break;
                            case "CreatedBy":
                                property.CurrentValue = userId;
                                break;
                            case "IPAddress":
                                property.CurrentValue = ipAddress;
                                break;
                        }
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    foreach (var property in entry.Properties)
                    {
                        switch (property.Metadata.Name)
                        {
                            case "UpdatedBy":
                                if (property.IsModified)
                                {
                                    property.CurrentValue = userId;
                                }
                                break;
                            case "UpdatedOn":
                                if (property.IsModified)
                                {
                                    property.CurrentValue = DateTime.UtcNow;
                                }
                                break;
                            case "IPAddress":
                                if (property.IsModified)
                                    property.CurrentValue = ipAddress;
                                break;
                        }
                    }
                }
            }
        }
    }
}
