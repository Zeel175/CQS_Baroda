using CQSAirborne.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Dashboard
{
    public class ViewDocumentModel : BaseModel
    {
        public ViewDocumentModel()
        {
            ClickablePath = new List<ClickablePathModel>();
        }
        public int DocumentId { get; set; }
        public DocumentOperationType DocumentOperationType { get; set; }
        public List<ClickablePathModel> ClickablePath { get; set; }
    }
}
