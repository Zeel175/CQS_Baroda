using CQSAirborne.Model.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Services.API.Models
{
    public class LoginResponse
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpireAt { get; set; }
        public string OrgRole { get; set; }
        public string UserName { get; set; }
        public string EmpName { get; set; }
        public List<AssignPermissionViewModel> Permissions { get; set; }
    }
}
