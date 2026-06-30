using CQSAirborne.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class DocumentHistoryMainModel : BaseModel
    {
        public DocumentHistoryMainModel()
        {
            Plants = new List<PlantSelectListModel>();
        }
        public int DocumentId { get; set; }
        public List<PlantSelectListModel> Plants { get; set; }
    }
}
