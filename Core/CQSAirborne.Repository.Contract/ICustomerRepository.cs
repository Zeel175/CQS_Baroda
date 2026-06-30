using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface ICustomerRepository : IBaseRepository<CustomerEntity>
    {
        CustomerEntity GetByCustomerById(long customerId);
        IQueryable<CustomerEntity> GetAllActive();
    }
}
