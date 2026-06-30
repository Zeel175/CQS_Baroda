using CQSAirborne.Model.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class DocumentChartModel : BaseModel
    {
        public DocumentChartModel()
        {
        }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string PrimaryCategoryName { get; set; }
        public int DocumentCount { get; set; }
        public string CategoryFullName { get; set; }
    }
}
