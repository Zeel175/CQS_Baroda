using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Employee;
using CQSAirborne.Model.CPRMaster;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
//using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Security.Cryptography;
using System.Threading;
using CQSAirborne.Services.Implementation.Utils;
using System.Reflection;
using Serilog;

namespace CQSAirborne.Services.Implementation
{
    public class CPRMasterService : ICPRMasterService
    {
        private readonly ICPRMasterRepository _cPRMasterRepository;
        private readonly ICPRMasterApproverRepository _cPRMasterApproverRepository;
        private readonly ICustomerDocumentMappingRepository _customerDocumentMappingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataMapper _dataMapper;
        private readonly IUserService _userService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentPlantRepository _documentPlantRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly EmailHelper _emailHelper;
        private readonly IGroupCodesRepository _groupCodesRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly IEmployeeRepository _emp;
        private readonly ICategoryRepository _categoryRepository;

        public CPRMasterService(ICPRMasterRepository cPRMasterRepository
            , IUnitOfWork unitOfWork
            , IDataMapper dataMapper
            , IUserService userService, ICustomerDocumentMappingRepository customerDocumentMappingRepository,
            IDocumentRepository documentRepository, IDocumentPlantRepository documentPlantRepository,
            IHostingEnvironment hostingEnvironment, IConfiguration configuration, EmailHelper emailHelper,
            ICPRMasterApproverRepository cPRMasterApproverRepository
            , IGroupCodesRepository groupCodesRepository
            , IPlantRepository plantRepository, IEmployeeRepository emp, ICategoryRepository categoryRepository
            )
        {
            _cPRMasterRepository = cPRMasterRepository;
            _unitOfWork = unitOfWork;
            _dataMapper = dataMapper;
            _userService = userService;
            _customerDocumentMappingRepository = customerDocumentMappingRepository;
            _documentRepository = documentRepository;
            _documentPlantRepository = documentPlantRepository;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _emailHelper = emailHelper;
            _cPRMasterApproverRepository = cPRMasterApproverRepository;
            _groupCodesRepository = groupCodesRepository;
            _plantRepository = plantRepository;
            _emp = emp;
            _categoryRepository = categoryRepository;
        }

        public IQueryable<CPRMasterModel> GetAll()
        {
            return _dataMapper.Project<CPRMasterEntity, CPRMasterModel>(
                _cPRMasterRepository.GetAllNoTracking().Where(a => a.IsActive == true));
        }

