using CQSAirborne.Domain;
using CQSAirborne.Model.ErrorLog;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Services.Implementation
{
    public class ErrorLogService : IErrorLogService
    {
        private readonly IErrorLogRepository _errorLogRepository;
        private readonly IDataMapper _dataMapper;
        private readonly IUnitOfWork _unitOfWork;

        public ErrorLogService(IErrorLogRepository errorLogRepository
            , IDataMapper dataMapper
            , IUnitOfWork unitOfWork)
        {
            _errorLogRepository = errorLogRepository;
            _dataMapper = dataMapper;
            _unitOfWork = unitOfWork;
        }
        public void Insert(AddEditErrorLogModel addEditErrorLogModel)
        {
            var entity = _dataMapper.Map<AddEditErrorLogModel, ErrorLogEntity>(addEditErrorLogModel);
            entity.CreatedOn = DateTime.Now;

            _errorLogRepository.Insert(entity);

            _unitOfWork.Commit();
        }
    }
}
