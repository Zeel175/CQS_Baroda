using CQSAirborne.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Dashboard
{
    public class QuickSearchRequestModel : DataSourceRequest
    {
        public string DbColumn { get; set; }
        public string SearchValue { get; set; }
    }
}
