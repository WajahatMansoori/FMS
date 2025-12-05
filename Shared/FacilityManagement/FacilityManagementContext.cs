using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Shared.FacilityManagement
{
    public class FacilityManagementContext : DbContext
    {
        public FacilityManagementContext(DbContextOptions<FacilityManagementContext> options)
            : base(options)
        {
        }

        // Add your DbSets here as you create entities
        // Example:
        // public DbSet<YourEntity> YourEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add your entity configurations here
        }
    }
}
