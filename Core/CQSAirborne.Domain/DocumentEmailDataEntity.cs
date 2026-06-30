using System;
using System.Collections.Generic;

namespace CQSAirborne.Domain
{
    public class DocumentEmailDataEntity : BaseAuditableEntity
    {
       
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string SpecificPersonEmail { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentName { get; set; }
        public string DocumentNumber { get; set; }
        public string RevisionNumber { get; set; }
        public string ProcessOwner { get; set; }
        public int DocumentTypeId { get; set; }
        public bool IsMailSend { get; set; }
        public bool IsActive { get; set; }
    }
}
