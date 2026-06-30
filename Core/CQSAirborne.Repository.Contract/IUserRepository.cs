using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IUserRepository : IBaseRepository<UserEntity>
    {
        Task<UserEntity> LoginUserAsync(string userName, string password);
        Task<UserEntity> InsertAsync(UserEntity user);
    }
}
