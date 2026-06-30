using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IDocumentTagsRepository : IBaseRepository<DocumentTagsEntity>
    {
        IQueryable<DocumentTagsEntity> GetByDocumentId(int documentId);
    }
}
