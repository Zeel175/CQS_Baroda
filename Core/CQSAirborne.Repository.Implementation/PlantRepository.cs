using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class PlantRepository : BaseRepository<PlantEntity>, IPlantRepository
    {
        public PlantRepository(IDataContext dataContext)
            : base(dataContext)
        {
        }

        public bool IsDisplayOrderUnique(int id, int value)
        {
            return GetAllNoTracking()
                .Where(w => w.Id != id && w.DisplayOrder == value)
                .Count() == 0;
        }
    }
}
