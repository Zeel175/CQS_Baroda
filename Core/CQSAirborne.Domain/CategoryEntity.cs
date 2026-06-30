using System.Collections.Generic;

namespace CQSAirborne.Domain
{
    public class CategoryEntity : BaseAuditableEntity
    {
        public CategoryEntity()
        {
            SecondaryCategories = new List<CategoryEntity>();
            Documents = new List<DocumentEntity>();
            DocumentHistorys = new List<DocumentHistoryEntity>();
            this.IsRestricted = false;
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryTypeId { get; set; }
        public int? PrimaryCategoryId { get; set; }
        public string Remark { get; set; }
        public bool IsAvailableForDownload { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRestricted { get; set; }
        public virtual CategoryEntity PrimaryCategory { get; set; }
        public virtual ICollection<CategoryEntity> SecondaryCategories { get; set; }
        public virtual ICollection<DocumentEntity> Documents { get; set; }
        public virtual ICollection<DocumentHistoryEntity> DocumentHistorys { get; set; }
        public virtual GroupCodeEntity CategoryType { get; set; }

        //public virtual ICollection<CPRMasterEntity> CRPMasterEntity { get; set; }
    }
}
