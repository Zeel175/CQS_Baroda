using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class PictureRepository : BaseRepository<PictureEntity>, IPictureRepository
    {
        public PictureRepository(IDataContext dataContext)
            : base(dataContext)
        {
        }
    }
}
