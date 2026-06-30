using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class PlantEntity : BaseAuditableEntity
    {
        public PlantEntity()
        {
            DocumentPlantMaps = new List<DocumentPlantMapEntity>();
            DocumentPlantMapHistories = new List<DocumentPlantMapHistoryEntity>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string PinCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string WebSite { get; set; }
        public long? LogoId { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsCPREnable { get; set; } = false;
        public string DMRUserIds { get; set; }
        public string MRUserIds { get; set; }

        public string FinalReleaseToEmails { get; set; }
        public string FinalReleaseCcEmails { get; set; }
        public virtual PictureEntity Logo { get; set; }

        public virtual ICollection<DocumentPlantMapEntity> DocumentPlantMaps { get; set; }
        public virtual ICollection<DocumentPlantMapHistoryEntity> DocumentPlantMapHistories { get; set; }
    }
}
