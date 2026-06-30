using System;

namespace CQSAirborne.Domain
{
    /// <summary>
    /// Detail: adm_CPRMasterApproverDetails
    /// </summary>
    public class CPRMasterApproverDetailEntity : BaseAuditableEntity
    {
        public long Id { get; set; }
        public long? CPRMasterId { get; set; }

        public long? UserId { get; set; }
        public long? StageId { get; set; }
        public long? ApprovalStatusId { get; set; }   // Pending / Approved / Rejected lookup
        public DateTime? StatusDateTime { get; set; }
        public int? ApprovalLevel { get; set; }

        public bool IsActive { get; set; }
        public bool? IsMailSend { get; set; }
        public string ApproverRemarks { get; set; }

        // Navigations
        public virtual CPRMasterEntity CprMaster { get; set; }
        //public virtual EmployeeEntity User { get; set; }
        //public virtual GroupCodeEntity CPRApprovalStatus { get; set; }
    }
}
