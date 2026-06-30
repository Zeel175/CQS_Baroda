using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class CustomerDocumentMappingRepository : BaseRepository<CustomerDocumentMappingEntity>, ICustomerDocumentMappingRepository
    {
        private readonly IStoredProcedureContext _storedProcedureContext;
        public CustomerDocumentMappingRepository(IDataContext dataContext
            , IStoredProcedureContext storedProcedureContext)
            : base(dataContext)
        {
            _storedProcedureContext = storedProcedureContext;
        }

        public IQueryable<CustomerDocumentMappingEntity> GetAllActive()
        {
            return GetAllNoTracking().Where(w => w.IsActive);
        }

    }
}
