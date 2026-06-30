using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class UserEntity : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PasswordHash { get; set; }
        public string EmployeeId { get; set; }
        public string Email { get; set; }
    }
}
