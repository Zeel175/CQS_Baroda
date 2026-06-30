using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Dashboard
{
    public class ClickablePathModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public bool IsLastElement { get; set; }
        public string DocFullName { get; set; }
    }
}
