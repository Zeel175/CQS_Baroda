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
    public class EmailHistoryRepository : BaseRepository<EmailHistoryEntity>, IEmailHistoryRepository
    {
        private readonly IStoredProcedureContext _storedProcedureContext;
        public EmailHistoryRepository(IDataContext dataContext
            , IStoredProcedureContext storedProcedureContext)
            : base(dataContext)
        {
            _storedProcedureContext = storedProcedureContext;
        }
    }
}
