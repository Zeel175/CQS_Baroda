using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IUnitOfWork
    {
        int Commit();
        Task<int> CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
