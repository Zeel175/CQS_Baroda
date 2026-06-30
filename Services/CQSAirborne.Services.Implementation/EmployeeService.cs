using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Employee;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Implementation
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataMapper _dataMapper;
        private readonly IUserService _userService;
        private readonly IPlantRepository _plantRepository;
        public EmployeeService(IEmployeeRepository employeeRepository
            , IUnitOfWork unitOfWork
            , IDataMapper dataMapper
            , IUserService userService, IPlantRepository plantRepository)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
            _dataMapper = dataMapper;
            _userService = userService;
            _plantRepository = plantRepository;
        }

        //public bool BulkUpdate(List<AddEditEmployeeViewModel> addEditEmployeeViewModels)
        //{
        //    //var recordsToInActive = _employeeRepository.GetAll().Where(w => !w.IsManual).ToList();
        //    //foreach (var employee in recordsToInActive)
        //    //{
        //    //    employee.IsActive = false;
        //    //    _employeeRepository.Update(employee);
        //    //}
        //    // 1. Get plant data (Alias instead of Name)
        //    var plantData1 = _plantRepository.GetAllNoTracking()
        //        .Select(p => new { p.Id, p.Alias })
        //        .ToList();

        //    // 2. Dictionary for lookup
        //    var plantLookup = plantData1.ToDictionary(
        //        x => x.Alias.Trim(),
        //        x => x.Id,
        //        StringComparer.OrdinalIgnoreCase
        //    );

        //    foreach (var employee in addEditEmployeeViewModels)
        //    {

        //        // 3. Map PlantAccess → PlantIds
        //        string plantAccess = employee.PlantAccess ?? string.Empty;

        //        var plantIds = string.Join(",",
        //            plantAccess
        //                .Split(',', StringSplitOptions.RemoveEmptyEntries)
        //                .Select(alias => alias.Trim())
        //                .Where(alias => plantLookup.ContainsKey(alias))
        //                .Select(alias => plantLookup[alias])
        //        );

        //        employee.PlantIds = plantIds;

        //        var model = _employeeRepository.GetByEmpId(employee.EmpId);
        //        if (model != null)
        //        {
        //            model.IsActive = true;
        //            model.ModifiedOn = DateTime.Now;

        //            if (employee != null && employee.Plant != null && employee.Plant != "")
        //            {
        //                try
        //                {
        //                    var plantData = _plantRepository.GetAllNoTracking().Where(a => a.Alias.Trim() == employee.Plant.Trim()).FirstOrDefault();
        //                    model.PlantId = plantData != null && plantData.Id != null ? plantData.Id : model.PlantId;
        //                }
        //                catch (Exception e)
        //                {

        //                }
        //            }
        //            model.PlantIds = plantIds;
        //            _employeeRepository.Update(model);
        //        }
        //        else
        //        {
        //            model = _dataMapper.Map<AddEditEmployeeViewModel, EmployeeEntity>(employee);
        //            model.IsActive = true;
        //            model.CreatedOn = DateTime.Now;

        //            if (employee != null && employee.Plant != null && employee.Plant != "")
        //            {
        //                try
        //                {
        //                    var plantData = _plantRepository.GetAllNoTracking().Where(a => a.Alias.Trim() == employee.Plant.Trim()).FirstOrDefault();
        //                    model.PlantId = plantData != null && plantData.Id != null ? plantData.Id : 0;
        //                }
        //                catch (Exception e)
        //                {

        //                }
        //            }

        //            _employeeRepository.Insert(model);
        //        }
        //    }
        //    return _unitOfWork.Commit() > 0;
        //}

        public BulkUploadResponseModel BulkUpdate(List<AddEditEmployeeViewModel> employees)
        {
            int createdCount = 0;
            int updatedCount = 0;

            List<string> updatedEmpIds = new List<string>();
            List<string> createdEmpIds = new List<string>();

            // ✅ 1. Load Plant Data Once
            var plantData = _plantRepository.GetAllNoTracking()
                .Select(p => new { p.Id, p.Alias })
                .ToList();

            var plantLookup = plantData.ToDictionary(
                x => x.Alias.Trim(),
                x => x.Id,
                StringComparer.OrdinalIgnoreCase
            );

            var empentity  = _employeeRepository.GetAllNoTracking();
            var empdata = _dataMapper.Project<EmployeeEntity, EmployeeListViewModel>(empentity);
            // ✅ 2. Load Existing Employees Once
            var existingEmployees = empdata.Where(a => a.IsActive == true && a.EmpId != null).Select(b=>b.EmpId).Distinct()
                .ToDictionary(x => x.Trim(), x => x);

            foreach (var employee in employees)
            {
                // ✅ Map PlantAccess → PlantIds
                string plantAccess = employee.PlantAccess ?? string.Empty;

                var plantIds = string.Join(",",
                    plantAccess
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(alias => alias.Trim())
                        .Where(alias => plantLookup.ContainsKey(alias))
                        .Select(alias => plantLookup[alias])
                );

                employee.PlantIds = plantIds;

                // ✅ Check EmpId Exists or Not
                if (existingEmployees.ContainsKey(employee.EmpId.Trim()))
                {
                    // ========================
                    // ✅ UPDATE EXISTING RECORD
                    // ========================
                    var model = _employeeRepository.GetByEmpId(employee.EmpId);
                    //var model = existingEmployees[employee.EmpId.Trim()];

                    model.IsActive = true;
                    model.ModifiedOn = DateTime.Now;
                    model.PlantIds = plantIds;

                    // Plant Mapping
                    if (!string.IsNullOrWhiteSpace(employee.Plant))
                    {
                        var plantObj = plantData.FirstOrDefault(x =>
                            x.Alias.Equals(employee.Plant.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (plantObj != null)
                            model.PlantId = plantObj.Id;
                    }

                    _employeeRepository.Update(model);

                    updatedCount++;
                    updatedEmpIds.Add(employee.EmpId);
                }
                else
                {
                    // ========================
                    // ✅ INSERT NEW RECORD
                    // ========================
                    var model = _dataMapper.Map<AddEditEmployeeViewModel, EmployeeEntity>(employee);

                    model.IsActive = true;
                    model.CreatedOn = DateTime.Now;
                    model.PlantIds = plantIds;

                    // Plant Mapping
                    if (!string.IsNullOrWhiteSpace(employee.Plant))
                    {
                        var plantObj = plantData.FirstOrDefault(x =>
                            x.Alias.Equals(employee.Plant.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (plantObj != null)
                            model.PlantId = plantObj.Id;
                    }

                    _employeeRepository.Insert(model);

                    createdCount++;
                    createdEmpIds.Add(employee.EmpId);
                }
            }

            // ✅ Commit Once
            var commitResult = _unitOfWork.Commit();

            if (commitResult > 0)
            {
                return new BulkUploadResponseModel
                {
                    IsSuccess = true,
                    CreatedCount = createdCount,
                    UpdatedCount = updatedCount,
                    UpdatedEmpIds = updatedEmpIds,
                    CreatedEmpIds = createdEmpIds,

                    Message =
                        $"✅ Upload Completed Successfully! Created: {createdCount}, Updated: {updatedCount}"
                };
            }

            return new BulkUploadResponseModel
            {
                IsSuccess = false,
                CreatedCount = 0,
                UpdatedCount = 0,
                UpdatedEmpIds = new List<string>(),
                CreatedEmpIds = new List<string>(),
                Message = "❌ Upload Failed. Please try again."
            };
        }

        public bool BulkUpdateWithProcedure(List<AddEditEmployeeViewModel> addEditEmployeeViewModels, bool isManual)
        {
            var data = _dataMapper.Map<List<AddEditEmployeeViewModel>, List<AddEmployeeDataType>>(addEditEmployeeViewModels);
            return _employeeRepository.BulkInsertEmployeeWithProcedure(data, isManual);
        }

        public IQueryable<EmployeeListViewModel> GetAll()
        {
            return _dataMapper.Project<EmployeeEntity, EmployeeListViewModel>(
                _employeeRepository.GetAllNoTracking());
        }
        public IQueryable<EmployeeListViewModel> GetAllActive()
        {
            return _dataMapper.Project<EmployeeEntity, EmployeeListViewModel>(
                _employeeRepository.GetAllActive());
        }
        public AddEditEmployeeViewModel GetById(long id)
        {
            var entity = _employeeRepository.GetById(id);
            if (entity == null)
                return null;
            return _dataMapper.Map<EmployeeEntity, AddEditEmployeeViewModel>(entity);
        }

        public bool Insert(AddEditEmployeeViewModel addEditEmployeeViewModel)
        {
            if (addEditEmployeeViewModel.PlantIdList != null && addEditEmployeeViewModel.PlantIdList.Count() > 0)
            {
                addEditEmployeeViewModel.PlantIds = string.Join(",", addEditEmployeeViewModel.PlantIdList);
            }

            var entity = _dataMapper.Map<AddEditEmployeeViewModel, EmployeeEntity>(addEditEmployeeViewModel);
            entity.IsActive = true;
            entity.CreatedOn = DateTime.Now;
            if (addEditEmployeeViewModel != null && addEditEmployeeViewModel.PlantId != null && addEditEmployeeViewModel.PlantId != 0)
            {
                var plantData = _plantRepository.GetById(addEditEmployeeViewModel.PlantId);
                entity.Plant = plantData != null && plantData.Name != null ? plantData.Name : "";
            }
            _employeeRepository.Insert(entity);
            if (addEditEmployeeViewModel.ADID == null && addEditEmployeeViewModel.UserName != null && addEditEmployeeViewModel.Password != null)
            {
                var user = new AddEditUserViewModel()
                {
                    Email = addEditEmployeeViewModel.OfficalEmpEmailID,
                    FirstName = addEditEmployeeViewModel.EmployeeName,
                    LastName = addEditEmployeeViewModel.EmployeeName,
                    FullName = addEditEmployeeViewModel.EmployeeName,
                    EmployeeId = addEditEmployeeViewModel.EmpId,
                    Password = addEditEmployeeViewModel.Password,
                    PasswordHash = addEditEmployeeViewModel.Password,
                    UserName = addEditEmployeeViewModel.UserName
                };
                _userService.Insert(user);
            }
            return _unitOfWork.Commit() > 0;
        }

        public bool Update(AddEditEmployeeViewModel addEditEmployeeViewModel)
        {
            if (addEditEmployeeViewModel.PlantIdList != null && addEditEmployeeViewModel.PlantIdList.Count() > 0)
            {
                addEditEmployeeViewModel.PlantIds = string.Join(",", addEditEmployeeViewModel.PlantIdList);
            }
            var entity = _employeeRepository.GetById(addEditEmployeeViewModel.Id);
            if (entity == null)
                return false;
            bool isManual = entity.IsManual;
            _dataMapper.Map(addEditEmployeeViewModel, entity);
            entity.ModifiedOn = DateTime.Now;
            entity.IsManual = isManual;

            if (addEditEmployeeViewModel != null && addEditEmployeeViewModel.PlantId != null && addEditEmployeeViewModel.PlantId != 0)
            {
                var plantData = _plantRepository.GetById(addEditEmployeeViewModel.PlantId);
                entity.Plant = plantData != null && plantData.Name != null ? plantData.Name : "";
            }

            _employeeRepository.Update(entity);

            return _unitOfWork.Commit() > 0;
        }
        public async Task<bool> ChangePassword(string Password, string UserName)
        {
            try
            {
                var entity = await _employeeRepository.FirstOrDefaultAsync(m => m.UserName == UserName);
                if (entity == null)
                    return false;
                entity.Password = Password;
                _employeeRepository.Update(entity);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
            }
            return true;
        }
        public async Task<bool> ResetPassword(ResetPasswordViewModel model)
        {
            var rowToken = _userService.DecryptData(model.Token);
            if (!string.IsNullOrEmpty(rowToken))
            {
                var info = rowToken.Split(';');
                var username = info[0];
                var expireyDateTime = Convert.ToDateTime(info[1]);
                var tokenDateTime = Convert.ToDateTime(info[2]);
                if (expireyDateTime > DateTime.Now)
                    return false;
                var entity = await _employeeRepository.FirstOrDefaultAsync(m => m.UserName == username);
                if (entity == null)
                    return false;
                if (entity.ModifiedOn >= tokenDateTime)
                    return false;
                await ChangePassword(username, model.Password);
            }
            return false;
        }

        public bool DeleteEmployee(long id)
        {
            bool result = false;
            var entity = _employeeRepository.GetAll().Where(a => a.Id == id).FirstOrDefault();
            entity.IsActive = false;
            entity.ModifiedOn = DateTime.Now;
            //entity.ModifiedBy = UserId;
            _employeeRepository.Update(entity);
            result = true;
            _unitOfWork.Commit();
            return result;
        }
        public bool ChangeStatus(int id, bool status)
        {
            var entity = _employeeRepository.GetById(id);
            if (entity == null)
                return false;

            entity.IsActive = status;
            return _unitOfWork.Commit() > 0;
        }

        public async Task<List<EmployeeListViewModel>> GetEmployeeListForViewPageAsync()
        {
            return await _employeeRepository.GetEmployeeListWithProcedure();
        }
    }
}
