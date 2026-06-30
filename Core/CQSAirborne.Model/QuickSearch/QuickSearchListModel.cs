using CQSAirborne.Model.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.QuickSearch
{
    public class QuickSearchListModel : BaseModel
    {
        public QuickSearchListModel()
        {
            ClickablePath = new List<ClickablePathModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string CompareText { get; set; }
        public string NavigationLink { get; set; }
        public List<ClickablePathModel> ClickablePath { get; set; }
        public string Type { get; set; }
        public bool CanDownload { get; set; }

        public string DocCode { get; set; }
        public string DocUniqueNo { get; set; }
        public string RevisionNumber { get; set; }
        public string PlantName { get; set; }
        public int? DocumentTypeId { get; set; }
    }
}
