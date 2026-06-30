using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IPlantRepository : IBaseRepository<PlantEntity>
    {
        bool IsDisplayOrderUnique(int id, int value);
    }
}
