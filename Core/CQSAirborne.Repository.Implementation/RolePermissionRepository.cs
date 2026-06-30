using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Model;
using CQSAirborne.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Repository.Implementation
{
    public class RolePermissionRepository : BaseRepository<RolePermissionEntity>, IRolePermissionRepository
    {
        private readonly IDataContext _dataContext;
        public RolePermissionRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }
    }
}