        public async Task<CPRMasterModel> GetByIdAsync(long id)
        {

            try
            {
                var custEntity = _cPRMasterRepository.GetById(id);
                //        var custEntity = _cPRMasterRepository.GetAllNoTracking()
                //.Include(x => x.ApprovalsEntity)
                //.FirstOrDefault(a => a.Id == id);
                if (custEntity == null)
                    return null;


                var detail = custEntity.ApprovalsEntity.ToList().Where(a => a.IsActive == true).ToList();
                custEntity.ApprovalsEntity.Clear();
                custEntity.ApprovalsEntity = detail;

                var model = _dataMapper.Map<CPRMasterEntity, CPRMasterModel>(custEntity);
                return model;
            }
            catch (Exception ee)
            {
                return null;
            }
        }
        public async Task<bool> CreateEditAsync(CPRMasterModel model, int userId)
        {
            try
            {
                if (model.Id == null || model.Id == 0)                 // CREATE
                {
                    var entity = _dataMapper.Map<CPRMasterModel, CPRMasterEntity>(model);

                    // === Generate CPR-XXXXXXX (7 digits) ===
                    entity.CPRUniqueCode = GetNextCprUniqueCode(model.PlantId ?? 0);

                    // Default workflow state
                    if (model.StageId != Constants.CPRStage.Level_I && model.StatusId != Constants.CPRStatusType.Awaiting_Approval)
                    {
                        entity.StageId = Constants.CPRStage.Generate;      // 13
                        entity.StatusId = Constants.CPRStatusType.Open;     // 9
                    }
                    entity.AdminStatusId = Constants.CPRStatusType.Open;     // 9

                    entity.RequestedById = userId;
                    entity.IsActive = true;
                    entity.CreatedOn = DateTime.Now;
                    entity.ModifiedOn = DateTime.Now;
                    entity.CreatedBy = userId;
                    entity.ModifiedBy = userId;
                    entity.ADC = model.ADC != null && model.ADC != DateTime.MinValue ? model.ADC : null;
                    entity.EDC = model.EDC != null && model.EDC != DateTime.MinValue ? model.EDC : null;
                    _cPRMasterRepository.Insert(entity);

                    // ---- Workflow transition rules ----
                    bool isSendMail = false;
                    // If user selects "Awaiting Approval" from UI, move to Level I (Vertical Head)
                    if (model.StatusId == Constants.CPRStatusType.Awaiting_Approval)
                    {
                        // Only jump to Level I when coming from a "draft-like" state

                        entity.StageId = Constants.CPRStage.Level_I;                // 14
                        entity.StatusId = Constants.CPRStatusType.Awaiting_Approval; // 10
                        isSendMail = true;
                    }
                    var resul = _unitOfWork.Commit() > 0;
                    var isAwaiting = entity.StatusId == Constants.CPRStatusType.Awaiting_Approval;
                    if (isAwaiting)
                    {
                        IEnumerable<string> desiredApproverIds = Enumerable.Empty<string>();
                        if (entity.StageId == Constants.CPRStage.Level_I)
                        {
                            desiredApproverIds = entity.ProcessOwners?.Split(",");
                        }
                        if (entity.StageId == Constants.CPRStage.Level_II)
                        {
                            desiredApproverIds = entity.RelevantVerticalHeads?.Split(",");
                        }

                        // Will add missing, keep existing, and inactivate removed
                        await SyncApproversAsync(entity.Id, (int)entity.StageId, desiredApproverIds, userId);
                        _unitOfWork.Commit();
                    }


                    if (isSendMail == true)
                    {
                        SendCPRNotificationEmail(entity, "Awaiting Approval", _emp.GetById(userId)?.EmployeeName, null, DateTime.Now);//V: 2.0 //Added by : Dhruv //Requested By: Mani sir //Date: 11 Feb 2026 
                    }

                    return resul;
                }
                else                                                   // UPDATE
                {
                    bool? isclosed = model.IsClosed;
                    var existing = _cPRMasterRepository.GetById(model.Id);
                    if (existing == null) return false;

                    // Keep old audit fields for later
                    var createdBy = existing.CreatedBy;
                    var createdOn = existing.CreatedOn;
                    var prevStage = existing.StageId;
                    var prevStatus = existing.StatusId;
                    var isActive = existing.IsActive;
                    var oldStageId = existing.StageId;
                    var oldStatusId = existing.StatusId;
                    var oldIsClosed = existing.IsClosed;
                    var adminStatusId = existing.AdminStatusId;
                    var adminStatusDate = existing.AdminStatusDate;

                    // 🔥 Detect approver changes
                    bool isApproverChanged =
                        (existing.ProcessOwners ?? "") != (model.ProcessOwners ?? "")
                        || (existing.RelevantVerticalHeads ?? "") != (model.RelevantVerticalHeads ?? "");

                    // Map incoming changes onto the tracked entity
                    var entity = _dataMapper.Map<CPRMasterModel, CPRMasterEntity>(model, existing);

                    // Preserve original audit fields
                    entity.CreatedBy = createdBy;
                    entity.CreatedOn = createdOn;
                    entity.StageId = oldStageId;
                    entity.StatusId = oldStatusId;

                    entity.AdminStatusDate = adminStatusDate;
                    entity.AdminStatusId = adminStatusId;

                    if (isclosed != null && isclosed == true && oldIsClosed != null && oldIsClosed == false)
                    {
                        entity.ClosedDate = DateTime.Now;
                        entity.StatusId = Constants.CPRStatusType.Closed;
                        entity.FinalApprovedById = userId;
                    }
                    bool isSendMail = false;
                    // ---- Workflow transition rules ----
                    // If user selects "Awaiting Approval" from UI, move to Level I (Vertical Head)
                    if (model.StatusId == Constants.CPRStatusType.Awaiting_Approval)
                    {
                        // Only jump to Level I when coming from a "draft-like" state
                        if (prevStage == null
                            || prevStage == Constants.CPRStage.Generate
                            || prevStatus == Constants.CPRStatusType.Open)
                        {
                            entity.StageId = Constants.CPRStage.Level_I;                // 14
                            entity.StatusId = Constants.CPRStatusType.Awaiting_Approval; // 10
                            isSendMail = true;
                        }
                        else
                        {
                            entity.StageId = model.StageId;
                            entity.StatusId = model.StatusId;
                        }
                    }

                    var isAwaiting = entity.StatusId == Constants.CPRStatusType.Awaiting_Approval;
                    if (isAwaiting)
                    {
                        IEnumerable<string> desiredApproverIds = Enumerable.Empty<string>();
                        if (entity.StageId == Constants.CPRStage.Level_I)
                        {
                            desiredApproverIds = entity.ProcessOwners?.Split(",");
                        }
                        if (entity.StageId == Constants.CPRStage.Level_II)
                        {
                            desiredApproverIds = entity.RelevantVerticalHeads?.Split(",");
                        }

                        // Will add missing, keep existing, and inactivate removed
                        await SyncApproversAsync(entity.Id, (int)entity.StageId, desiredApproverIds, userId);
                    }

                    // If someone flips status back to Open, keep it at Generate
                    if (model.StatusId == Constants.CPRStatusType.Open && prevStage == null)
                    {
                        entity.StageId = Constants.CPRStage.Generate;
                    }
                    // -----------------------------------

                    entity.ModifiedOn = DateTime.Now;
                    entity.ModifiedBy = userId;
                    entity.IsActive = isActive;
                    _cPRMasterRepository.Update(entity);

                    var data = _unitOfWork.Commit() > 0;


                    var isAwaitingFinal = entity.StatusId == Constants.CPRStatusType.Awaiting_Approval;
                    //if (isSendMail == true)
                    if (isSendMail == true || (isApproverChanged && isAwaitingFinal))
                    {
                        SendCPRNotificationEmail(entity, "Awaiting Approval", _emp.GetById(userId)?.EmployeeName, null, DateTime.Now);//V: 2.0 //Added by : Dhruv //Requested By: Mani sir //Date: 11 Feb 2026 //Updated by Dhruv //Requested By Dayana //Date: 06 Apr 2026
                    }

                    return data;
                }


            }
            catch (Exception ee)
            {
                return false;
            }
        }


