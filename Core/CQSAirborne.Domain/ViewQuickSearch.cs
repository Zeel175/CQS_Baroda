using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class ViewQuickSearch : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CompareText { get; set; }
        public string DbName { get; set; }
        public bool CanDownload { get; set; }
        public string DocCode { get; set; }
        public string DocUniqueNo { get; set; }
        public string RevisionNumber { get; set; }
        public string PlantName { get; set; }
        public bool IsActive { get; set; }
        public int? DocumentTypeId { get; set; }
        public int? PlantId { get; set; }
    }
}
