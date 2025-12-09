using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacilityManagement.Application.Interfaces;
using FacilityManagement.Application.Mapping;
using FacilityManagement.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.FacilityManagement;
using Shared.Helpers;
using Shared.UnitOfWork;

namespace FacilityManagement.Application
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddFacilityManagementService(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AstrikFacilityContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                }));


            // Uncomment when you have AutoMapper profiles
            //services.AddAutoMapper(typeof(Profiles));
            
            services.AddScoped<IFacilityManagementUnitOfWork, FacilityManagementUnitOfWork>();

            // Add your services here as you create them
            // Example:
            // services.AddScoped<IYourService, YourService>();

            services.AddScoped<FileUploadHelper>();
            services.AddScoped<IFacilitySlotService, FacilitySlotService>();
            services.AddScoped<IFacilityService, FacilityService>();
            services.AddScoped<IFacilityManagementUnitOfWork, FacilityManagementUnitOfWork>();
            services.AddScoped<IFacilityBookingService, FacilityBookingService>();
            services.AddScoped<IFacilityEmployeeService, FacilityEmployeeService>();
            services.AddAutoMapper(typeof(Profiles));

            return services;
        }
    }
}
