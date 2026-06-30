using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CQSAirborne.Model.CPRMaster
{
    public class CPRMasterModel : BaseValidateModel
    {
        /// <summary>
        /// DTO for CPRMasterEntity (adm_CPRMaster)
        /// </summary>
        public long Id { get; set; }
        public string CPRUniqueCode { get; set; }
        [Required(ErrorMessage = "Please Select Category")]
        public long? CategoryId { get; set; }
        //[Required(ErrorMessage = "Please Select Document")]
        public long? DocumentId { get; set; }
        [Required(ErrorMessage = "Please Select Requested Date")]
        public DateTime RequestedDate { get; set; }
        
        [Required(ErrorMessage = "Please Select Plant")]
        public int? PlantId { get; set; }
        public string Plant { get; set; }
        public string CPRNo { get; set; }
        public string DepartmentTitle { get; set; }
        public string Department { get; set; }
        public string Revision { get; set; }
        public long? RequestedById { get; set; }
        public string Program { get; set; }
        public string ActionRequested { get; set; }
        public string CPRRaisedDueTo { get; set; }
        public string SectionRequiringChange { get; set; }
        public string ChangeDescription { get; set; }
        public string ReasonForChange { get; set; }
        [DisplayName("Approved by")]
        public string RelevantVerticalHeads { get; set; }
        
        [DisplayName("Reviewed by")]
        public string ProcessOwners { get; set; }
        [Required(ErrorMessage = "Please select Stage")]
        public long? StageId { get; set; }
        [Required(ErrorMessage = "Please select Status")]
        public long? StatusId { get; set; }
        public long? AdminStatusId { get; set; }
        public DateTime? AdminStatusDate { get; set; }
        public string QMSStatus { get; set; }
        public DateTime? EDC { get; set; }
        public DateTime? ADC { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public string AdminRemarks { get; set; }
        public string DocumentFilePath { get; set; }
        public string DocumentFilePath1 { get; set; }
        public long? FinalApprovedById { get; set; }
        public bool IsClosed { get; set; } = false;
        public DateTime? ClosedDate { get; set; }
        public string ManualDocumentNo { get; set; }

        // Child collections
        public List<CPRMasterApproverDetailModel> ApprovalsList { get; set; } = new List<CPRMasterApproverDetailModel>();

        // Optional display fields (populate via joins/projection)
        public string CategoryName { get; set; }
        public string DocumentNo { get; set; }
        public string DocumentTitle { get; set; }
        public string RequestedByName { get; set; }
        public string StatusName { get; set; }
        public string RelevantVerticalHeadNames { get; set; }
        public string ProcessOwnerNames { get; set; }
        public string StageName { get; set; }
        // in CPRMasterModel
        public string SessionId { get; set; }
        public string RequestedBy { get; set; }
        public string FormattedRequestedDate
        {
            get
            {
                string formatDate = "";
                if (RequestedDate != null && RequestedDate != DateTime.MinValue)
                {
                    formatDate = RequestedDate.ToString("dd-MMM-yyyy");
                }
                return formatDate;
            }
        }
    }

    public class CPRStageStatusUpdateModel
    {
        public long CPRId { get; set; }                 // which CPR
        public int RequestedStatusId { get; set; }      // one of Constants.CPRStatusType (Awaiting_Approval / Approved / Rejected)
        public string Remarks { get; set; }            // optional comment
        public string AdminRemarks { get; set; }
        public bool IsFromAdminSide { get; set; } = false;
        public bool IsClosed { get; set; } = false;
        public DateTime? EDC { get; set; }
        public DateTime? ADC { get; set; }
    }

    public class NonStandardCategoryModel
    {
        public int Id { get; set; }
        public int? PrimaryCategoryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class CPRApprovalDetailsModel
    {
        public long Id { get; set; }
        public long CPRMasterId { get; set; }
        public string EmployeeName { get; set; }
        public string Approver { get; set; }
        public string Status { get; set; }          // Approval Status (GroupCode)
        public string Stage { get; set; }           // Stage Name (GroupCode)
        public bool IsActive { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsMailSend { get; set; }
        public string ApproverRemarks { get; set; }
        public long? StageId { get; set; }
        public long? ApprovalStatusId { get; set; }
        public DateTime? StatusDateTime { get; set; }
    }

}
