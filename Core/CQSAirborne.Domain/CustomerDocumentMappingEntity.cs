using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class CustomerDocumentMappingEntity : BaseAuditableEntity
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public int DocumentId { get; set; }
        public long PictureId { get; set; }
        public string Path { get; set; }
        public string GeneratedPath { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedLink { get; set; }
        public virtual CustomerEntity Customer { get; set; }
        public virtual PictureEntity Picture { get; set; }
        public virtual DocumentEntity Documents { get; set; }
    }
}
