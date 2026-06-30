using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class CodeMaintainEntity : BaseEntity
    {
        public int Id { get; set; }
        public string ModuleName { get; set; }
        public int LastNumber { get; set; }
        public string Prefix { get; set; }
        public int Padding { get; set; }
    }
}
