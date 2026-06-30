using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Dashboard
{
    public class DashboardModel : BaseModel
    {
        public DashboardModel()
        {
            ClickablePath = new List<ClickablePathModel>();
        }
        public int CategoryId { get; set; }
        public string NavigationLink { get; set; }
        public List<ClickablePathModel> ClickablePath { get; set; }
    }

}
