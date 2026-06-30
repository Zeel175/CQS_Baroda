using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class GroupCodeEntity : BaseAuditableEntity
    {
        public GroupCodeEntity()
        {
            Categories = new List<CategoryEntity>();
            DocumentTypeWiseDocuments = new List<DocumentEntity>();
            DocumentTypeWiseDocumentHistories = new List<DocumentHistoryEntity>();
            //Permissions = new List<PermissionEntity>();
        }

        public int Id { get; set; }
        public string ModuleName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<PermissionEntity> Permissions { get; set; }
        public virtual ICollection<CategoryEntity> Categories { get; set; }

        public virtual ICollection<DocumentEntity> DocumentTypeWiseDocuments { get; set; }
        public virtual ICollection<DocumentHistoryEntity> DocumentTypeWiseDocumentHistories { get; set; }
        //public virtual ICollection<CPRMasterApproverDetailEntity> CPRMasterApproverDetailEntity { get; set; }
    }
}
