using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class EmailHistoryEntity : BaseEntity
    {
        public int Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string BccEmail { get; set; }
        public string CCEmail { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool IsSuccess { get; set; } = false;
        public string ErrorMessage { get; set; }
        public DateTime? SentOn { get; set; }
    }
}
