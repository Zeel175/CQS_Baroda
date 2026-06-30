using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Dashboard
{
    public class PlantDocumentListModel : BaseModel
    {
        public int Id { get; set; }
        public int? PlantTableId { get; set; }
        public int PlantId { get; set; }
        public string PlantName { get; set; }
        public string PlantAlias { get; set; }
        public bool CanDownload { get; set; }
        public string DocumentDisplayName { get; set; }
        public int DisplayOrder { get; set; }
    }
}
