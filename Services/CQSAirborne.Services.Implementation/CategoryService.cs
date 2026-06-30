using CQSAirborne.Domain;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Document;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDataMapper _dataMapper;
        private readonly IGroupCodesRepository _groupCodesRepository;
        private readonly ICodeMaintainRepository _codeMaintainRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ICategoryRepository categoryRepository
            , IDataMapper dataMapper
            , IGroupCodesRepository groupCodesRepository
            , ICodeMaintainRepository codeMaintainRepository
            , IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _dataMapper = dataMapper;
            _groupCodesRepository = groupCodesRepository;
            _codeMaintainRepository = codeMaintainRepository;
            _unitOfWork = unitOfWork;
        }

        public IQueryable<CategoryListModel> GetAll()
        {
            return _dataMapper.Project<CategoryEntity, CategoryListModel>
                (_categoryRepository.GetAllNoTracking());
        }

        public List<SelectListModel> GetCategoryTypes()
        {
            return _dataMapper.Project<GroupCodeEntity, SelectListModel>
                (_groupCodesRepository.GetByModule(Constants.ModuleNames.CategoryType))
                .ToList();
        }

        public List<SelectListModel> GetPrimaryCategories()
        {
            return _dataMapper.Project<CategoryEntity, SelectListModel>
                (_categoryRepository.GetPrimaryCategories()).OrderBy(o => o.Name)
                .ToList();
        }

        public List<CategorySelectListModel> GetAllCategories(int id)
        {
            return _dataMapper.Project<CategoryEntity, CategorySelectListModel>
                (_categoryRepository.GetAllCategory(id)).OrderBy(o => o.Name).ToList();
        }

        public async Task<AddEditCategoryModel> GetCreateModelAsync()
        {
            return new AddEditCategoryModel
            {
                Code = await _codeMaintainRepository.GetNextNumberAsync(Constants.CodeModule.Category)
            };
        }

        public async Task<bool> CreateAsync(AddEditCategoryModel addEditCategoryModel, int userId)
        {
            var entity = _dataMapper.Map<AddEditCategoryModel, CategoryEntity>(addEditCategoryModel);
            entity.CreatedBy = userId;
            entity.CreatedOn = DateTime.Now;
            entity.ModifiedOn = DateTime.Now;
            entity.IsActive = true;
            entity.Code = await _codeMaintainRepository.GetNextNumberAsync(Constants.CodeModule.Category);

            _categoryRepository.Insert(entity);
            _codeMaintainRepository.UpdateLastNumer(Constants.CodeModule.Category);
            bool isSaved = (await _unitOfWork.CommitAsync()) > 0;
            return isSaved;
        }

        public async Task<AddEditCategoryModel> GetByIdAsync(int id)
        {
            var data = await _categoryRepository.GetByIdAsync(id);
            if (data == null)
                return null;
            return _dataMapper.Map<CategoryEntity, AddEditCategoryModel>(data);
        }

        public async Task<bool> UpdateAsync(AddEditCategoryModel addEditCategoryModel, int userId)
        {
            var entity = await _categoryRepository.GetByIdAsync(addEditCategoryModel.Id);
            if (entity == null)
                return false;
            _dataMapper.Map(addEditCategoryModel, entity);
            entity.ModifiedOn = DateTime.Now;
            _categoryRepository.Update(entity);
            bool isSaved = (await _unitOfWork.CommitAsync()) > 0;
            return isSaved;
        }

        public bool IsCategryNameUnique(string categoryName, int id)
        {
            return _categoryRepository.IsCategryNameUnique(categoryName, id);
        }

        public bool IsCategryDisplayOrderUnique(int displayOrder, int id, int? primaryCategoryId)
        {
            return _categoryRepository.IsCategryDisplayOrderUnique(displayOrder, id, primaryCategoryId);
        }

        public bool ChangeStatus(int id, bool status)
        {
            var entity = _categoryRepository.GetById(id);
            if (entity == null)
                return false;

            entity.IsActive = status;
            return _unitOfWork.Commit() > 0;
        }

        public async Task<List<DocumentChartModel>> GetCategorywiseDocumentCount(long? employeeId, int? plantId)
        {
            var spData = await _categoryRepository.GetPrimaryCategorywiseDocumentCount(employeeId, plantId);

            return spData;
            //var data = _dataMapper.Project<CategoryEntity, DocumentChartModel>(_categoryRepository.GetPrimaryCategories()).ToList();
            //return data;
        }
        public async Task<List<DocumentChartModel>> GetSecondaryCategorywiseDocumentCount(long? employeeId, int? plantId)
        {
            var spData = await _categoryRepository.GetSecondaryCategorywiseDocumentCount(employeeId, plantId);

            return spData;
            //var data = _dataMapper.Project<CategoryEntity, DocumentChartModel>(_categoryRepository.GetPrimaryCategories()).ToList();
            //return data;
        }
        public static int DocCount(List<CategoryEntity> doc)
        {
            int m = 0;
            foreach (var item in doc)
            {
                m += item.Documents.Where(d => d.IsActive == true).Count();
                m += childDocCount(item.SecondaryCategories.ToList());
            }
            return m;
        }

        public static int childDocCount(List<CategoryEntity> doc)
        {
            int l = 0;
            foreach (var item in doc)
            {
                l += item.Documents.Where(d => d.IsActive == true).Count();
                l += DocCount(item.SecondaryCategories.ToList());
            }
            return l;
        }

        public async Task<List<DocumentChartModel>> GetCategorywiseDocumentCountByParentCategory(int categoryId, long? employeeId, int? plantId)
        {
            var model = await _categoryRepository.GetDocumentCountByCategoryId(categoryId, employeeId, plantId);

            return model;
        }

        public bool CategoryIsRestricted(int Id)
        {
            var model = _categoryRepository.GetAll().Where(m => m.IsRestricted == true && m.Id == Id).FirstOrDefault();
            if (model != null)
            {
                return true;
            }
            return false;
        }
    }
}
