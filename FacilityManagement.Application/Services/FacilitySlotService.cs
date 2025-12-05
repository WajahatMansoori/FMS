using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FacilityManagement.Application.Interfaces;
using Shared.Base;
using Shared.FacilityManagement;
using Shared.UnitOfWork;

namespace FacilityManagement.Application.Services
{
    public class FacilitySlotService : BaseDatabaseService<AstrikFacilityContext>, IFacilitySlotService
    {
        private readonly IMapper _mapper;
        private readonly AstrikFacilityContext _astrikFacilityContext;
        private readonly IFacilityManagementUnitOfWork _facilityManagementUnitOfWork;

        public FacilitySlotService(IMapper mapper, AstrikFacilityContext context, IFacilityManagementUnitOfWork facilityManagementUnitOfWork,
            AstrikFacilityContext astrikFacilityContext)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _facilityManagementUnitOfWork = facilityManagementUnitOfWork;
            _astrikFacilityContext = astrikFacilityContext;
        }
    }
}
