using System;
using System.Collections.Generic;

namespace CQSAirborne.Domain
{
    public class DocumentEntity : BaseAuditableEntity
    {
        public DocumentEntity()
        {
            DocumentPlantMaps = new List<DocumentPlantMapEntity>();
            DocumentTags = new List<DocumentTagsEntity>();
        }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime UploadDate { get; set; }
        public string RevisionNumber { get; set; }
        public string Alias { get; set; }
        public string ProcessOwner { get; set; }
        public int DocumentTypeId { get; set; }
        public bool IsActive { get; set; }
        public long? CommonPictureId { get; set; }

        public string ReasonForChange { get; set; }
        public string CPRNumber { get; set; }
        public long? CPRMasterId { get; set; }
        public string Remarks { get; set; }

        public virtual GroupCodeEntity DocumentType { get; set; }
        public virtual CategoryEntity Category { get; set; }
        public virtual PictureEntity CommonPicture { get; set; }
        public virtual CPRMasterEntity CPRMasterEntity { get; set; }
        public virtual ICollection<DocumentPlantMapEntity> DocumentPlantMaps { get; set; }
        public virtual ICollection<DocumentTagsEntity> DocumentTags { get; set; }

        public virtual ICollection<CustomerDocumentMappingEntity> CustomerDocuments { get; set; }
        //public virtual ICollection<CPRMasterEntity> CRPMasterEntity { get; set; }

    }
}
