using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class CodeMaintainRepository : BaseRepository<CodeMaintainEntity>, ICodeMaintainRepository
    {
        public CodeMaintainRepository(IDataContext dataContext)
            : base(dataContext)
        {
        }

        public async Task<string> GetNextNumberAsync(string moduleName)
        {
            var data = await FirstOrDefaultAsync(w => w.ModuleName == moduleName);
            if (data == null)
                return string.Empty;

            return $"{data.Prefix}{(data.LastNumber + 1).ToString().PadLeft(data.Padding, '0')}";
        }

        public void UpdateLastNumer(string category)
        {
            var data = GetAll().FirstOrDefault(w => w.ModuleName == category);
            if (data != null)
            {
                data.LastNumber = data.LastNumber + 1;
                Update(data);
            }
        }

      

    }
}
