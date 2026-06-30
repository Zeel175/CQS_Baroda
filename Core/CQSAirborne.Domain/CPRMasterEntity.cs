using System;
using System.Collections.Generic;

namespace CQSAirborne.Domain
{
    /// <summary>
    /// Header: adm_CPRMaster
    /// </summary>
    public class CPRMasterEntity : BaseAuditableEntity
    {
        public CPRMasterEntity()
        {
            ApprovalsEntity = new List<CPRMasterApproverDetailEntity>();
        }

        public long Id { get; set; }
        public string CPRUniqueCode { get; set; }
        public long? CategoryId { get; set; }
        public long? DocumentId { get; set; }
        public int? PlantId { get; set; }
        public DateTime? RequestedDate { get; set; }
        public string CPRNo { get; set; }
        public string DepartmentTitle { get; set; }
        public string Department { get; set; }
        public string Revision { get; set; }
        public long? RequestedById { get; set; }
        public string Program { get; set; }

        /// <summary>From PDF: New Release / Revision / Obsolete / Others (store as text or CSV/JSON)</summary>
        public string ActionRequested { get; set; }

        /// <summary>From PDF: CIB change / Improvement / Audit output / Others</summary>
        public string CPRRaisedDueTo { get; set; }

        public string SectionRequiringChange { get; set; }
        public string ChangeDescription { get; set; }
        public string ReasonForChange { get; set; }

        /// <summary>Comma-separated names/IDs as per your UX; can normalize later.</summary>
        public string RelevantVerticalHeads { get; set; }
        public string ProcessOwners { get; set; }
        public string AdminRemarks { get; set; }
        public long? StageId { get; set; }
        public long? StatusId { get; set; }
        public DateTime? EDC { get; set; }
        public DateTime? ADC { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public string DocumentFilePath { get; set; }

        public bool IsClosed { get; set; } = false;
        public DateTime? ClosedDate { get; set; }
        public long? AdminStatusId { get; set; }
        public DateTime? AdminStatusDate { get; set; }
        public string ManualDocumentNo { get; set; }
        public long? FinalApprovedById { get; set; }
        // Navigations
        public virtual ICollection<CPRMasterApproverDetailEntity> ApprovalsEntity { get; set; }
        public virtual ICollection<DocumentEntity>  DocumentEntities { get; set; }
        public virtual ICollection<DocumentHistoryEntity> DocumentHistoryEntities { get; set; }
        // Optional: link to a user entity if you have one
        //public virtual EmployeeEntity RequestedByEntity { get; set; }
        //public virtual DocumentEntity DocumentEntity { get; set; }
        //public virtual CategoryEntity CategoryEntity { get; set; }
    }
}
