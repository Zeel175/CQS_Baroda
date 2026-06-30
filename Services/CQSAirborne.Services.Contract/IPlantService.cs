using CQSAirborne.Model.Core;
using CQSAirborne.Model.Plant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IPlantService
    {
        IQueryable<PlantListModel> GetAll();
        Task<bool> InsertAsync(AddEditPlantModel addEditPlantModel);
        List<PlantSelectListModel> GetSelectList();
        Task<AddEditPlantModel> GetByIdAsync(int id);
        Task<bool> UpdateAsync(AddEditPlantModel addEditPlantModel);
        bool IsDisplayOrderUnique(int id, int value);
        bool ChangeStatus(int id, bool status);
    }
}
