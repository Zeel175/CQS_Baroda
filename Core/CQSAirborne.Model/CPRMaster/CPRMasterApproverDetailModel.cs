
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CQSAirborne.Model.CPRMaster
{
    public class CPRMasterApproverDetailModel : BaseValidateModel
    {
        /// <summary>
        /// DTO for CPRMasterApproverDetailEntity (adm_CPRMasterApproverDetails)
        /// </summary>

        public long Id { get; set; }
        public long? CPRMasterId { get; set; }
        public long? UserId { get; set; }
        public long? StageId { get; set; }
        public long? ApprovalStatusId { get; set; }
        public DateTime? StatusDateTime { get; set; }
        public int? ApprovalLevel { get; set; }
        public bool IsActive { get; set; }
        public bool? IsMailSend { get; set; }
        public string ApproverRemarks { get; set; }

        // Optional display fields
        public string ApproverName { get; set; }
        public string ApprovalStatusName { get; set; } // from GroupCode/lookup
    }
}
