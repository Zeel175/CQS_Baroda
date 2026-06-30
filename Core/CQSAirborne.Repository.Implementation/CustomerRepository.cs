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
    public class CustomerRepository : BaseRepository<CustomerEntity>, ICustomerRepository
    {
        private readonly IStoredProcedureContext _storedProcedureContext;
        public CustomerRepository(IDataContext dataContext
            , IStoredProcedureContext storedProcedureContext)
            : base(dataContext)
        {
            _storedProcedureContext = storedProcedureContext;
        }

        public IQueryable<CustomerEntity> GetAllActive()
        {
            return GetAllNoTracking().Where(w => w.IsActive);
        }

        public CustomerEntity GetByCustomerById(long customerId)
        {
            return GetAll().Where(w => w.Id == customerId).FirstOrDefault();
        }
    }
}
