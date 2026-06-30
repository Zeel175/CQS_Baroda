using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Model;
using CQSAirborne.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Repository.Implementation
{
    public class PermissionRepository : BaseRepository<PermissionEntity>, IPermissionRepository
    {
        private readonly IDataContext _dataContext;
        public PermissionRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }
    }
}
