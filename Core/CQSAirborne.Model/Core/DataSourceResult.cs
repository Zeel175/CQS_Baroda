using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Core
{
    public class DataSourceResult
    {
        public object Data { get; set; }
        public int Draw { get; set; }
        public int RecordsFiltered { get; set; }
        public int RecordsTotal { get; set; }
    }
}
