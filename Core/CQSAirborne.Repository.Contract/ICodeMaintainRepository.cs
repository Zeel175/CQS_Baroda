using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface ICodeMaintainRepository : IBaseRepository<CodeMaintainEntity>
    {
        Task<string> GetNextNumberAsync(string moduleName);
        void UpdateLastNumer(string category);
    }
}
