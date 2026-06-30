using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class PictureEntity : BaseAuditableEntity
    {
        public PictureEntity()
        {
            Plants = new List<PlantEntity>();
            DocumentPlants = new List<DocumentPlantMapEntity>();
            DocumentPlantHistories = new List<DocumentPlantMapHistoryEntity>();
            Documents = new List<DocumentEntity>();
            DocumentHistories = new List<DocumentHistoryEntity>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string DisplayName { get; set; }


        public virtual ICollection<PlantEntity> Plants { get; set; }
        public virtual ICollection<DocumentPlantMapEntity> DocumentPlants { get; set; }
        public virtual ICollection<DocumentPlantMapHistoryEntity> DocumentPlantHistories { get; set; }
        public virtual ICollection<DocumentEntity> Documents { get; set; }
        public virtual ICollection<DocumentHistoryEntity> DocumentHistories { get; set; }
        public virtual ICollection<CustomerDocumentMappingEntity> CustomerDocuments { get; set; }
    }
}
