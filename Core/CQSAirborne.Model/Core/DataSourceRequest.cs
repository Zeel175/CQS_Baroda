using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Core
{
    public class DataSourceRequest
    {
        public DataSourceRequest()
        {
            Columns = new List<DataColumnRequest>();
            Order = new List<DataOrderRequest>();
        }
        public int Draw { get; set; }
        public List<DataColumnRequest> Columns { get; set; }
        public List<DataOrderRequest> Order { get; set; }
        public DataSearchRequest Search { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string? Operator { get; set; } = "";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int StatusId { get; set; }
        public int DocumentTypeId { get; set; }
        public string? DocTypePrefix { get; set; } = "";
        public DateTime RevisionDate { get; set; }
        public string? CustomFilter { get; set; } = "";
        public class DataColumnRequest
        {
            public string? Data { get; set; } = "";
            public string? Name { get; set; } = "";
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }
            public DataSearchRequest Search { get; set; }

        }

        public class DataSearchRequest
        {
            public string? Value { get; set; } = "";
            public bool Regex { get; set; }
        }

        public class DataOrderRequest
        {
            public int Column { get; set; }
            public string? Dir { get; set; } = "";
        }
    }
}
