using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CQSAirborne.Model.CPRMaster
{
    public class CPRPrintModel : BaseValidateModel
    {
        // Header & meta
        // header
        public long Id { get; set; }
        public string CPRUniqueCode { get; set; }
        public string CPRNo { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CategoryName { get; set; }
        public string DocumentNo { get; set; }
        public string DepartmentTitle { get; set; }
        public string Department { get; set; }
        public string Revision { get; set; }
        public string Program { get; set; }
        public string ActionRequested { get; set; }
        public string CPRRaisedDueTo { get; set; }
        public bool IsNewRelease { get; set; }
        public bool IsRevision { get; set; }
        public bool IsObsolete { get; set; }
        public bool IsOtherAction { get; set; }

        // Raised due to (tick-boxes)
        public bool DueToCibChange { get; set; }
        public bool DueToImprovement { get; set; }
        public bool DueToAuditOutput { get; set; }
        public bool DueToOther { get; set; }
        public string SectionRequiringChange { get; set; }
        public string ChangeDescription { get; set; }
        public string ReasonForChange { get; set; }
        public string Remarks { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public string DocumentFilePath { get; set; }
        public int? StageId { get; set; }
        public string StageName { get; set; }
        public DateTime? EDC { get; set; }
        public DateTime? ADC { get; set; }
        public int? RequestedById { get; set; }
        public string RequestedByName { get; set; }
        public string RelevantVerticalHeadNames { get; set; }
        public string ProcessOwnerNames { get; set; }

        public long? AdminStatusId { get; set; }
        public DateTime? AdminStatusDate { get; set; }
        public string QMSAdminStatus { get; set; }
        public string QMSAdmin { get; set; }
        public DateTime? ClosedDate { get; set; }
        // Attachments
        public List<(string DisplayName, string FileName, string DownloadUrl)> Attachments { get; set; } = new List<(string DisplayName, string FileName, string DownloadUrl)>();

        // approvals
        public List<CprApprovalRow> Level1Approvals { get; set; } = new List<CprApprovalRow>();
        public List<CprApprovalRow> Level2Approvals { get; set; } = new List<CprApprovalRow>();
    }


    public class CprHeaderRow
    {
        public long Id { get; set; }
        public string CPRUniqueCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CPRNo { get; set; }
        public DateTime? RequestedDate { get; set; }
        public string CategoryName { get; set; }
        public string DocumentNo { get; set; }
        public string DepartmentTitle { get; set; }
        public string Department { get; set; }
        public string Revision { get; set; }
        public string Program { get; set; }
        public string ActionRequested { get; set; }
        public string CPRRaisedDueTo { get; set; }
        public string SectionRequiringChange { get; set; }
        public string ChangeDescription { get; set; }
        public int? StatusId { get; set; }
        public long? AdminStatusId { get; set; }
        public string QMSAdminStatus { get; set; }
        public string StatusName { get; set; }
        public int? StageId { get; set; }
        public string StageName { get; set; }
        public DateTime? EDC { get; set; }
        public DateTime? ADC { get; set; }
        public int? RequestedById { get; set; }
        public string ReasonforChange { get; set; }
        public string RequestedByName { get; set; }
        public string RelevantVerticalHeadNames { get; set; }
        public string ProcessOwnerNames { get; set; }
        public string DocumentFilePath { get; set; }
        public string QMSAdmin { get; set; }
        public DateTime? ClosedDate { get; set; }
        public DateTime? AdminStatusDate { get; set; }
    }

    public class CprApprovalRow
    {
        public long Id { get; set; }
        public long CPRMasterId { get; set; }
        public long? UserId { get; set; }

        public string ApproverName { get; set; }
        public int StageId { get; set; }          // 14 or 15
        public string Stage { get; set; }         // stage name from GroupCode
        public int? ApprovalStatusId { get; set; }
        public string ApprovalStatusName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? StatusDateTime { get; set; }
        public string ApproverRemarks { get; set; }
        public string ApprovedByUser { get; set; } // from ModifiedBy join
    }
}