        /// <summary>
        /// Generates the next CPRUniqueCode in the format "CPR-0000001".
        /// Looks for the highest existing code with the same prefix and increments it.
        /// </summary>
        //private string GetNextCprUniqueCode(long plantId)
        //{
        //    const string prefix = "CPR-";
        //    var plant = _plantRepository.GetAllNoTracking().Where(a => a.Id == plantId).FirstOrDefault();
        //    // because the numeric part is zero-padded to 7 digits,
        //    // string ordering matches numeric ordering
        //    var lastCode = _cPRMasterRepository.GetAllNoTracking()
        //        .Where(x => x.CPRUniqueCode != null && x.CPRUniqueCode.StartsWith(prefix) && x.PlantId == plantId)
        //        .OrderByDescending(x => x.CPRUniqueCode)
        //        .Select(x => x.CPRUniqueCode)
        //        .FirstOrDefault();

        //    long lastNumber = 0;
        //    if (!string.IsNullOrWhiteSpace(lastCode))
        //    {
        //        var numberPart = lastCode.Substring(prefix.Length).Trim();
        //        long.TryParse(numberPart, out lastNumber);
        //    }

        //    var nextNumber = lastNumber + 1;
        //    return prefix + nextNumber.ToString("D7"); // pad to 7 digits
        //}
        private string GetNextCprUniqueCode(int plantId)
        {
            const string prefix = "CPR-";

            // 1️⃣ Get Plant Alias
            var plant = _plantRepository
                .GetAllNoTracking()
                .Where(p => p.Id == plantId)
                .Select(p => new { p.Id, p.Alias })
                .FirstOrDefault();

            if (plant == null || string.IsNullOrWhiteSpace(plant.Alias))
                throw new Exception("Invalid Plant or PlantAlias not configured");

            // IMPORTANT: Keep PlantAlias AS-IS (spaces & hyphens allowed)
            var plantAlias = plant.Alias.Trim().ToUpper();

            // CPR-TASL B01-
            var fullPrefix = $"{prefix}{plantAlias}-";

            // 2️⃣ Get last CPR for this plant
            var lastCode = _cPRMasterRepository
                .GetAllNoTracking()
                .Where(x =>
                    x.PlantId == plantId &&
                    x.CPRUniqueCode != null &&
                    x.CPRUniqueCode.StartsWith(fullPrefix))
                .OrderByDescending(x => x.CPRUniqueCode)
                .Select(x => x.CPRUniqueCode)
                .FirstOrDefault();

            long lastNumber = 0;

            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                // Always safe because we cut AFTER the exact prefix
                var numberPart = lastCode.Substring(fullPrefix.Length);
                long.TryParse(numberPart, out lastNumber);
            }

            // 3️⃣ Generate next CPR
            var nextNumber = lastNumber + 1;

            return $"{fullPrefix}{nextNumber:D7}";
        }


        /// <summary>
        /// Synchronize Approvals table for the current stage:
        /// - Add entries for new approvers (Pending/Awaiting Approval)
        /// - Keep existing entries untouched
        /// </summary>
        private async Task SyncApproversAsync(long cprId, int stageId, IEnumerable<string> desiredApproverIds, int userId)
        {
            // Flatten & parse string user ids -> long ids
            var desiredIds = ((desiredApproverIds ?? Enumerable.Empty<string>())
                                .SelectMany(s => (s ?? string.Empty)
                                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)))
                                .Select(s => long.TryParse(s, out var v) ? (long?)v : null)
                                .Where(v => v.HasValue)
                                .Select(v => v.Value)
                                .ToHashSet();

            // Existing rows for this CPR+Stage
            var existing = _cPRMasterApproverRepository.GetAll()
                            .Where(a => a.CPRMasterId == cprId && a.StageId == stageId
                            && a.ApprovalStatusId != (int)Constants.CPRStatusType.Rejected
                            && a.ApprovalStatusId != (int)Constants.CPRStatusType.Approved)
                            .ToList();

            var existingActive = existing.Where(a => a.IsActive).ToList();
            var existingActiveIds = existingActive
                                    .Where(a => a.UserId.HasValue)
                                    .Select(a => a.UserId.Value)
                                    .ToHashSet();

