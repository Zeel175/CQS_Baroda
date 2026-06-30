using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model
{
    public class LoginResult
    {
        public LoginResult()
        {
            Claims = new Dictionary<string, string>();
        }
        public long Id { get; set; }
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public IDictionary<string, string> Claims { get; set; }
        public string OrgRole { get; set; }
        public string EmpName { get; set; }
    }


}
