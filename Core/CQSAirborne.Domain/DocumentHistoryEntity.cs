using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class DocumentHistoryEntity : BaseAuditableEntity
    {
        public DocumentHistoryEntity()
        {
            DocumentPlantMaps = new List<DocumentPlantMapHistoryEntity>();
        }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime UploadDate { get; set; }
        public string RevisionNumber { get; set; }

        public DateTime? InActiveDate { get; set; }
        public DateTime? ActualInActiveDate { get; set; }

        public string Alias { get; set; }
        public string ProcessOwner { get; set; }
        public int DocumentTypeId { get; set; }
        public long? CommonPictureId { get; set; }

        public string ReasonForChange { get; set; }
        public string CPRNumber { get; set; }
        public string Remarks { get; set; }
        public long? CPRMasterId { get; set; }
        public virtual CPRMasterEntity CPRMasterEntity { get; set; }
        public virtual GroupCodeEntity DocumentType { get; set; }
        public virtual PictureEntity CommonPicture { get; set; }
        public virtual CategoryEntity Category { get; set; }
        public virtual ICollection<DocumentPlantMapHistoryEntity> DocumentPlantMaps { get; set; }

    }
}