            // --- Add missing approvers (or re-activate if an inactive row already exists) ---
            var toAddIds = desiredIds.Except(existingActiveIds).ToList();
            foreach (var approverId in toAddIds)
            {
                // If there is an inactive row for this user, re-activate instead of inserting
                var inactiveRow = existing
                    .FirstOrDefault(a => a.UserId == approverId && !a.IsActive);

                if (inactiveRow != null)
                {
                    inactiveRow.IsActive = true;
                    inactiveRow.ApprovalStatusId = Constants.CPRStatusType.Awaiting_Approval;
                    inactiveRow.ModifiedOn = DateTime.Now;
                    inactiveRow.ModifiedBy = userId;
                    _cPRMasterApproverRepository.Update(inactiveRow);
                }
                else
                {
                    var row = new CPRMasterApproverDetailEntity
                    {
                        CPRMasterId = cprId,
                        StageId = stageId,
                        UserId = approverId,
                        ApprovalStatusId = Constants.CPRStatusType.Awaiting_Approval,
                        IsActive = true,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now,
                        CreatedBy = userId,
                        ModifiedBy = userId,
                        ApproverRemarks = null
                    };
                    _cPRMasterApproverRepository.Insert(row);
                }
            }

            // --- Inactivate removed approvers ---
            var toInactivate = existingActive
                .Where(a => a.UserId.HasValue && !desiredIds.Contains(a.UserId.Value))
                .ToList();

