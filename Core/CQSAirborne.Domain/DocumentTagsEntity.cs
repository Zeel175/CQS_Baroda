using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class DocumentTagsEntity : BaseEntity
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string DocumentTag { get; set; }
        public virtual DocumentEntity Document { get; set; }
     
    }
}
