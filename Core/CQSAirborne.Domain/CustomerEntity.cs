using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class CustomerEntity : BaseAuditableEntity
    {
        public CustomerEntity()
        {
            CustomerDocumentMappings = new List<CustomerDocumentMappingEntity>();
        }
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<CustomerDocumentMappingEntity> CustomerDocumentMappings { get; set; }
    }
}
