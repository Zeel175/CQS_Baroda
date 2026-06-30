using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Plant;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Implementation
{
    public class PlantService : IPlantService
    {
        private readonly IPlantRepository _plantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataMapper _dataMapper;
        private readonly IEmployeeRepository _employeeRepository;

        public PlantService(IPlantRepository plantRepository
            , IUnitOfWork unitOfWork
            , IDataMapper dataMapper,
            IEmployeeRepository employeeRepository)
        {
            _plantRepository = plantRepository;
            _unitOfWork = unitOfWork;
            _dataMapper = dataMapper;
            _employeeRepository = employeeRepository;
        }

        public bool ChangeStatus(int id, bool status)
        {
            var entity = _plantRepository.GetById(id);
            if (entity == null)
                return false;

            entity.IsActive = status;
            return _unitOfWork.Commit() > 0;
        }

        public IQueryable<PlantListModel> GetAll()
        {
            return _dataMapper.Project<PlantEntity, PlantListModel>(
                _plantRepository.GetAllNoTracking().OrderBy(w => w.ModifiedOn));
        }

        public async Task<AddEditPlantModel> GetByIdAsync(int id)
        {
            var entity = await _plantRepository.GetByIdAsync(id);
            return _dataMapper.Map<PlantEntity, AddEditPlantModel>(entity);
        }

        public List<PlantSelectListModel> GetSelectList()
        {
            return _dataMapper.Project<PlantEntity, PlantSelectListModel>(
                _plantRepository.GetAllNoTracking()
                .Where(w => w.IsActive)
                .OrderBy(w => w.ModifiedOn)).ToList();
        }

        public async Task<bool> InsertAsync(AddEditPlantModel addEditPlantModel)
        {
            var entity = _dataMapper.Map<AddEditPlantModel, PlantEntity>(addEditPlantModel);
            entity.CreatedOn = DateTime.Now;
            entity.ModifiedOn = DateTime.Now;
            entity.IsActive = true;
            _plantRepository.Insert(entity);
            bool isSaved = await _unitOfWork.CommitAsync() > 0;
            return isSaved;
        }

        public bool IsDisplayOrderUnique(int id, int value)
        {
            return _plantRepository.IsDisplayOrderUnique(id, value);
        }

        public async Task<bool> UpdateAsync(AddEditPlantModel addEditPlantModel)
        {
            string newPlant = addEditPlantModel.Name;
            var entity = await _plantRepository.GetByIdAsync(addEditPlantModel.Id);
            if (entity == null)
                return false;

            string oldCC = entity.FinalReleaseCcEmails;
            string oldTo = entity.FinalReleaseToEmails;
            string oldPlant = entity.Name;
            
            _dataMapper.Map(addEditPlantModel, entity);
            entity.ModifiedOn = DateTime.Now;
            entity.FinalReleaseCcEmails = oldCC;
            entity.FinalReleaseToEmails = oldTo;
            _plantRepository.Update(entity);
            bool isSaved = await _unitOfWork.CommitAsync() > 0;

            ///Update Employees Plant
            if (newPlant != oldPlant)
            {
                var empList = _employeeRepository.GetAllActive().Where(a => a.PlantId == addEditPlantModel.Id).ToList();
                var data = _dataMapper.Map<List<EmployeeEntity>, List<AddEmployeeDataType>>(empList);
                var isUpdated = _employeeRepository.UpdateBulkEmployees(data,addEditPlantModel.Id);
            }

            return isSaved;
        }
    }
}
