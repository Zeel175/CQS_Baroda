using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IGroupCodesRepository : IBaseRepository<GroupCodeEntity>
    {
        IQueryable<GroupCodeEntity> GetByModule(string moduleName);
        bool IsGlobalDocument(int documentTypeId);
    }
}
