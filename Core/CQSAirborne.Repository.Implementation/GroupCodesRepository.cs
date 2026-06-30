using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class GroupCodesRepository : BaseRepository<GroupCodeEntity>, IGroupCodesRepository
    {
        public GroupCodesRepository(IDataContext dataContext)
            : base(dataContext)
        {
        }

        public IQueryable<GroupCodeEntity> GetByModule(string moduleName)
        {
            return GetAllNoTracking()
                .Where(w => w.ModuleName == moduleName)
                .OrderBy(w => w.Name);
        }

        public bool IsGlobalDocument(int documentTypeId)
        {
            return GetAllNoTracking().Where(w => w.Code == "DOC_GLBL" && w.Id == documentTypeId).Any();
        }
    }
}