            foreach (var row in toInactivate)
            {
                row.IsActive = false;
                row.ModifiedOn = DateTime.Now;
                row.ModifiedBy = userId;
                _cPRMasterApproverRepository.Update(row);
            }
        }

        public async Task<bool> DeleteAsync(long id, int UserId)
        {
            bool result = false;
            var entity = _cPRMasterRepository.GetAll().Where(a => a.Id == id).FirstOrDefault();
            entity.IsActive = false;
            entity.ModifiedOn = DateTime.Now;
            entity.ModifiedBy = UserId;
            _cPRMasterRepository.Update(entity);

            var entity3 = _cPRMasterApproverRepository.GetAll().Where(a => a.CPRMasterId == id && a.IsActive == true);
            if (entity3 != null && entity3.Count() > 0)
            {
                foreach (var item in entity3)
                {
                    item.IsActive = false;
                    item.ModifiedOn = DateTime.Now;
                    item.ModifiedBy = UserId;
                    _cPRMasterApproverRepository.Update(item);
                }
            }
            result = true;
            _unitOfWork.Commit();
            return result;
        }

        public async Task<List<SelectListModel>> GetCPRMasterStatusAsync()
        {
            var entity = _groupCodesRepository.GetByModule(Constants.ModuleNames.CPRStatusType);
            var data = _dataMapper.Project<GroupCodeEntity, SelectListModel>
                (entity);
            return data.ToList();
        }
        public async Task<List<SelectListModel>> GetCPRMasterStageAsync()
        {
            var entity = _groupCodesRepository.GetByModule(Constants.ModuleNames.CPRStage);
            var data = _dataMapper.Project<GroupCodeEntity, SelectListModel>
                (entity);
            return data.ToList();
        }

        public async Task<bool> UpdateStageStatusAsync(CPRStageStatusUpdateModel model, int userId)
        {
            var e = _cPRMasterRepository.GetById(model.CPRId);
            if (e == null) return false;

            if (model.IsFromAdminSide == true)
            {
                long oldAdminStatusId = e.AdminStatusId ?? 0;
                bool isSendMail = false;
                e.AdminStatusId = model.RequestedStatusId;   // APPR or REJ
                e.AdminStatusDate = DateTime.Now;
                e.FinalApprovedById = userId;
                if (model.IsClosed != null && model.IsClosed == true)
                {
                    e.IsClosed = true;
                    e.ClosedDate = DateTime.Now;
                    e.StatusId = Constants.CPRStatusType.Closed;

                }
                e.EDC = model.EDC;
                e.ADC = model.ADC;
                e.AdminRemarks = model.AdminRemarks;
                e.ModifiedOn = DateTime.Now;
                e.ModifiedBy = userId;
                _cPRMasterRepository.Update(e);
                var resu = _unitOfWork.Commit() > 0;

                if (e.AdminStatusId == Constants.CPRStatusType.Approved && oldAdminStatusId != e.AdminStatusId && e.IsClosed == true)
                {

                    SendCPRNotificationEmail(
                               e,
                               "CPR Closed",
                               _emp.GetById(userId.ToString())?.EmployeeName,
                               "",
                               DateTime.Now);
                }
                else if (e.AdminStatusId == Constants.CPRStatusType.Approved && oldAdminStatusId != e.AdminStatusId)
                {
                    SendCPRNotificationEmail(
                                   e,
                                   "Released",
                                   _emp.GetById(userId.ToString())?.EmployeeName,
                                   "",
                                   DateTime.Now);
                }
                else if (e.AdminStatusId == Constants.CPRStatusType.Rejected && oldAdminStatusId != e.AdminStatusId)
                {
                    SendCPRNotificationEmail(
                                  e,
                                  "QMS Rejected",
                                  _emp.GetById(userId.ToString())?.EmployeeName,
                                  "",
                                  DateTime.Now);
                }
                return resu;
            }
            else
            {

                // Short-hands
                var GEN = Constants.CPRStage.Generate;
                var L1 = Constants.CPRStage.Level_I;
                var L2 = Constants.CPRStage.Level_II;

                var OPEN = Constants.CPRStatusType.Open;
                var AWAIT = Constants.CPRStatusType.Awaiting_Approval;
                var APPR = Constants.CPRStatusType.Approved;
                var REJ = Constants.CPRStatusType.Rejected;

                int curStage = (int)(e.StageId ?? 0);
                int curStatus = (int)(e.StatusId ?? 0);

                int nextStage = curStage;
                int nextStatus = curStatus;
                bool allowed = false;

                // helper to split CSV ids (inside UpdateStageStatusAsync)
                IEnumerable<string> SplitIds(string s)
                {
                    return (s ?? string.Empty)
                        .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }

                // -------- 1) Decide transition ----------
                if (model.RequestedStatusId == AWAIT)
                {
                    // First submit from Generate/Open -> L1 Awaiting
                    if ((curStage == GEN && (curStatus == OPEN || curStatus == 0)) ||
                        (curStage == 0 && (curStatus == OPEN || curStatus == 0)))
                    {
                        nextStage = L1; nextStatus = AWAIT; allowed = true;
                    }
                    // Resubmit after rejection => same stage back to Awaiting
                    else if (curStatus == REJ && (curStage == L1 || curStage == L2))
                    {
                        nextStage = curStage; nextStatus = AWAIT; allowed = true;
                    }
                }
                else if (model.RequestedStatusId == APPR)
                {
                    // L1 Approves -> L2 Awaiting
                    if (curStage == L1 && curStatus == AWAIT)
                    {
                        nextStage = L2; nextStatus = AWAIT; allowed = true;
                    }
                    // L2 Approves -> final Approved
                    else if (curStage == L2 && curStatus == AWAIT)
                    {
                        nextStage = L2; nextStatus = APPR; allowed = true;
                    }

                    //SendCPRNotificationEmail(e, "Approved", _emp.GetById(userId)?.EmployeeName, model.Remarks, DateTime.Now);//V: 2.0 //Added by : Dhruv //Requested By: Mani sir //Date: 11 Feb 2026
                }
                else if (model.RequestedStatusId == REJ)
                {
                    // Reject keeps same stage
                    if ((curStage == L1 || curStage == L2) && curStatus == AWAIT)
                    {
                        nextStage = curStage; nextStatus = REJ; allowed = true;
                    }

                    //SendCPRNotificationEmail(e, "Rejected", _emp.GetById(userId)?.EmployeeName, model.Remarks, DateTime.Now);//V: 2.0 //Added by : Dhruv //Requested By: Mani sir //Date: 11 Feb 2026
                }

                if (!allowed) return false;

                // -------- 2) Update approver rows for THIS stage (only those in Awaiting) ----------
                // Use the StageId sent by client to be explicit
                int stageFilter = curStage;

                if (model.RequestedStatusId == APPR || model.RequestedStatusId == REJ)
                {
                    var pendingRows = _cPRMasterApproverRepository.GetAll()
                        .Where(a => a.CPRMasterId == e.Id
                                    && a.StageId == stageFilter
                                    && a.IsActive
                                    && a.ApprovalStatusId == AWAIT)
                        .ToList();

                    var now = DateTime.Now;
                    foreach (var row in pendingRows)
                    {
                        row.ApprovalStatusId = model.RequestedStatusId;   // APPR or REJ
                        row.StatusDateTime = now;
                        row.ApproverRemarks = model.Remarks;
                        row.ModifiedOn = now;
                        row.ModifiedBy = userId;
                        _cPRMasterApproverRepository.Update(row);
                    }
                }

                // -------- 3) Move master to new Stage/Status ----------
                e.StageId = nextStage;
                e.StatusId = nextStatus;
                e.ModifiedOn = DateTime.Now;
                e.ModifiedBy = userId;
                _cPRMasterRepository.Update(e);

                // -------- 4) Seed approvers for the "Awaiting" stage we just entered ----------
                if (nextStatus == AWAIT)
                {
                    if (nextStage == L1)
                    {
                        // entering Level 1 awaiting -> ensure Vertical Heads rows exist 
                        //V: 2.0 //Changed by : Dhruv //Requested By: Mani sir //Mail Date: 09 Oct 2025 14:58
                        await SyncApproversAsync(e.Id, L1, SplitIds(e.ProcessOwners), userId);
                    }
                    else if (nextStage == L2)
                    {
                        // (a) L1 Approved or (b) resubmitted L2 awaiting -> ensure Process Owners rows exist
                        await SyncApproversAsync(e.Id, L2, SplitIds(e.RelevantVerticalHeads), userId);

                        //// Optional: deactivate any still-active L1 rows once we move to L2
                        //var l1Active = _cPRMasterApproverRepository.GetAll()
                        //    .Where(a => a.CPRMasterId == e.Id && a.StageId == L1 && a.IsActive)
                        //    .ToList();
                        //foreach (var row in l1Active)
                        //{
                        //    row.IsActive = false;
                        //    row.ModifiedOn = DateTime.Now;
                        //    row.ModifiedBy = userId;
                        //    _cPRMasterApproverRepository.Update(row);
                        //}
                    }

                }

                //return _unitOfWork.Commit() > 0;
                var result = _unitOfWork.Commit() > 0;

                if (result)
                {
                    if (nextStatus == AWAIT)
                    {
                        SendCPRNotificationEmail(
                            e,
                            "Awaiting Approval",
                            _emp.GetById(userId.ToString())?.EmployeeName,
                            null,
                            DateTime.Now);
                    }
                    else if (nextStatus == APPR)
                    {
                        SendCPRNotificationEmail(
                            e,
                            "Approved",
                            _emp.GetById(userId.ToString())?.EmployeeName,
                            model.Remarks,
                            DateTime.Now);
                    }
                    else if (nextStatus == REJ)
                    {
                        SendCPRNotificationEmail(
                            e,
                            "Rejected",
                            _emp.GetById(userId.ToString())?.EmployeeName,
                            model.Remarks,
                            DateTime.Now);
                    }
                }

                return result;

            }
        }



        private void SendCPRNotificationEmail(
           CPRMasterEntity entity,
           string statusText,
           string approverName,
           string remarks,
           DateTime actionDate)
        {
            try
            {
                string DMRUserIds = "";
                string MRUserIds = "";
                var plantData = _plantRepository.GetById((int)(entity.PlantId ?? 0));
                if (plantData != null)
                {
                    DMRUserIds = plantData.DMRUserIds ?? "";
                    MRUserIds = plantData.MRUserIds ?? "";
                }

                string basePath = _hostingEnvironment.ContentRootPath;
                string filePath = "EmailTemplate/CPR_Notification.template";
                string path = Path.Combine(basePath, filePath);

                var template = File.ReadAllText(path);

                var requester = _emp.GetById(entity.CreatedBy?.ToString());
                var requesterEmail = requester?.OfficalEmpEmailID;
                var submittedBy = requester?.EmployeeName ?? "User";

                string statusColor = statusText switch
                {
                    "Approved" => "#28a745",
                    "Rejected" => "#dc3545",
                    "Awaiting Approval" => "#ffc107",
                    _ => "#6c757d"
                };

                string remarksSection = "";
                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    remarksSection = $@"
<tr>
<td style='padding:10px 0;'>
<b>Remarks:</b>
<div style='margin-top:5px;background:#fff3f3;border-left:4px solid #dc3545;padding:10px;border-radius:4px;font-size:13px;'>
{remarks}
</div>
</td>
</tr>";
                }

                string viewUrl = _configuration["WebUrl"]
                                 + "CPR/Edit/" + entity.Id;

                template = template
                    .Replace("#CPRNumber#", entity.CPRUniqueCode)
                    .Replace("#CPRTitle#", entity.DepartmentTitle)
                    .Replace("#SubmittedBy#", submittedBy)
                    .Replace("#Status#", statusText)
                    .Replace("#STATUSCOLOR#", statusColor)
                    .Replace("#ActionDate#", actionDate.ToString("dd-MMM-yyyy HH:mm"))
                    .Replace("#ApproverName#", approverName ?? "-")
                    .Replace("#RemarksSection#", remarksSection)
                    .Replace("#ViewLink#", viewUrl)
                    .Replace("#Year#", DateTime.Now.Year.ToString());

                string subject = $"CPR {entity.CPRUniqueCode} - {statusText}";

                var toEmails = new List<string>();
                var ccEmails = new List<string>();

                // ===============================
                // 🔹 Fetch Reviewers (ProcessOwners)
                // ===============================
                var reviewerIds = (entity.ProcessOwners ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(a => a != "")
                    .Select(x => x.Trim())
                    .ToList();

                var reviewerEmails = _emp.GetAllActive()
                    .Where(x => reviewerIds.Contains(x.Id.ToString()))
                    .Select(x => x.OfficalEmpEmailID)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                // ===============================
                // 🔹 Fetch Approvers (RelevantVerticalHeads)
                // ===============================
                var approverIds = (entity.RelevantVerticalHeads ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(a => a != "")
                    .Select(x => x.Trim())
                    .ToList();

                var approverEmails = _emp.GetAllActive()
                    .Where(x => approverIds.Contains(x.Id.ToString()))
                    .Select(x => x.OfficalEmpEmailID)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                // ===============================
                // 🔹 Fetch DMR / MR Emails
                // ===============================
                var dmrMrIds = ((DMRUserIds ?? "") + "," + (MRUserIds ?? ""))
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(a => a != "")
                    .Select(x => x.Trim())
                    .ToList();

                var dmrMrEmails = _emp.GetAllActive()
                    .Where(x => dmrMrIds.Contains(x.Id.ToString()))
                    .Select(x => x.OfficalEmpEmailID)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                // Send to Admin also
                var admins = _emp.GetAllActive()
.Where(x => x.OrgRole == "Admin")
.ToList();   // bring to memory first

                var adminEmails = admins
                    .Where(x =>
                        !string.IsNullOrEmpty(x.PlantIds) &&
                        x.PlantIds.Split(',')
                                  .Any(p => p.Trim() == entity.PlantId.ToString()))
                    .Select(x => x.OfficalEmpEmailID)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                // ======================================================
                // 🔵 CASE 1: Level 1 Awaiting Approval
                // ======================================================
                if (statusText == "Awaiting Approval"
                    && entity.StageId == Constants.CPRStage.Level_I)
                {
                    // TO → Reviewer
                    toEmails.AddRange(reviewerEmails);

                    // CC → Requestor + Approver + Site Admin
                    if (!string.IsNullOrEmpty(requesterEmail))
                        ccEmails.Add(requesterEmail);

                    ccEmails.AddRange(approverEmails);
                    ccEmails.AddRange(adminEmails);
                }


                // ======================================================
                // 🟢 CASE 2: Level 2 Awaiting Approval
                // ======================================================
                else if (statusText == "Awaiting Approval"
                         && entity.StageId == Constants.CPRStage.Level_II)
                {
                    // TO → Approver
                    toEmails.AddRange(approverEmails);

                    // CC → Requestor + Reviewer + DMR/MR + Site Admin
                    if (!string.IsNullOrEmpty(requesterEmail))
                        ccEmails.Add(requesterEmail);

                    ccEmails.AddRange(reviewerEmails);
                    ccEmails.AddRange(dmrMrEmails);
                    ccEmails.AddRange(adminEmails);
                }

                // ============================
                // 🟢 CASE 2: Approved - Level 1
                // ============================
                else if (statusText == "Approved"
                         && entity.StageId == Constants.CPRStage.Level_I)
                {
                    if (!string.IsNullOrEmpty(requesterEmail))
                        toEmails.Add(requesterEmail);
                }
                // ============================
                // 🟢 CASE 3: Final Approved - Level 2
                // ============================
                else if (statusText == "Approved"
                         && entity.StageId == Constants.CPRStage.Level_II)
                {
                    if (!string.IsNullOrEmpty(requesterEmail))
                        toEmails.Add(requesterEmail);


                    toEmails.AddRange(adminEmails);

                    if (!string.IsNullOrEmpty(requesterEmail))
                        ccEmails.Add(requesterEmail);
                }
                // ============================
                // 🔴 CASE 4: Rejected
                // ============================
                else if (statusText == "Rejected")
                {
                    if (!string.IsNullOrEmpty(requesterEmail))
                        toEmails.Add(requesterEmail);
                }
                // ============================
                // 🔴 CASE 5: FinalApproved
                // ============================
                else if (statusText == "CPR Closed" || statusText == "Released")
                {
                    // TO → Requestor
                    if (!string.IsNullOrEmpty(requesterEmail))
                        toEmails.Add(requesterEmail);

                    // CC → Reviewer + Approver + DMR/MR + Site Admin
                    ccEmails.AddRange(reviewerEmails);
                    ccEmails.AddRange(approverEmails);
                    ccEmails.AddRange(dmrMrEmails);
                    ccEmails.AddRange(adminEmails);
                    try
                    {
                        SendFinalReleaseMail(entity);
                    }
                    catch (Exception ex)
                    {
                        // log error but continue with email sending
                        Log.Error("SendCPRNotificationEmail Failed", $"Status: {statusText}, Error: {ex.Message}");
                    }
                }
                else if (statusText == "QMS Rejected")
                {
                    // TO → Requestor
                    if (!string.IsNullOrEmpty(requesterEmail))
                        toEmails.Add(requesterEmail);

                    // CC → Reviewer + Approver + DMR/MR + Site Admin
                    ccEmails.AddRange(reviewerEmails);
                    ccEmails.AddRange(approverEmails);
                    ccEmails.AddRange(dmrMrEmails);
                    ccEmails.AddRange(adminEmails);
                }
                

                if (toEmails.Any())
                {
                    _emailHelper.SendEmail(
                        string.Join(",", toEmails.Distinct()),
                        subject,
                        template, null,
                        ccEmails.Distinct().ToList());
                }

            }
            catch (Exception ee)
            {
                // log error
                Log.Error("SendCPRNotificationEmail Failed", ee);
            }
        }


        private void SendFinalReleaseMail(CPRMasterEntity entity)
        {
            try
            {
                string basePath = _hostingEnvironment.ContentRootPath;
                string filePath = "EmailTemplate/CPR_FinalRelease.template";
                string path = Path.Combine(basePath, filePath);

                var template = File.ReadAllText(path);

                string portalLink = _configuration["WebUrl"];

                // Changes requested by Dayana on Jun 5, 2026, 1:15 PM mail and discussed on call.
                // Final Release mail recipients are now maintained plant-wise in adm_Plant.
                // If To/CC are not configured for any plant, existing logic will continue:
                // mail will be sent to all active employees mapped with that plant.

                var plantData = _plantRepository.GetById((int)(entity.PlantId ?? 0));

                var configuredToEmails = (plantData?.FinalReleaseToEmails ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

                var configuredCcEmails = (plantData?.FinalReleaseCcEmails ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

                List<string> plantEmployees = new List<string>();

                bool hasConfiguredMail = configuredToEmails.Any() || configuredCcEmails.Any();

                if (!hasConfiguredMail)
                {
                    // Existing logic fallback: Get all active employees of that plant
                    plantEmployees = _emp.GetAllActive().ToList()
                        .Where(x => !string.IsNullOrEmpty(x.OfficalEmpEmailID)
                                    && ((x.PlantId != null && x.PlantId == entity.PlantId)
                                    || (!string.IsNullOrEmpty(x.PlantIds) &&
                                        x.PlantIds
                                            .Split(",")
                                            .Any(pid => pid.Trim() == entity.PlantId.ToString()))
                                    ))
                        .Select(x => x.OfficalEmpEmailID)
                        .Distinct()
                        .ToList();
                }

                var catId = (int)(entity.CategoryId ?? 0);
                var catdata = _categoryRepository.GetById(catId);
                var cat = catdata != null ? catdata.Name : "";

                var docId = (int)(entity.DocumentId ?? 0);
                var docData = _documentRepository.GetById(docId);
                var doc = docData != null ? docData.DocumentNumber : entity.ManualDocumentNo;
                var docName = docData != null ? docData.Name : entity.DepartmentTitle;

                template = template
                    .Replace("#CompanyName#", "TASL")
                    .Replace("#CPRNumber#", entity.CPRUniqueCode)
                    .Replace("#ReleaseDate#", DateTime.Now.ToString("dd-MM-yyyy"))
                    .Replace("#Department#", entity.Department ?? "")
                    .Replace("#DocCategory#", cat ?? "")
                    .Replace("#DocNumber#", doc ?? "")
                    .Replace("#DocName#", docName ?? "")
                    .Replace("#RevisionNo#", entity.Revision ?? "")
                    .Replace("#PortalLink#", portalLink);

                string subject = $"Document Released - {entity.CPRUniqueCode}";

                if (hasConfiguredMail)
                {
                    if (configuredToEmails.Any())
                    {
                        _emailHelper.SendEmail(
                            string.Join(",", configuredToEmails),
                            subject,
                            template,
                            null,
                            configuredCcEmails);
                    }
                    else
                    {
                        Log.Error("FinalReleaseMail Failed",
                            $"FinalReleaseToEmails is empty but FinalReleaseCcEmails is configured for PlantId: {entity.PlantId}");
                    }
                }
                else if (plantEmployees.Any())
                {
                    _emailHelper.SendEmail(
                        "",
                        subject,
                        template,
                        plantEmployees);
                }
            }
            catch (Exception ex)
            {
                // log error
                Log.Error("FinalReleaseMail Failed", ex);
            }
        }
        //private void SendFinalReleaseMail(CPRMasterEntity entity)
        //{
        //    try
        //    {
        //        string basePath = _hostingEnvironment.ContentRootPath;
        //        string filePath = "EmailTemplate/CPR_FinalRelease.template";
        //        string path = Path.Combine(basePath, filePath);

        //        var template = File.ReadAllText(path);

        //        string portalLink = _configuration["WebUrl"];

        //        // Get all employees of that plant
        //        var plantEmployees = _emp.GetAllActive().ToList()
        //            .Where(x => !string.IsNullOrEmpty(x.OfficalEmpEmailID)
        //                        && ((x.PlantId != null && x.PlantId == entity.PlantId)
        //                        || (!string.IsNullOrEmpty(x.PlantIds) &&
        // x.PlantIds
        //    .Split(",")
        //    .Any(pid => pid.Trim() == entity.PlantId.ToString()))
        //    ))
        //            .Select(x => x.OfficalEmpEmailID)
        //            .Distinct()
        //            .ToList();


        //        var catId = (int)(entity.CategoryId ?? 0);
        //        var catdata = _categoryRepository.GetById(catId);
        //        var cat = catdata != null ? catdata.Name : "";

        //        var DocId = (int)(entity.DocumentId ?? 0);
        //        var docData = _documentRepository.GetById(DocId);
        //        var doc = docData != null ? docData.DocumentNumber : entity.ManualDocumentNo;
        //        var docName = docData != null ? docData.Name : "";

        //        template = template
        //            .Replace("#CompanyName#", "TASL")
        //            .Replace("#CPRNumber#", entity.CPRUniqueCode)
        //            .Replace("#ReleaseDate#", DateTime.Now.ToString("dd-MM-yyyy"))
        //            .Replace("#Department#", entity.DepartmentTitle ?? "")
        //            .Replace("#DocCategory#", cat ?? "")
        //            .Replace("#DocNumber#", doc ?? "")
        //            .Replace("#DocName#", docName ?? "")
        //            .Replace("#RevisionNo#", entity.Revision ?? "")
        //            .Replace("#PortalLink#", portalLink);

        //        string subject = $"Document Released - {entity.CPRUniqueCode}";

        //        if (plantEmployees.Any())
        //        {
        //            _emailHelper.SendEmail(
        //                "",
        //                subject,
        //                template, plantEmployees);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // log error
        //        Log.Error("FinalReleaseMail Failed", ex);
        //    }
        //}

    }
}
