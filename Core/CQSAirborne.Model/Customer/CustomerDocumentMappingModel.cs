using CQSAirborne.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Customer
{
    public class CustomerDocumentMappingModel : BaseValidateModel
    {
        public CustomerDocumentMappingModel()
        {
        }
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public int DocumentId { get; set; }
        public long PictureId { get; set; }
        public string Path { get; set; }
        public string GeneratedPath { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }

        public string DocumentName { get; set; }
        public string AttachmentName { get; set; }
        public string CreatedLink { get; set; }
    }
}
