using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Model;
using CQSAirborne.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Repository.Implementation
{
    public class RoleRepository : BaseRepository<RoleEntity>, IRoleRepository
    {
        private readonly IDataContext _dataContext;
        public RoleRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }
    }
}
