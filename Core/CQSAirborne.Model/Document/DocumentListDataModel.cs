using CQSAirborne.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class DocumentListDataModel : BaseModel
    {
        public DocumentListDataModel()
        {
            Plants = new List<PlantSelectListModel>();
            DocTypePrefix = "";
        }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int StatusId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocTypePrefix { get; set; }
        public DateTime RevisionDate { get; set; }
        public List<PlantSelectListModel> Plants { get; set; }
    }
}
