using CQSAirborne.Domain;
using CQSAirborne.Model.ExternalLink;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Implementation
{
    public class ExternalLinkService : IExternalLinkService
    {
        private readonly IExternalLinkRepository _externalLinkRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataMapper _dataMapper;

        public ExternalLinkService(IExternalLinkRepository externalLinkRepository
            , IUnitOfWork unitOfWork
            , IDataMapper dataMapper)
        {
            _externalLinkRepository = externalLinkRepository;
            _unitOfWork = unitOfWork;
            _dataMapper = dataMapper;
        }

        public IQueryable<ExternalLinkListModel> GetAll()
        {
            return _dataMapper.Project<ExternalLinkEntity, ExternalLinkListModel>
                (_externalLinkRepository.GetAllNoTracking());
        }

        public AddEditExternalLinkModel GetCreateModel()
        {
            return new AddEditExternalLinkModel
            {
                Priority = 1
            };
        }

        public bool Insert(AddEditExternalLinkModel addEditExternalLinkModel, int userId)
        {
            var entity = _dataMapper.Map<AddEditExternalLinkModel, ExternalLinkEntity>(addEditExternalLinkModel);
            entity.CreatedBy = userId;
            entity.CreatedOn = DateTime.Now;
            entity.ModifiedOn = DateTime.Now;

            _externalLinkRepository.Insert(entity);

            return _unitOfWork.Commit() > 0;
        }

        public AddEditExternalLinkModel GetById(int id)
        {
            var externalLinkEntity = _externalLinkRepository.GetById(id);
            if (externalLinkEntity == null)
                return null;

            return _dataMapper.Map<ExternalLinkEntity, AddEditExternalLinkModel>(externalLinkEntity);
        }

        public async Task<AddEditExternalLinkModel> GetByIdAsync(int id)
        {
            var externalLinkEntity = await _externalLinkRepository.FirstOrDefaultAsync(w => w.Id == id);
            if (externalLinkEntity == null)
                return null;

            return _dataMapper.Map<ExternalLinkEntity, AddEditExternalLinkModel>(externalLinkEntity);
        }

        public bool Update(AddEditExternalLinkModel addEditExternalLinkModel, int userId)
        {
            var entity = _externalLinkRepository.GetById(addEditExternalLinkModel.Id);
            if (entity == null)
                return false;
            _dataMapper.Map(addEditExternalLinkModel, entity);
            entity.ModifiedOn = DateTime.Now;

            // Save changes
            _externalLinkRepository.Update(entity);

            return _unitOfWork.Commit() > 0;
        }
    }
}
