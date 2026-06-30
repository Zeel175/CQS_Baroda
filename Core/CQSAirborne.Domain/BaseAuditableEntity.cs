using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
