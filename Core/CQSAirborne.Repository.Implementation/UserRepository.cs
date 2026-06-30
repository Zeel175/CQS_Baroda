using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class UserRepository : BaseRepository<UserEntity>, IUserRepository
    {
        private readonly IDataContext _dataContext;

        public UserRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public Task<UserEntity> LoginUserAsync(string userName, string password)
        {
            return FirstOrDefaultAsync(w => w.UserName == userName && w.Password == password);
        }
        public async Task<UserEntity> InsertAsync(UserEntity user)
        {
            await InsertAsync(user);
            return user;
        }
    }
}
